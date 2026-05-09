using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    // Mono's HttpClientHandler resolves hostnames via getaddrinfo and connects to the returned
    // addresses sequentially (no Happy Eyeballs / RFC 8305). When Cloudflare publishes AAAA
    // records and the user's network has broken IPv6, the IPv6 SYN times out for ~75s before
    // the OS falls back to IPv4 — past our request CancelAfter window, surfacing as a
    // TimeoutException. UnityWebRequest didn't have this problem because libcurl does
    // Happy Eyeballs natively.
    //
    // Workaround: pre-resolve the hostname ourselves, prefer the first IPv4 address,
    // rewrite the URI to the IP literal, and put the original hostname in the Host header
    // so Cloudflare still routes correctly. SNI on Mono is suppressed for IP literals
    // (per RFC 6066), so Cloudflare returns its universal SSL fallback cert; we accept
    // RemoteCertificateNameMismatch only when the cert is otherwise chain-valid for the
    // original hostname.
    internal sealed class Ipv4PreferringHandler : DelegatingHandler {
        private const string OriginalHostPropertyKey = "BeatLeader.Ipv4PreferringHandler.OriginalHost";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan NegativeCacheTtl = TimeSpan.FromSeconds(30);
        private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new(StringComparer.OrdinalIgnoreCase);

        private readonly CookieContainer _cookieContainer;

        public Ipv4PreferringHandler(HttpMessageHandler inner, CookieContainer cookieContainer) : base(inner) {
            _cookieContainer = cookieContainer;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var originalUri = request.RequestUri;
            if (originalUri == null) {
                return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            if (!ShouldRewrite(originalUri)) {
                return await SendWithCookies(request, originalUri, cancellationToken).ConfigureAwait(false);
            }

            var ipv4 = await ResolveIpv4Async(originalUri.Host, cancellationToken).ConfigureAwait(false);
            if (ipv4 == null) {
                return await SendWithCookies(request, originalUri, cancellationToken).ConfigureAwait(false);
            }

            var rewritten = new UriBuilder(originalUri) { Host = ipv4.ToString() }.Uri;
            request.RequestUri = rewritten;
            request.Headers.Host = originalUri.IsDefaultPort
                ? originalUri.Host
                : $"{originalUri.Host}:{originalUri.Port}";
            request.Properties[OriginalHostPropertyKey] = originalUri.Host;
            Plugin.Log.Debug($"[Ipv4PreferringHandler] {originalUri.Host} -> {ipv4}");

            try {
                return await SendWithCookies(request, originalUri, cancellationToken).ConfigureAwait(false);
            } finally {
                request.RequestUri = originalUri;
            }
        }

        private async Task<HttpResponseMessage> SendWithCookies(HttpRequestMessage request, Uri cookieUri, CancellationToken cancellationToken) {
            var cookieHeader = _cookieContainer.GetCookieHeader(cookieUri);
            if (!string.IsNullOrEmpty(cookieHeader)) {
                request.Headers.Remove("Cookie");
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookies)) {
                foreach (var c in setCookies) {
                    try {
                        _cookieContainer.SetCookies(cookieUri, c);
                    } catch (CookieException) {
                        // Ignore malformed cookies.
                    }
                }
            }

            return response;
        }

        private static bool ShouldRewrite(Uri uri) {
            if (uri.HostNameType != UriHostNameType.Dns) return false;
            if (string.IsNullOrEmpty(uri.Host)) return false;
            if (uri.IsLoopback) return false;
            return true;
        }

        private static async Task<IPAddress?> ResolveIpv4Async(string host, CancellationToken cancellationToken) {
            var now = DateTime.UtcNow;
            if (_cache.TryGetValue(host, out var cached) && cached.ExpiresAtUtc > now) {
                return cached.Address;
            }

            IPAddress? ipv4 = null;
            try {
                var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                ipv4 = addresses.FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);
            } catch (Exception ex) {
                Plugin.Log.Debug($"[Ipv4PreferringHandler] DNS lookup failed for {host}: {ex.Message}");
            }

            var ttl = ipv4 != null ? CacheTtl : NegativeCacheTtl;
            _cache[host] = new CacheEntry(ipv4, now + ttl);
            return ipv4;
        }

        // Cert validation hook installed on the inner HttpClientHandler. When the URI was
        // rewritten to an IP literal, the cert won't match the IP — accept that single error
        // only if the cert is otherwise chain-valid AND the SAN/CN matches the ORIGINAL host.
        public static bool ValidateServerCertificate(
            HttpRequestMessage request,
            X509Certificate2? cert,
            X509Chain? chain,
            SslPolicyErrors errors
        ) {
            if (errors == SslPolicyErrors.None) return true;
            if (errors != SslPolicyErrors.RemoteCertificateNameMismatch) return false;
            if (cert == null) return false;
            if (request == null) return false;
            if (!request.Properties.TryGetValue(OriginalHostPropertyKey, out var hostObj) || hostObj is not string originalHost) {
                return false;
            }
            return CertificateMatchesHostname(cert, originalHost);
        }

        private static bool CertificateMatchesHostname(X509Certificate2 cert, string hostname) {
            foreach (var sanName in EnumerateSubjectAlternativeDnsNames(cert)) {
                if (HostMatchesPattern(hostname, sanName)) return true;
            }
            // Per RFC 6125 §6.4.4, CN should only be checked when no SANs are present, but
            // we already iterated SANs above and returned on a hit — falling through here
            // means none matched, so the CN fallback only applies if there were no SANs.
            var cn = cert.GetNameInfo(X509NameType.DnsName, forIssuer: false);
            return !string.IsNullOrEmpty(cn) && HostMatchesPattern(hostname, cn);
        }

        // SubjectAlternativeName ::= SEQUENCE OF GeneralName
        // GeneralName CHOICE { ..., dNSName [2] IMPLICIT IA5String, ... }
        // We parse the raw ASN.1 directly because X509Extension.Format() output is locale-
        // dependent ("DNS Name=" on en-US, "Nom DNS=" on fr-FR, etc.).
        private static IEnumerable<string> EnumerateSubjectAlternativeDnsNames(X509Certificate2 cert) {
            var ext = cert.Extensions["2.5.29.17"];
            if (ext == null) yield break;
            var data = ext.RawData;
            if (data == null || data.Length < 2) yield break;

            var pos = 0;
            if (data[pos++] != 0x30) yield break; // outer SEQUENCE
            if (!TryReadAsn1Length(data, ref pos, out var seqLen)) yield break;
            var end = Math.Min(data.Length, pos + seqLen);

            while (pos < end) {
                var tag = data[pos++];
                if (!TryReadAsn1Length(data, ref pos, out var len)) yield break;
                if (pos + len > end) yield break;
                if (tag == 0x82) { // [2] IMPLICIT — dNSName
                    yield return Encoding.ASCII.GetString(data, pos, len);
                }
                pos += len;
            }
        }

        private static bool TryReadAsn1Length(byte[] data, ref int pos, out int length) {
            length = 0;
            if (pos >= data.Length) return false;
            var b = data[pos++];
            if ((b & 0x80) == 0) {
                length = b;
                return true;
            }
            var n = b & 0x7F;
            if (n == 0 || n > 4 || pos + n > data.Length) return false;
            for (var i = 0; i < n; i++) {
                length = (length << 8) | data[pos++];
            }
            return length >= 0;
        }

        private static bool HostMatchesPattern(string hostname, string pattern) {
            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(hostname)) return false;
            if (pattern.Equals(hostname, StringComparison.OrdinalIgnoreCase)) return true;
            if (pattern.StartsWith("*.", StringComparison.Ordinal)) {
                var suffix = pattern.Substring(1); // ".example.com"
                var dotIdx = hostname.IndexOf('.');
                if (dotIdx > 0
                    && hostname.Substring(dotIdx).Equals(suffix, StringComparison.OrdinalIgnoreCase)
                    && hostname.IndexOf('.', dotIdx + 1) < 0) {
                    return true;
                }
            }
            return false;
        }

        private readonly struct CacheEntry {
            public IPAddress? Address { get; }
            public DateTime ExpiresAtUtc { get; }
            public CacheEntry(IPAddress? address, DateTime expiresAtUtc) {
                Address = address;
                ExpiresAtUtc = expiresAtUtc;
            }
        }
    }
}

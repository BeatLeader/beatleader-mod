using BeatLeader.Utils;
using BeatLeader.WebRequests;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader {
    public class WebSocketClient : IDisposable
    {
        private ClientWebSocket _webSocket;
        private readonly Uri _serverUri;
        private readonly ConcurrentQueue<(byte[] data, WebSocketMessageType type)> _sendQueue = new();
        private readonly SemaphoreSlim _sendSignal = new(0, int.MaxValue);
        private CancellationTokenSource _cts;
        private Task _senderTask;

        public WebSocketClient(string serverUri)
        {
            _serverUri = new Uri(serverUri);
        }

        public bool IsAlive() =>
            _webSocket != null && _webSocket.State == WebSocketState.Open;

        public async Task ConnectAsync(CancellationToken token = default)
        {
            _webSocket = CreateSocket();
            await _webSocket.ConnectAsync(_serverUri, token);
            _cts = new CancellationTokenSource();
            _senderTask = Task.Run(() => SenderLoop(_cts.Token));
        }

        private ClientWebSocket CreateSocket()
        {
            var ws = new ClientWebSocket();
            var container = new CookieContainer();
            var cookies = WebRequestFactory.CookieContainer.GetCookies(new Uri(BLConstants.BEATLEADER_API_URL));
            var sb = new StringBuilder();
            foreach (Cookie c in cookies) {
                if (sb.Length > 0) sb.Append("; ");
                sb.Append($"{c.Name}={c.Value}");
            }

            var cookie = sb.ToString();

            container.SetCookies(_serverUri, cookie);
            ws.Options.Cookies = container;
            return ws;
        }

        public void QueueBinary(byte[] data)
        {
            _sendQueue.Enqueue((data, WebSocketMessageType.Binary));
            _sendSignal.Release();
        }

        public void QueueText(string message)
        {
            _sendQueue.Enqueue((Encoding.UTF8.GetBytes(message), WebSocketMessageType.Text));
            _sendSignal.Release();
        }

        private int _consecutiveFailures;

        private async Task SenderLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try { await _sendSignal.WaitAsync(token); }
                catch (OperationCanceledException) { break; }

                while (_sendQueue.TryDequeue(out var msg))
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        if (!IsAlive()) await ReconnectWithBackoffAsync(token);
                        if (IsAlive())
                        {
                            await _webSocket.SendAsync(
                                new ArraySegment<byte>(msg.data),
                                msg.type,
                                true,
                                token);
                            _consecutiveFailures = 0;
                        }
                    }
                    catch (OperationCanceledException) { break; }
                    catch (Exception e)
                    {
                        _consecutiveFailures++;
                        if (_consecutiveFailures <= 3) {
                            Plugin.Log.Error($"WebSocket send failed (attempt {_consecutiveFailures}): {e.Message}");
                        }
                        if (_consecutiveFailures >= 5) {
                            Plugin.Log.Error("Too many consecutive WebSocket failures, stopping sender.");
                            while (_sendQueue.TryDequeue(out _)) { }
                            return;
                        }
                        try { await ReconnectWithBackoffAsync(token); } catch { }
                    }
                }
            }
        }

        private async Task ReconnectWithBackoffAsync(CancellationToken token)
        {
            var delayMs = Math.Min(1000 * (1 << _consecutiveFailures), 30000);
            await Task.Delay(delayMs, token);

            try { _webSocket?.Dispose(); } catch { }
            _webSocket = CreateSocket();
            await _webSocket.ConnectAsync(_serverUri, token);
        }

        public void Dispose()
        {
            _cts?.Cancel();
            try { _senderTask?.Wait(TimeSpan.FromSeconds(2)); } catch { }
            _cts?.Dispose();

            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None)
                            .Wait(TimeSpan.FromSeconds(2));
                    }
                    catch { }
                }
                _webSocket.Dispose();
            }
        }
    }
}

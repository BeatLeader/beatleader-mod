using System;
using System.Threading.Tasks;
using Steamworks;

namespace BeatLeader.API {
    internal static class Authentication {
        public static async Task<string> SteamTicket() {
            if (!SteamManager.Initialized) {
                Plugin.Log.Error("SteamManager is not initialized!");
                return null;
            }

            var getAuthResponseTcs = new TaskCompletionSource<bool>();
            var getAuthSessionTicketCallback = new Callback<GetAuthSessionTicketResponse_t>(resp => getAuthResponseTcs.TrySetResult(resp.m_eResult == EResult.k_EResultOK));

            string ticket = null;

            const int ticketBufferSize = 1024;
            var byteBuffer = new byte[ticketBufferSize];
            if (SteamUser.GetAuthSessionTicket(byteBuffer, ticketBufferSize, out var ticketLength) != HAuthTicket.Invalid) {
                ticket = BitConverter.ToString(byteBuffer, 0, (int) ticketLength).Replace("-", "");
            }

            await Task.WhenAny(getAuthResponseTcs.Task, Task.Delay(TimeSpan.FromSeconds(10.0))).ConfigureAwait(false);

            getAuthSessionTicketCallback.Dispose();
            return !getAuthResponseTcs.Task.IsCompleted || !getAuthResponseTcs.Task.Result ? null : ticket;
        }
    }
}
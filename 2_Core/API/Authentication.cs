using System;
using System.Threading.Tasks;
using Steamworks;
using Oculus.Platform;
using Oculus.Platform.Models;

namespace BeatLeader.API {

    internal static class Authentication {

        public static Task<string> PlatformTicket(string platform) {
            return platform switch {
                "steam" => SteamTicket(),
                "oculus" => OculusTicket(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

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
                ticket = BitConverter.ToString(byteBuffer, 0, (int)ticketLength).Replace("-", "");
            }

            await Task.WhenAny(getAuthResponseTcs.Task, Task.Delay(TimeSpan.FromSeconds(10.0))).ConfigureAwait(false);

            getAuthSessionTicketCallback.Dispose();
            return !getAuthResponseTcs.Task.IsCompleted || !getAuthResponseTcs.Task.Result ? null : ticket;
        }

        public static async Task<string> OculusTicket() {
            TaskCompletionSource<string> tcs = new();
            Users.GetUserProof().OnComplete(delegate (Message<UserProof> message) {
                tcs.TrySetResult(message.IsError ? null : message.Data.Value);
            });
            return await tcs.Task;
        }
    }
}
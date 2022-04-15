using Steamworks;
using System;
using System.Threading.Tasks;

namespace BeatLeader.API {
    class Authentication {
        public static async Task<string> SteamTicket() {
            if (!SteamManager.Initialized) {
                Plugin.Log.Error("SteamManager is not initialized!");
                return null;
            }

            TaskCompletionSource<bool> getAuthResponseTcs = new TaskCompletionSource<bool>();
            Callback<GetAuthSessionTicketResponse_t> getAuthSessionTicketCallback = new Callback<GetAuthSessionTicketResponse_t>((Callback<GetAuthSessionTicketResponse_t>.DispatchDelegate)(resp => getAuthResponseTcs.TrySetResult(resp.m_eResult == EResult.k_EResultOK)));

            string? ticket = null;

            byte[]? m_Ticket = new byte[1024];
            uint m_pcbTicket;
            if (SteamUser.GetAuthSessionTicket(m_Ticket, 1024, out m_pcbTicket) != HAuthTicket.Invalid) {
                Array.Resize(ref m_Ticket, (int)m_pcbTicket);
                System.Text.StringBuilder sb = new();
                foreach (byte b in m_Ticket)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                ticket = sb.ToString();
            }

            Task task = await Task.WhenAny(new Task[2]
            {
                (Task) getAuthResponseTcs.Task,
                Task.Delay(TimeSpan.FromSeconds(10.0))
            });

            getAuthSessionTicketCallback.Dispose();
            return !getAuthResponseTcs.Task.IsCompleted || !getAuthResponseTcs.Task.Result ? null : ticket;
        }
    }
}

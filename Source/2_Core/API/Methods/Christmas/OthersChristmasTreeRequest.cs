using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class OthersChristmasTreeRequest : PersistentSingletonRequestHandler<OthersChristmasTreeRequest, ChristmasTreeSettings> {
        public static void SendRequest(string playerId) {
            var descriptor = new JsonGetRequestDescriptor<ChristmasTreeSettings>($"{BLConstants.BEATLEADER_API_URL}/projecttree/{playerId}");
            Instance.Send(descriptor);
        }
    }
}
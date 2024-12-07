using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class ChristmasTreeRequest : PersistentSingletonRequestHandler<ChristmasTreeRequest, ChristmasTreeSettings> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<ChristmasTreeSettings>($"{BLConstants.BEATLEADER_API_URL}/projecttree");
            Instance.Send(descriptor);
        }
    }
}
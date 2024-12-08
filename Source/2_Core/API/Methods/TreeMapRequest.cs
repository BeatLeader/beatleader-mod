using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class TreeMapRequest : PersistentSingletonRequestHandler<TreeMapRequest, TreeStatus> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<TreeStatus>($"{BLConstants.BEATLEADER_API_URL}/projecttree/status");
            Instance.Send(descriptor);
        }
    }
}

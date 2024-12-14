using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class TreeMapRequest : PersistentSingletonRequestHandler<TreeMapRequest, TreeStatus> {
        public static TreeStatus? treeStatus;
        private static bool addedListener = false;
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<TreeStatus>($"{BLConstants.BEATLEADER_API_URL}/projecttree/status");
            if (!addedListener) {
                AddStateListener(OnRequestStateChanged);
                addedListener = true;
            }

            Instance.Send(descriptor);
        }

        private static void OnRequestStateChanged(RequestState state, TreeStatus result, string failReason) {
            switch (state) {
                case RequestState.Finished:
                    treeStatus = result;
                    Plugin.Log.Error($"OnRequestStateChanged Finished {treeStatus}");
                    break;
                default: 
                    treeStatus = null;
                    Plugin.Log.Error($"OnRequestStateChanged default {treeStatus}");
                    break;
            }
        }
    }
}

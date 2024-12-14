using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;

namespace BeatLeader.API.Methods {
    internal class UploadTreeOrnamentsRequest : PersistentSingletonRequestHandler<UploadTreeOrnamentsRequest, string?> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/projecttree/ornaments";

        protected override bool KeepState => false;

        public static void SendRequest(ChristmasTreeOrnamentSettings[] ornaments) {
            var requestDescriptor = new JsonPostRequestDescriptor<string?>(Endpoint, JsonConvert.SerializeObject(ornaments));
            Instance.Send(requestDescriptor);
        }
    }
}
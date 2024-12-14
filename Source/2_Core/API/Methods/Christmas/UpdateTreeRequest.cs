using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.API.Methods {

    internal class UpdateTreeRequest : PersistentSingletonRequestHandler<UpdateTreeRequest, string?> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/projecttree/game";

        protected override bool KeepState => false;

        public static void SendRequest(FullSerializablePose pose) {
            var requestDescriptor = new JsonPostRequestDescriptor<string?>(Endpoint, JsonConvert.SerializeObject(pose));
            Instance.Send(requestDescriptor);
        }
    }
}

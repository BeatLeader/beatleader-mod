using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class UserRequest : PersistentSingletonRequestHandler<UserRequest, User> {
        // /user
        private const string Endpoint = BeatLeaderConstants.BEATLEADER_API_URL + "/user";

        public static void SendRequest() {
            var requestDescriptor = new JsonGetRequestDescriptor<User>(Endpoint);
            instance.Send(requestDescriptor);
        }
    }
}
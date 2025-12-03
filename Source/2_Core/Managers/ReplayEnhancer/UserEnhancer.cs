using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models.Replay;
using IPA.Utilities;
using OculusStudios.Platform.Core;
using UnityEngine;

namespace BeatLeader.Core.Managers.ReplayEnhancer
{
    class UserEnhancer
    {
        private static readonly FieldAccessor<PlatformLeaderboardsModel, IPlatform>.Accessor AccessPlatformUserModel;
        private static readonly object getUserLock = new object();

        static string userName = null;
        static string userID = null;
        static string userPlatform = null;

        private static UserInfo getUserTask;
        private static IPlatform _platformUserModel;

        static UserEnhancer()
        {
            try
            {
                AccessPlatformUserModel = FieldAccessor<PlatformLeaderboardsModel, IPlatform>.GetAccessor("_platform");
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error getting PlatformUserModel, GetUserInfo is unavailable: {ex.Message}");
                Plugin.Log.Debug(ex);
            }
        }

        public static void Enhance(Replay replay)
        {
            GetUser();
            replay.info.playerID = userID;
            replay.info.platform = userPlatform;
            replay.info.playerName = userName;
        }

        public static UserInfo GetUser()
        {
            try
            {
                lock (getUserLock)
                {
                    IPlatform platformUserModel = GetPlatformUserModel();
                    if (platformUserModel == null)
                    {
                        Plugin.Log.Error("IPlatformUserModel not found, cannot update user info.");
                        return null;
                    }
                    if (getUserTask == null)
                        getUserTask = InternalGetUser();
                }
                return getUserTask;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error retrieving UserInfo: {ex.Message}.");
                Plugin.Log.Debug(ex);
                throw;
            }
        }

        private static UserInfo InternalGetUser()
        {
            UserInfo userInfo = new UserInfo(_platformUserModel.key switch {
                "steam" => UserInfo.Platform.Steam,
                "oculus" => UserInfo.Platform.Oculus,
                "oculus-mock" => UserInfo.Platform.Oculus,
                "mock" => UserInfo.Platform.Test,
                _ => throw new NotImplementedException(),
            }, _platformUserModel.user.userId.ToString(), _platformUserModel.user.displayName);

            if (userInfo != null)
            {
                Plugin.Log.Debug($"UserInfo found: {userInfo.platformUserId}: {userInfo.userName} on {userInfo.platform}");
                userName = userInfo.userName;
                userID = userInfo.platformUserId;
                if (userInfo.platform == UserInfo.Platform.Steam)
                    userPlatform = "steam";
                else if (userInfo.platform == UserInfo.Platform.Oculus)
                    userPlatform = "oculuspc";
            }
            else
                throw new InvalidOperationException("UserInfo is null.");
            return userInfo;
        }

        internal static IPlatform GetPlatformUserModel()
        {
            if (_platformUserModel != null)
                return _platformUserModel;
            try
            {
                // Need to check for null because there's multiple PlatformLeaderboardsModels (at least sometimes), and one has a null IPlatformUserModel with 'vrmode oculus'
                var leaderboardsModel = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>().Where(p => AccessPlatformUserModel(ref p) != null).LastOrDefault();
                IPlatform platformUserModel = null;
                if (leaderboardsModel == null)
                {
                    Plugin.Log.Error("Could not find a 'PlatformLeaderboardsModel', GetUserInfo unavailable.");
                    return null;
                }
                if (AccessPlatformUserModel == null)
                {
                    Plugin.Log.Error("Accessor for 'PlatformLeaderboardsModel._platformUserModel' is null, GetUserInfo unavailable.");
                    return null;
                }

                platformUserModel = AccessPlatformUserModel(ref leaderboardsModel);
                _platformUserModel = platformUserModel;
            }
            catch (Exception ex)
            {
                Plugin.Log.Error($"Error getting 'IPlatformUserModel', GetUserInfo unavailable: {ex.Message}");
                Plugin.Log.Debug(ex);
            }
            return _platformUserModel;
        }
    }
}

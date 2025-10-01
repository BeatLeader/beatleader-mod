using BeatSaber.BeatAvatarSDK;
using UnityEngine;
using Random = System.Random;

namespace BeatLeader.Utils {
    internal static class AvatarUtils {
        public static readonly AvatarData DefaultAvatarData = new() {
            clothesId = "Hoodie",
            clothesPrimaryColor = Color.black,
            clothesSecondaryColor = Color.white,
            clothesDetailColor = Color.magenta
        };
        
        public static void RandomizeAvatarByPlayerId(string playerId, AvatarData avatarData, AvatarPartsModel avatarPartsModel) {
            var seed = playerId.GetHashCode();
            var random = new Random(seed);
            RandomizeAvatar(random, avatarData, avatarPartsModel);
        }
        
        public static void RandomizeAvatar(Random random, AvatarData avatarData, AvatarPartsModel avatarPartsModel) {
            RandomizeModels(random, avatarData, avatarPartsModel);
            RandomizeColors(random, avatarData);
        }

        private static void RandomizeModels(Random random, AvatarData avatarData, AvatarPartsModel avatarPartsModel) {
            avatarData.headTopId = RandomItem(random, avatarPartsModel.headTopCollection);
            avatarData.eyesId = RandomItem(random, avatarPartsModel.eyesCollection);
            avatarData.mouthId = RandomItem(random, avatarPartsModel.mouthCollection);
            avatarData.glassesId = RandomItem(random, avatarPartsModel.glassesCollection);
            avatarData.facialHairId = RandomItem(random, avatarPartsModel.facialHairCollection);
            avatarData.handsId = RandomItem(random, avatarPartsModel.handsCollection);
            avatarData.clothesId = RandomItem(random, avatarPartsModel.clothesCollection);

            static string RandomItem<T>(Random random, AvatarPartCollection<T> collection) where T : Object, IAvatarPart {
                return collection.GetByIndex(random.Next(0, collection.count)).id;
            }
        }

        private static void RandomizeColors(Random random, AvatarData avatarData) {
            avatarData.headTopPrimaryColor = RandomColor(random);
            avatarData.headTopSecondaryColor = RandomColor(random);
            avatarData.glassesColor = RandomColor(random);
            avatarData.facialHairColor = RandomColor(random);
            avatarData.handsColor = RandomColor(random);
            avatarData.clothesPrimaryColor = RandomColor(random);
            avatarData.clothesSecondaryColor = RandomColor(random);
            avatarData.clothesDetailColor = RandomColor(random);

            static Color RandomColor(Random random) {
                return ColorUtils.RandomColor(rand: random);
            }
        }
    }
}
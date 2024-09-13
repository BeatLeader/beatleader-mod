#nullable disable

using BeatSaber.BeatAvatarSDK;

namespace BeatLeader.Models {
    public class AvatarSettings {
        public AvatarPartSettings headTop;
        public AvatarPartSettings glasses;
        public AvatarPartSettings eyes;
        public AvatarPartSettings facialHair;
        public AvatarPartSettings mouth;
        public AvatarPartSettings hands;
        public AvatarPartSettings clothes;
        public string skinColorId;
        
        public AvatarData ToAvatarData() {
            return new AvatarData {
                headTopId = headTop.modelId,
                headTopPrimaryColor = headTop.primaryColor,
                headTopSecondaryColor = headTop.secondaryColor,
                glassesId = glasses.modelId,
                glassesColor = glasses.primaryColor,
                eyesId = eyes.modelId,
                facialHairId = facialHair.modelId,
                facialHairColor = facialHair.primaryColor,
                mouthId = mouth.modelId,
                handsId = hands.modelId,
                handsColor = hands.primaryColor,
                clothesId = clothes.modelId,
                clothesPrimaryColor = clothes.primaryColor,
                clothesSecondaryColor = clothes.secondaryColor,
                clothesDetailColor = clothes.detailColor,
                skinColorId = skinColorId
            };
        }

        public static AvatarSettings FromAvatarData(AvatarData data) {
            return new AvatarSettings {
                headTop = new() {
                    modelId = data.headTopId,
                    primaryColor = data.headTopPrimaryColor,
                    secondaryColor = data.headTopSecondaryColor
                },
                glasses = new() {
                    modelId = data.glassesId,
                    primaryColor = data.glassesColor
                },
                eyes = new() {
                    modelId = data.eyesId
                },
                facialHair = new() {
                    modelId = data.facialHairId,
                    primaryColor = data.facialHairColor
                },
                mouth = new() {
                    modelId = data.mouthId
                },
                hands = new() {
                    modelId = data.handsId,
                    primaryColor = data.handsColor
                },
                clothes = new() {
                    modelId = data.clothesId,
                    primaryColor = data.clothesPrimaryColor,
                    secondaryColor = data.clothesSecondaryColor,
                    detailColor = data.clothesDetailColor
                },
                skinColorId = data.skinColorId
            };
        }
    }
}
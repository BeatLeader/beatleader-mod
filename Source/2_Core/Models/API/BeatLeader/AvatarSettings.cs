#nullable disable

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

        private AvatarData? _avatarData;
        
        public AvatarData ToAvatarData() {
            _avatarData ??= new AvatarData {
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
            return _avatarData;
        }
    }
}
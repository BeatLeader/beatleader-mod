using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;
using Random = System.Random;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleAvatar : MonoBehaviour {
        #region Pool

        public class Pool : MonoMemoryPool<BattleRoyaleAvatar> {
            protected override void OnSpawned(BattleRoyaleAvatar item) {
                item.PresentAvatar();
            }

            protected override void OnDespawned(BattleRoyaleAvatar item) {
                item.HideAvatar();
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly EditAvatarFlowCoordinator _editAvatarFlowCoordinator = null!;
        [Inject] private readonly AvatarPartsModel _avatarPartsModel = null!;
        [Inject] private readonly DiContainer _diContainer = null!;

        #endregion

        #region Setup

        private AvatarTweenController _avatarTweenController = null!;
        private AvatarVisualController _avatarVisualController = null!;
        private readonly AvatarData _avatarData = new();

        public void Init(IReplayHeaderBase header) {
            var seed = header.ReplayInfo!.PlayerID.GetHashCode();
            var random = new Random(seed);
            RandomizeAvatar(random, _avatarData, _avatarPartsModel);
            _avatarVisualController.UpdateAvatarVisual(_avatarData);
        }

        private void Awake() {
            _avatarTweenController = InstantiateAvatar();
            _avatarVisualController = _avatarTweenController.GetComponentInChildren<AvatarVisualController>();
        }

        private AvatarTweenController InstantiateAvatar() {
            //TODO: asm pub
            var prefab = _editAvatarFlowCoordinator.GetField<AvatarTweenController, EditAvatarFlowCoordinator>("_avatarTweenController");
            var instance = Instantiate(prefab, transform, false);
            _diContainer.InjectGameObject(instance.gameObject);
            var trans = instance.transform;
            trans.localPosition = Vector3.zero;
            trans.localEulerAngles = Vector3.zero;
            return instance;
        }

        #endregion

        #region Enable & Disable

        public void PresentAvatar() {
            gameObject.SetActive(true);
            _avatarTweenController.gameObject.SetActive(true);
            _avatarTweenController.PresentAvatar();
        }

        public void HideAvatar() {
            _avatarTweenController.HideAvatar();
        }

        private void OnEnable() {
            PresentAvatar();
        }

        private void OnDisable() {
            HideAvatar();
        }

        #endregion

        #region RandomizeAvatar

        private static void RandomizeAvatar(Random random, AvatarData avatarData, AvatarPartsModel avatarPartsModel) {
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

        #endregion
    }
}
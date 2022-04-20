using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Movement;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Tools
{
    public class SimpleAvatarController : MonoBehaviour
    {
        [Inject] protected readonly AvatarPartsModel _avatarPartsModel;
        [Inject] protected readonly MovementManager _movementManager;

        protected AvatarDataFileManagerSO _avatarDataFileManager;
        protected GameObject _loadedHead;
        protected GameObject _loadedLeftHand;
        protected GameObject _loadedRightHand;
        protected GameObject _body;

        public GameObject body => _body;
        public GameObject head => _loadedHead;
        public GameObject leftHand => _loadedLeftHand;
        public GameObject rightHand => _loadedRightHand;

        public void Start()
        {
            _avatarDataFileManager = Resources.FindObjectsOfTypeAll<AvatarDataFileManagerSO>().First();
            Load(_avatarDataFileManager.Load());
            _loadedHead.transform.SetParent(_movementManager.head.transform);
        }
        public void Load(AvatarData data)
        {
            _loadedHead = new GameObject("Glasses");
            _loadedHead.AddComponent<MeshFilter>().mesh = GetPartByID(_avatarPartsModel.glassesCollection, data.glassesId).mesh;
        }
        private T GetPartByID<T>(AvatarPartCollection<T> collection, string id) where T : UnityEngine.Object, IAvatarPart
        {
            return collection.parts[collection.GetIndexById(id)];
        }
    }
}

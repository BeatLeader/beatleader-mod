using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Tools
{
    public class SimpleAvatarController : MonoBehaviour
    {
        [Inject] protected readonly AvatarPartsModel _avatarPartsModel;

        protected GameObject _loadedHead;
        protected GameObject _loadedLeftHand;
        protected GameObject _loadedRightHand;
        protected GameObject _body;

        public GameObject body => _body;
        public GameObject loadedHead => _loadedHead;
        public GameObject loadedLeftHand => _loadedLeftHand;
        public GameObject loadedRightHand => _loadedRightHand;

        public void Load(AvatarData data)
        {
            //_avatarPartsModel.
        }
    }
}

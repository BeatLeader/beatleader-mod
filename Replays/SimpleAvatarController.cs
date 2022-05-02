using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Movement;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class SimpleAvatarController : MonoBehaviour
    {
        [Inject] protected readonly BodyManager _bodyManager;

        protected AvatarTweenController _avatarTweenController;

        public void Start()
        {
            _avatarTweenController = Resources.FindObjectsOfTypeAll<AvatarTweenController>().First();
            StartCoroutine(_avatarTweenController.AppearAnimation());
            _avatarTweenController.transform.Find("PlayerAvatar/Head").SetParent(_bodyManager.head.transform, false);
        }
    }
}

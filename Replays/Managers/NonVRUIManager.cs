using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Forms;
using System.Windows.Forms;
using UnityEngine;

namespace BeatLeader.Replays.Managers
{
    public class NonVRUIManager : MonoBehaviour
    {
        public Form nonVRUI;

        public void Start()
        {
            nonVRUI = new NonVRReplayUI();
        }
        public void Update()
        {
            if (Input.GetMouseButtonDown(1)) nonVRUI.Show();
        }
    }
}

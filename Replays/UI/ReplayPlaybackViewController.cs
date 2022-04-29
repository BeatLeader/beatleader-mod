using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using BeatSaberMarkupLanguage;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.ReplayPlaybackUI.bsml")]
    public class ReplayPlaybackViewController : BSMLAutomaticViewController
    {
        public void Init()
        {

        }
    }
}

using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace BeatLeader
{
    [ViewDefinition("BeatLeader.9_Resources.BSML.ReplayPauseView.bsml")]
    [HotReload(RelativePathToLayout = @"..\9_Resources\BSML\ReplayPauseView.bsml")]
    class ReplayPauseViewController : BSMLAutomaticViewController
    {
        [UIComponent("testText")]
        TextMeshProUGUI testText;

        [UIAction("#post-parse")]
        public void PostParse()
        {

        }
    }
}

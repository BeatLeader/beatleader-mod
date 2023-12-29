using BeatLeader.Components;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.UI.Hub {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.Flows.BattleRoyaleFlow.Views.OpponentsView.BattleRoyaleOpponentsView.bsml")]
    internal class BattleRoyaleOpponentsViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region UI Components

        [UIComponent("opponents-list"), UsedImplicitly]
        private BattleRoyaleOpponentsList _opponentsList = null!;

        [UIComponent("opponents-list-scrollbar"), UsedImplicitly]
        private Scrollbar _opponentsListScrollbar = null!;
        
        #endregion

        #region Setup

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnInitialize() {
            _opponentsList.Setup(_battleRoyaleHost);
            _opponentsList.Scrollbar = _opponentsListScrollbar;
            _opponentsList.Refresh();
        }

        #endregion
    }
}
using HMUI;
using Reactive;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class PlayerProfileViewController : ViewController {
        [Inject] private readonly PlayerProfileContext _context = null!;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (firstActivation) {
                new ProfileOverviewPanel(_context).WithRectExpand().Use(transform);
            }
        }
    }
}

namespace BeatLeader.Components {
    internal class TabsContainer : LayoutComponentBase<TabsContainer> {
        public void SelectTab(int tabIndex) {
            for (var i = 0; i < ContentTransform!.childCount; i++) {
                ContentTransform!.GetChild(i).gameObject.SetActive(i == tabIndex);
            }
        }
    }
}
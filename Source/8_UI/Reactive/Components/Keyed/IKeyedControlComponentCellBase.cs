namespace BeatLeader.UI.Reactive.Components {
    internal interface IKeyedControlComponentCellBase<in TKey, in TParam> {
        void Init(TKey key, TParam param);
    }
}
namespace BeatLeader.UI.Reactive {
    internal interface ICopiable<in T> {
        void CopyFrom(T mod);
    }
}
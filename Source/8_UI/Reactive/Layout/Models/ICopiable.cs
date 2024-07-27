namespace BeatLeader.UI.Reactive {
    internal interface ICopiable<T> {
        void CopyFrom(T mod);
        T CreateCopy();
    }
}
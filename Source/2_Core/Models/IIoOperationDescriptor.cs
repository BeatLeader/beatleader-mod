namespace BeatLeader.Models {
    public interface IIoOperationDescriptor {
        byte[] Buffer { get; }

        void OnProgressChanged(long bytesRead, long totalBytes);
    }
}
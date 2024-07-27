namespace BeatLeader.Models {
    public interface IIoOperationDescriptor {
        byte[] Buffer { get; }
        long ContentSize { set; }
        
        void OnProgressChanged(long bytesRead, long totalBytes);
    }
}
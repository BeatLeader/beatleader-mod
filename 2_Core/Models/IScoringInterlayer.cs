
namespace BeatLeader.Models
{
    public interface IScoringInterlayer
    {
        T Convert<T>(ScoringData data) where T : ScoringElement;
    }
}

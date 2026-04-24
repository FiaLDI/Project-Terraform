using UnityEngine;

namespace Features.Multiplayer.SceneBinding
{
    public interface ISceneBoundView
    {
        string BoundType { get; }
        string BoundId { get; }
        string BoundKey { get; }
        GameObject GameObject { get; }
    }
}

using UnityEngine;

public interface IBuffTarget
{
    Transform Transform { get; }
    GameObject GameObject { get; }
    BuffSystem BuffSystem { get; }
}

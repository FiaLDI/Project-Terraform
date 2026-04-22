using UnityEngine;

namespace Features.Effects.Application
{
    public sealed class SpawnedObjectContext : MonoBehaviour
    {
        public GameObject Source { get; set; }
        public GameObject Target { get; set; }
        public float Lifetime { get; set; }
    }
}

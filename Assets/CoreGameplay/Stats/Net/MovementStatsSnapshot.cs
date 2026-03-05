using System;

namespace Features.Stats.Net
{
    [Serializable]
    public struct MovementStatsSnapshot
    {
        public float walk;
        public float sprint;
        public float crouch;
        public float rotation;

        public float gravity;
        public float jumpHeight;
    }
}
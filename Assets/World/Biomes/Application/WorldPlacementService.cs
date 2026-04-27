using Biomes.Data;
using Unity.Mathematics;
using UnityEngine;

namespace Biomes.Application
{
    public static class WorldPlacementService
    {
        public static bool TryFindReachablePoint(
            WorldConfig world,
            Vector3 origin,
            float minDistance,
            float maxDistance,
            int attempts,
            int salt,
            out Vector3 position)
        {
            position = default;

            if (world == null)
                return false;

            minDistance = Mathf.Max(0f, minDistance);
            maxDistance = Mathf.Max(minDistance + 1f, maxDistance);
            attempts = Mathf.Max(1, attempts);

            var random = new System.Random(world.seed ^ salt);
            float2 origin2 = new(origin.x, origin.z);

            for (int i = 0; i < attempts; i++)
            {
                float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
                float distance = Mathf.Lerp(minDistance, maxDistance, (float)random.NextDouble());
                float2 candidate2 = origin2 + new float2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;

                if (!IsWalkableSurface(world, candidate2))
                    continue;

                if (!HasSimpleWalkablePath(world, origin2, candidate2))
                    continue;

                float y = world.GetHeight(candidate2);
                position = new Vector3(candidate2.x, y + 1.25f, candidate2.y);
                return true;
            }

            float fallbackDistance = Mathf.Max(minDistance, world.safeSpawnBlendRadius + 10f);
            for (int i = 0; i < 16; i++)
            {
                float angle = (Mathf.PI * 2f * i) / 16f;
                float2 candidate2 = origin2 + new float2(Mathf.Cos(angle), Mathf.Sin(angle)) * fallbackDistance;

                if (!IsWalkableSurface(world, candidate2))
                    continue;

                if (!HasSimpleWalkablePath(world, origin2, candidate2))
                    continue;

                float y = world.GetHeight(candidate2);
                position = new Vector3(candidate2.x, y + 1.25f, candidate2.y);
                return true;
            }

            return false;
        }

        public static Vector3 SnapToGround(WorldConfig world, Vector3 position, float heightOffset = 1.25f)
        {
            if (world == null)
                return position;

            float2 xz = new(position.x, position.z);
            return new Vector3(position.x, world.GetHeight(xz) + heightOffset, position.z);
        }

        private static bool IsWalkableSurface(WorldConfig world, float2 point, bool requireOutsideSafeZone = true)
        {
            const float sampleRadius = 2.5f;
            const float maxHeightDelta = 3.5f;

            if (requireOutsideSafeZone && world.GetSafeSpawnFactor(point) < 0.7f)
                return false;

            float center = world.GetHeight(point);
            float h1 = world.GetHeight(point + new float2(sampleRadius, 0f));
            float h2 = world.GetHeight(point - new float2(sampleRadius, 0f));
            float h3 = world.GetHeight(point + new float2(0f, sampleRadius));
            float h4 = world.GetHeight(point - new float2(0f, sampleRadius));

            float min = Mathf.Min(center, h1, h2, h3, h4);
            float max = Mathf.Max(center, h1, h2, h3, h4);
            return max - min <= maxHeightDelta;
        }

        private static bool HasSimpleWalkablePath(WorldConfig world, float2 from, float2 to)
        {
            const float stepLength = 4f;
            const float maxStepHeight = 5f;

            float distance = math.distance(from, to);
            int steps = Mathf.Max(1, Mathf.CeilToInt(distance / stepLength));
            float previousHeight = world.GetHeight(from);

            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                float2 point = math.lerp(from, to, t);

                if (!IsWalkableSurface(world, point, requireOutsideSafeZone: false))
                    return false;

                float height = world.GetHeight(point);
                if (Mathf.Abs(height - previousHeight) > maxStepHeight)
                    return false;

                previousHeight = height;
            }

            return true;
        }
    }
}

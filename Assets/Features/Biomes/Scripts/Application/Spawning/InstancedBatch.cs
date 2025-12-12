using System.Collections.Generic;
using UnityEngine;
using Features.Biomes.Domain;

namespace Features.Biomes.Application
{
    public class InstancedBatch
    {
        public Mesh mesh;
        public Material material;

        public readonly List<Matrix4x4> matrices = new(1024);
        public readonly List<Color>     colors   = new(1024);
        public readonly List<float>     randoms  = new(1024);

        public InstancedBatch(Mesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
        }

        public void Clear()
        {
            matrices.Clear();
            colors.Clear();
            randoms.Clear();
        }
    }
}

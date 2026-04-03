using UnityEngine;

namespace Features.Enemy.Data
{
    [CreateAssetMenu(menuName = "Enemies/Render Config")]
    public class EnemyRenderConfigSO : ScriptableObject
    {
        [Header("LOD Prefabs")]
        public GameObject lod0Prefab;
        public GameObject lod1Prefab;
        public GameObject lod2Prefab;

        [Header("LOD Distances")]
        public float lod0Distance = 15f;
        public float lod1Distance = 40f;
        public float lod2Distance = 80f;

        [Header("Canvas")]
        public GameObject worldCanvasPrefab;
        public float canvasHideDistance = 30f;

        [Header("GPU Instancing")]
        public bool useGPUInstancing = true;
        public float instancingDistance = 120f;

        [Header("Animation")]
        public RuntimeAnimatorController animatorController;
    }
}

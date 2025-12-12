using UnityEngine;

namespace Features.Enemies
{
    /// <summary>
    /// Конфиг LOD/Instancing/Canvas для КОНКРЕТНОГО типа врага.
    /// Вешается на корень врага (там же где EnemyBehavior).
    /// </summary>
    public class EnemyLODConfig : MonoBehaviour
    {
        [Header("Renderers (можно оставить null — найдём автоматически)")]
        public Renderer lod0Renderer;   // ближний, hi-poly
        public Renderer lod1Renderer;   // средний
        public Renderer lod2Renderer;   // дальний / ultra-low poly

        [Header("LOD Distances")]
        public float lod0Distance = 15f;
        public float lod1Distance = 40f;
        public float lod2Distance = 80f;

        [Header("Canvas")]
        public Canvas worldCanvas;          // HP-бар врага
        public float canvasHideDistance = 30f;

        [Header("GPU Instancing")]
        public bool useGPUInstancing = true;
        public float instancingDistance = 120f;

        [Header("Доп. оптимизация")]
        public bool disableAnimatorInInstancing = true;
        public bool makeRigidbodyKinematicInInstancing = true;
    }
}

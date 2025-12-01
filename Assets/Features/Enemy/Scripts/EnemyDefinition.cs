using UnityEngine;

namespace Features.Enemies
{
    /// <summary>
    /// Описание типа врага:
    /// - какие у него префабы LOD0/LOD1/LOD2
    /// - на каких дистанциях переключать LOD
    /// Это висит на КОРНЕВОМ префабе врага.
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemyDefinition : MonoBehaviour
    {
        [Header("ID / Info (опционально)")]
        public string enemyId;         // Можно оставить пустым или задать уникальный ID
        public string displayName;     // Имя врага для UI / дебага
        public Sprite icon;            // Иконка, если понадобиться в UI

        [Header("LOD Prefabs")]
        [Tooltip("Префаб врага для ближней дистанции (полный модель / анимации)")]
        public GameObject prefabLOD0;

        [Tooltip("Префаб врага для средней дистанции (упрощённая модель)")]
        public GameObject prefabLOD1;

        [Tooltip("Префаб врага для дальней дистанции (ультра-лоу поли или billboard)")]
        public GameObject prefabLOD2;

        [Header("LOD Distances")]
        [Tooltip("До этой дистанции используем LOD0")]
        public float lod0Distance = 15f;

        [Tooltip("До этой дистанции используем LOD1. Дальше — LOD2/instancing")]
        public float lod1Distance = 40f;

        private void Reset()
        {
            // По умолчанию считаем, что базовый префаб и есть LOD0
            if (prefabLOD0 == null)
                prefabLOD0 = gameObject;
        }
    }
}

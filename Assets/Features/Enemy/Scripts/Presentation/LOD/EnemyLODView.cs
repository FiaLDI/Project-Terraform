using UnityEngine;
using System;

namespace Features.Enemy.Presentation.LOD
{
    public class EnemyLODView : MonoBehaviour
    {
        [SerializeField] private Transform anchor;
        [SerializeField] private GameObject[] lodPrefabs;

        private GameObject[] instances;
        private Animator animator;

        public event Action<GameObject> OnModelChanged;

        private void Awake()
        {
            if (anchor == null)
            {
                anchor = new GameObject("Anchor").transform;
                anchor.SetParent(transform);
                anchor.localPosition = Vector3.zero;
                anchor.localRotation = Quaternion.identity;
            }
        }

        public void Init(GameObject lod0, GameObject lod1, GameObject lod2)
        {
            if (lod0 == null || lod1 == null || lod2 == null)
            {
                Debug.LogWarning("[LOD] Invalid config, disabling LOD", this);
                enabled = false;
                return;
            }

            lodPrefabs = new[] { lod0, lod1, lod2 };

            instances = new GameObject[lodPrefabs.Length];

            for (int i = 0; i < lodPrefabs.Length; i++)
            {
                instances[i] = Instantiate(lodPrefabs[i], anchor);
                instances[i].SetActive(false);
            }
        }

        public void SetLOD(int lod)
        {
            if (instances == null || instances.Length == 0)
            {
                Debug.LogError("[LOD] No instances!", this);
                return;
            }

            lod = Mathf.Clamp(lod, 0, instances.Length - 1);

            for (int i = 0; i < instances.Length; i++)
                instances[i].SetActive(i == lod);
            
            if (!instances[lod].activeSelf)
                instances[lod].SetActive(true);

            var obj = instances[lod];

            if (obj == null)
            {
                Debug.LogError($"[LOD] Instance {lod} is null", this);
                return;
            }

            animator = obj.GetComponentInChildren<Animator>();

            OnModelChanged?.Invoke(obj);
        }

        public Animator GetAnimator() => animator;

        public GameObject GetLODObject(int lod) => instances[lod];
    }
}

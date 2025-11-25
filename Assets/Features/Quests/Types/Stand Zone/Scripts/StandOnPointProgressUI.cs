using UnityEngine;
using UnityEngine.UI;

namespace Quests
{
    public class StandOnPointProgressUI : MonoBehaviour
    {
        public Image fillImage;           // ссылка на fill
        public Transform targetPoint;     // точка на земле (QuestPoint)
        public float requiredTime = 2f;

        private float currentTime;
        private Camera cam;
        private Canvas canvas;

        public float CurrentTime => currentTime;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
        }

        public void SetCamera(Camera c)
        {
            cam = c;
            canvas.worldCamera = cam;
        }

        public void ResetTimer()
        {
            currentTime = 0f;
            UpdateUI();
        }

        public void Tick(float dt)
        {
            currentTime += dt;
            UpdateUI();
        }

        private void UpdateUI()
        {
            float p = Mathf.Clamp01(currentTime / requiredTime);
            fillImage.fillAmount = p;
        }

        private void LateUpdate()
        {
            if (targetPoint == null || cam == null) return;

            transform.position = targetPoint.position + Vector3.up * 2f;
            transform.LookAt(cam.transform);
        }
    }
}

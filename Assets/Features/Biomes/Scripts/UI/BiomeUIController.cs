using UnityEngine;
using TMPro;
using System.Collections;

namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(100)]
    public class BiomeUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI biomeLabel;   // постоянный текст (название биома)
        public TextMeshProUGUI biomePopup;   // всплывающее сообщение "Вы вошли в..."

        [Header("Popup Settings")]
        public float popupDuration = 1.5f;
        public float popupFadeDuration = 1f;

        [Header("Glow Settings")]
        [Tooltip("При каком 'уровне тумана' начинаем светиться (0..1)")]
        public float glowThreshold = 0.7f;
        [Tooltip("Максимальная толщина обводки при сильном тумане")]
        public float glowIntensity = 0.5f;

        private Coroutine popupRoutine;

        private BiomeFog fog;
        private Material labelMatInstance;

        private void Awake()
        {
            if (biomeLabel != null && biomeLabel.fontMaterial != null)
            {
                labelMatInstance = Instantiate(biomeLabel.fontMaterial);
                biomeLabel.fontMaterial = labelMatInstance;
            }

            fog = Object.FindAnyObjectByType<BiomeFog>();

            if (biomePopup != null)
            {
                var c = biomePopup.color;
                c.a = 0f;
                biomePopup.color = c;
                biomePopup.gameObject.SetActive(false);
            }
        }

        // ------------------------------
        // Простое обновление названия
        // ------------------------------
        public void SetBiome(string biomeName)
        {
            if (biomeLabel != null)
                biomeLabel.text = biomeName;
        }

        public void SetBiome(string biomeName, Color color)
        {
            if (biomeLabel != null)
            {
                biomeLabel.text = biomeName;
                biomeLabel.color = color;
            }
        }

        // ------------------------------
        // ВСПЛЫВАЮЩЕЕ СООБЩЕНИЕ
        // ------------------------------
        public void ShowPopup(string biomeName, Color color)
        {
            if (biomePopup == null)
                return;

            if (popupRoutine != null)
                StopCoroutine(popupRoutine);

            popupRoutine = StartCoroutine(PopupRoutine(biomeName, color));
        }

        private IEnumerator PopupRoutine(string biomeName, Color color)
        {
            if (biomePopup == null)
                yield break;

            biomePopup.gameObject.SetActive(true);
            biomePopup.text = $"Вы вошли в: {biomeName}";

            Color c = color;
            c.a = 1f;
            biomePopup.color = c;

            // Держим текст на экране
            yield return new WaitForSeconds(popupDuration);

            // Плавное исчезновение
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.0001f, popupFadeDuration);
                c.a = Mathf.Lerp(1f, 0f, t);
                biomePopup.color = c;
                yield return null;
            }

            biomePopup.gameObject.SetActive(false);
        }

        // ------------------------------
        // ГРАДИЕНТ + GLOW по туману
        // light  – цвет при слабом тумане
        // heavy  – цвет при сильном тумане
        // scale  – коэффициент (например, biome.fogGradientScale)
        // ------------------------------
        public void UpdateFogGradient(Color light, Color heavy, float scale)
        {
            if (biomeLabel == null)
                return;

            // Берём плотность тумана:
            // - если есть BiomeFog → его значение
            // - иначе RenderSettings.fogDensity
            float d;
            if (fog != null)
                d = fog.GetFogFactor();
            else
                d = RenderSettings.fogDensity;

            // Переводим в 0..1 с учётом scale
            float k = Mathf.Clamp01(d * scale);

            // Плавный переход цвета текста
            Color result = Color.Lerp(light, heavy, k);
            biomeLabel.color = result;

            // GLOW: при сильном тумане добавляем обводку
            if (labelMatInstance == null)
                return;

            if (k > glowThreshold)
            {
                float glow = (k - glowThreshold) / (1f - glowThreshold); // 0..1
                float width = glow * glowIntensity;                       // итоговая толщина

                labelMatInstance.SetFloat("_OutlineWidth", width);
                labelMatInstance.SetColor("_OutlineColor", Color.white);
            }
            else
            {
                // Убираем обводку
                labelMatInstance.SetFloat("_OutlineWidth", 0f);
            }
        }
    }
}

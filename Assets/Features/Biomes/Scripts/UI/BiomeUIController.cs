using UnityEngine;
using TMPro;
using System.Collections;


namespace Features.Biomes.Runtime.Visual
{
    [DefaultExecutionOrder(100)]
    public class BiomeUIController : MonoBehaviour
    {
        [Header("UI Elements")]
        public TextMeshProUGUI biomeLabel;   // постоянный текст
        public TextMeshProUGUI biomePopup;   // всплывающее сообщение

        [Header("Popup Settings")]
        public float popupDuration = 1.5f;
        public float popupFadeDuration = 1f;

        [Header("Glow Settings")]
        public float glowThreshold = 0.7f;   // при каком тумане начинаем светиться
        public float glowIntensity = 0.5f;

        private Coroutine popupRoutine;

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


        // ✅ Всплывающее сообщение "Вы вошли в..."
        public void ShowPopup(string biomeName, Color color)
        {
            if (popupRoutine != null)
                StopCoroutine(popupRoutine);

            popupRoutine = StartCoroutine(PopupRoutine(biomeName, color));
        }

        private IEnumerator PopupRoutine(string biomeName, Color color)
        {
            if (biomePopup == null)
                yield break;

            biomePopup.text = $"Вы вошли в: {biomeName}";
            Color c = color;
            c.a = 1f;
            biomePopup.color = c;

            // Ждём
            yield return new WaitForSeconds(popupDuration);

            // Плавное исчезновение
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / popupFadeDuration;
                c.a = Mathf.Lerp(1f, 0f, t);
                biomePopup.color = c;
                yield return null;
            }
        }

        // ✅ Градиент по туману
        public void UpdateFogGradient(Color light, Color heavy, float scale)
        {
            if (biomeLabel == null)
                return;

            float d = FindObjectOfType<BiomeFog>()?.GetFogFactor() ?? RenderSettings.fogDensity;

            float k = Mathf.Clamp01(d * scale);

            // Плавный переход цвета
            Color result = Color.Lerp(light, heavy, k);
            biomeLabel.color = result;

            // ✅ Glow эффект при сильном тумане
            if (k > glowThreshold)
            {
                float glow = (k - glowThreshold) / (1f - glowThreshold);
                biomeLabel.fontMaterial.SetFloat("_OutlineWidth", glow * glowIntensity);
                biomeLabel.fontMaterial.SetColor("_OutlineColor", Color.white);
            }
            else
            {
                biomeLabel.fontMaterial.SetFloat("_OutlineWidth", 0);
            }
        }
    }
}
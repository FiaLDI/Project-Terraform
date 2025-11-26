using UnityEngine;

public class OverloadPulseBehaviour : MonoBehaviour
{
    [Header("Visual Objects")]
    public Transform shockwave;
    public Transform flash;

    [Header("Settings")]
    public float duration = 0.35f;
    public float startScale = 0.2f;
    public float shockwaveScaleMultiplier = 4f;
    public float flashScaleMultiplier = 2f;

    private float _deathTime;
    private bool _initialized;

    private void Awake()
    {
        Debug.Log("[PulseFX] Awake on " + name, this);
        // Для отладки можно временно сделать чтобы эффект жил долго,
        // даже без Init:
        //_deathTime = Time.time + 999f;
        //_initialized = true;
    }

    public void Init(Transform owner, float radius, float fxDuration)
    {
        _initialized = true;

        duration = fxDuration;
        _deathTime = Time.time + duration;

        if (shockwave)
            shockwave.localScale = Vector3.one * startScale;

        if (flash)
            flash.localScale = Vector3.one * startScale;

        Debug.Log($"[PulseFX] Init. duration={duration}, deathTime={_deathTime}", this);
    }

    private void Update()
    {
        if (!_initialized)
            return; // НИЧЕГО не делаем до Init

        float t = 1f - ((_deathTime - Time.time) / duration);
        t = Mathf.Clamp01(t);

        if (shockwave)
        {
            float s = Mathf.Lerp(startScale, shockwaveScaleMultiplier, t);
            shockwave.localScale = Vector3.one * s;
        }

        if (flash)
        {
            float s2 = Mathf.Lerp(startScale, flashScaleMultiplier, t);
            flash.localScale = Vector3.one * s2;
        }

        if (Time.time >= _deathTime)
        {
            Debug.Log("[PulseFX] Destroy by lifetime", this);
            Destroy(gameObject);
        }
    }
}

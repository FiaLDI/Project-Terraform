using UnityEngine;
using System.Collections;

public class OverloadPulseBehaviour : MonoBehaviour
{
    [Header("References")]
    public Transform shockwaveRing;
    public Transform flash;
    public ParticleSystem sparks;

    [Header("Settings")]
    public float fxDuration = 0.35f;        // сколько живёт анимация шоквейва
    public float startScale = 0.2f;
    public float shockwaveScaleMultiplier = 4f;
    public float flashScaleMultiplier = 2f;

    private float _fxEndTime;
    private float _buffEndTime;

    private float _radius;
    private Transform _owner;

    private Material shockMat;
    private Material flashMat;
    private int fadeID;

    private bool fxFinished = false;

    private void Awake()
    {
        shockMat = shockwaveRing.GetComponent<Renderer>().material;
        flashMat = flash.GetComponent<Renderer>().material;
        fadeID = Shader.PropertyToID("_Fade");
    }

    /// <summary>
    /// owner – игрок
    /// radius – радиус шоквейва
    /// buffDuration – сколько работает абилка (sparks)
    /// </summary>
    public void Init(Transform owner, float radius, float buffDuration)
    {
        _owner = owner;
        _radius = radius;

        _fxEndTime = Time.time + fxDuration;
        _buffEndTime = Time.time + buffDuration;

        shockwaveRing.localScale = Vector3.one * startScale;
        flash.localScale = Vector3.one * startScale;

        sparks.Play();
    }

    private void Update()
    {
        if (_owner != null)
            transform.position = _owner.position;

        if (!fxFinished)
        {
            float t = 1f - ((_fxEndTime - Time.time) / fxDuration);
            t = Mathf.Clamp01(t);

            float ringScale = Mathf.Lerp(startScale, _radius * shockwaveScaleMultiplier, t);
            shockwaveRing.localScale = Vector3.one * ringScale;
            shockMat.SetFloat(fadeID, 1f - t);

            float flashScale = Mathf.Lerp(startScale, _radius * flashScaleMultiplier, t * 0.5f);
            flash.localScale = Vector3.one * flashScale;

            float flashAlpha = Mathf.Clamp01(1f - t * 2f);
            flashMat.color = new Color(0.4f, 0.8f, 1f, flashAlpha);

            if (Time.time >= _fxEndTime)
            {
                shockwaveRing.gameObject.SetActive(false);
                flash.gameObject.SetActive(false);
                fxFinished = true;
            }
        }

        if (Time.time >= _buffEndTime)
        {
            Destroy(gameObject);
        }
    }
}

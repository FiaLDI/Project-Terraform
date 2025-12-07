using UnityEngine;

public class TurretSpawnAnimation : MonoBehaviour
{
    private float time = 0f;
    private float duration = 0.35f;
    private Vector3 targetScale;

    private void Awake()
    {
        targetScale = Vector3.one;
    }

    private void Update()
    {
        time += Time.deltaTime;
        float t = Mathf.SmoothStep(0, 1, time / duration);

        transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);

        if (time >= duration)
            Destroy(this);
    }
}

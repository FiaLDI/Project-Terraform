using UnityEngine;

public class ShieldGridBehaviour : MonoBehaviour
{
    private float _radius;
    private float _deathTime;
    private float _reduceFactor;
    private GameObject _owner;

    public void Init(float radius, float duration, float reductionPercent, GameObject owner)
    {
        _radius = radius;
        _deathTime = Time.time + duration;
        _reduceFactor = Mathf.Clamp01(1f - reductionPercent / 100f);

        _owner = owner;
        transform.localScale = Vector3.one * _radius * 2f;
    }

    public bool IsInside(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos) <= _radius;
    }

    public float ModifyDamage(float dmg)
    {
        return dmg * _reduceFactor;
    }

    private void Update()
    {
        if (Time.time >= _deathTime)
            Destroy(gameObject);
    }
}

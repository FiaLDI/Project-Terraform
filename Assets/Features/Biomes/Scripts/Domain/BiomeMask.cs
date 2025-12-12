using UnityEngine;

[System.Serializable]
public class BiomeMask
{
    public AnimationCurve curve = AnimationCurve.Linear(0, 1, 1, 1);

    public float GetWeight(Vector3 pos)
    {
        return curve.Evaluate(0.5f); // временно: всегда 1
    }
}

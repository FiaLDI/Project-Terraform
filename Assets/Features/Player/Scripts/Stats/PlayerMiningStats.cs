using UnityEngine;

public class PlayerMiningStats : MonoBehaviour
{
    [Header("Mining Speed Multiplier")]
    public float miningMultiplier = 1f;

    public void SetMultiplier(float amount) {
        miningMultiplier = amount;
    }
}

using UnityEngine;

public class PlayerMovementStats : MonoBehaviour
{
    [Header("Base speeds")]
    public float baseSpeed = 5f;
    public float walkSpeed = 2f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 1.5f;
    private float speedMult = 1f;

    [Header("Air & Gravity")]
    public float gravity = -9.81f;
    public float jumpForce = 5f;
    public float airControl = 0.3f;

    [Header("Crouch Settings")]
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float crouchTransitionSpeed = 10f;

    // ----- Бафф скорости (аддитивный)
    private float bonusSpeed = 0f;

    public void AddSpeedMultiplier(float m)
    {
        speedMult *= m;
    }

    public void RemoveSpeedMultiplier(float m)
    {
        speedMult /= m;
    }

    public float GetSpeed(bool isCrouch, bool isSprint, bool isWalk)
    {
        float baseVal =
            isCrouch ? crouchSpeed :
            isSprint ? sprintSpeed :
            isWalk ? walkSpeed :
            baseSpeed;

        return (baseVal + bonusSpeed) * speedMult;
    }

    public void AddSpeed(float v) => bonusSpeed += v;

    public void RemoveSpeed(float v)
    {
        bonusSpeed -= v;
        if (bonusSpeed < 0) bonusSpeed = 0;
    }
}

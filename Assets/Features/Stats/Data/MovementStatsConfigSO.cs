using UnityEngine;

[CreateAssetMenu(menuName = "Orbis/Stats/Movement")]
public class MovementStatsConfigSO : ScriptableObject
{
    [Header("Скорости")]
    public float baseSpeed = 5f;
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float crouchSpeed = 2f;
}

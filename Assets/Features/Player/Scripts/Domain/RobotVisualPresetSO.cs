using UnityEngine;

[CreateAssetMenu(menuName = "Game/Characters/Robot Visual Preset")]
public class RobotVisualPresetSO : ScriptableObject
{
    public string id;
    public GameObject modelPrefab;
    public GameObject deathBurstPrefab;
    public RuntimeAnimatorController animator;
}

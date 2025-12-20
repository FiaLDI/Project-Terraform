using UnityEngine;

[CreateAssetMenu(menuName = "Game/Characters/Robot Visual Preset")]
public class RobotVisualPresetSO : ScriptableObject
{
    public string id;                     // visualPresetId
    public GameObject modelPrefab;        // 3D модель робота
    public RuntimeAnimatorController animator;  // Контроллер анимаций
}

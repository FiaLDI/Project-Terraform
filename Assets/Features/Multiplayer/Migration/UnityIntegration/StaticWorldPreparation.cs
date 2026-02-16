using UnityEngine;

public sealed class StaticWorldPreparation : MonoBehaviour, IWorldPreparationStep
{
    public void Prepare(System.Action onFinished)
    {
        // Ничего не делаем
        onFinished?.Invoke();
    }
}

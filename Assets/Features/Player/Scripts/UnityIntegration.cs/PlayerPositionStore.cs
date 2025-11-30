using UnityEngine;

public class PlayerPositionStore : MonoBehaviour
{
    public static Transform Player;  // глобальная ссылка

    private void Awake()
    {
        Player = transform;
    }
}

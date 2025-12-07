// Assets/Features/Player/Scripts/UnityIntegration/FollowPlayerStore.cs
using UnityEngine;

namespace Features.Player.UnityIntegration
{
    /// <summary>
    /// Любой объект, который должен «следить за игроком»
    /// (например, магазин, UI-камера и т.п.).
    /// </summary>
    public class FollowPlayerStore : MonoBehaviour
    {
        public float speed = 5f;
        public Vector3 offset;

        private void LateUpdate()
        {
            var reg = PlayerRegistry.Instance;
            if (reg == null || reg.LocalPlayer == null)
                return;

            Vector3 targetPos = reg.LocalPlayer.transform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        }
    }
}

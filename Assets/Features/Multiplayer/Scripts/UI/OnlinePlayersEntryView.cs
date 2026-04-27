using TMPro;
using UnityEngine;

namespace Features.Multiplayer.UI
{
    public sealed class OnlinePlayersEntryView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nicknameText;
        [SerializeField] private TMP_Text classText;
        [SerializeField] private TMP_Text levelText;

        public void Bind(string nickname, string className, int level)
        {
            if (nicknameText != null)
                nicknameText.text = string.IsNullOrWhiteSpace(nickname) ? "Unknown" : nickname;

            if (classText != null)
                classText.text = string.IsNullOrWhiteSpace(className) ? "Unknown" : className;

            if (levelText != null)
                levelText.text = $"LVL {Mathf.Max(1, level)}";
        }
    }
}

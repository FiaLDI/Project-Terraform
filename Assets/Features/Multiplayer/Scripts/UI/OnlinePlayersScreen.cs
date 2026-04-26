using System;
using System.Collections.Generic;
using System.Text;
using Features.Classes.Data;
using Features.Input;
using Features.Player.UI;
using Features.UI;
using TMPro;
using UnityEngine;

namespace Features.Multiplayer.UI
{
    public sealed class OnlinePlayersScreen : PlayerBoundUIView, IUIScreen
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private GameObject emptyState;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private OnlinePlayersEntryView entryPrefab;
        [SerializeField] private float refreshInterval = 1f;

        private PlayerSessionNetwork playerSessionNetwork;
        private PlayerClassLibrarySO classLibrary;
        private float nextRefreshAt;
        private readonly List<OnlinePlayersEntryView> spawnedEntries = new();
        private string[] cachedNicknames = Array.Empty<string>();
        private string[] cachedClassIds = Array.Empty<string>();
        private int[] cachedLevels = Array.Empty<int>();

        public InputMode Mode => InputMode.Gameplay;

        protected override void OnEnable()
        {
            EnsureBuilt();
            base.OnEnable();
            UIRegistry.I?.Register(this);
        }

        protected override void OnDisable()
        {
            UIRegistry.I?.Unregister(this);
            base.OnDisable();
        }

        protected override void OnPlayerBound(GameObject player)
        {
            playerSessionNetwork = player != null
                ? player.GetComponent<PlayerSessionNetwork>()
                : null;

            if (root != null && root.activeSelf)
                RequestRefresh();
        }

        protected override void OnPlayerUnbound(GameObject player)
        {
            playerSessionNetwork = null;
            cachedNicknames = Array.Empty<string>();
            cachedClassIds = Array.Empty<string>();
            cachedLevels = Array.Empty<int>();
            ClearEntries();
            Hide();
        }

        private void Update()
        {
            if (root == null || !root.activeSelf)
                return;

            if (Time.unscaledTime >= nextRefreshAt)
                RequestRefresh();
        }

        public void Show()
        {
            if (!EnsureBuilt())
                return;

            root.SetActive(true);
            Render(cachedNicknames, cachedClassIds, cachedLevels);
            RequestRefresh();
        }

        public void Hide()
        {
            if (root != null)
                root.SetActive(false);
        }

        public void Open()
        {
            if (!EnsureBuilt())
                return;

            if (UIStackManager.I == null)
            {
                Show();
                return;
            }

            if (UIStackManager.I.IsTop<OnlinePlayersScreen>())
            {
                Show();
                return;
            }

            UIStackManager.I.Push(this);
        }

        public void Close()
        {
            if (UIStackManager.I != null && UIStackManager.I.IsTop<OnlinePlayersScreen>())
            {
                UIStackManager.I.Pop();
                return;
            }

            Hide();
        }

        public void ApplySnapshot(string[] nicknames, string[] classIds, int[] levels)
        {
            cachedNicknames = nicknames ?? Array.Empty<string>();
            cachedClassIds = classIds ?? Array.Empty<string>();
            cachedLevels = levels ?? Array.Empty<int>();
            Render(cachedNicknames, cachedClassIds, cachedLevels);
        }

        public void RequestRefresh()
        {
            nextRefreshAt = Time.unscaledTime + Mathf.Max(0.25f, refreshInterval);

            if (playerSessionNetwork == null)
            {
                RenderStatus("Player session is not ready.");
                return;
            }

            playerSessionNetwork.RequestOnlinePlayersServerRpc();
        }

        public static OnlinePlayersScreen Resolve()
        {
            var screen = UIRegistry.I?.Get<OnlinePlayersScreen>();
            if (screen != null)
                return screen;

            screen = UnityEngine.Object.FindFirstObjectByType<OnlinePlayersScreen>(FindObjectsInactive.Include);
            if (screen != null)
                return screen;

            var root = PlayerUIRoot.I ?? UnityEngine.Object.FindFirstObjectByType<PlayerUIRoot>(FindObjectsInactive.Include);
            if (root == null)
                return null;

            screen = root.GetComponent<OnlinePlayersScreen>();
            return screen;
        }

        private void Render(string[] nicknames, string[] classIds, int[] levels)
        {
            if (!EnsureBuilt())
                return;

            int count = Mathf.Min(
                nicknames != null ? nicknames.Length : 0,
                classIds != null ? classIds.Length : 0,
                levels != null ? levels.Length : 0);

            titleText.text = $"ONLINE PLAYERS  {count}";

            ClearEntries();

            if (emptyState != null)
                emptyState.SetActive(count == 0);

            if (count == 0)
            {
                SetEmptyStateText("No players online.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                var entry = Instantiate(entryPrefab, contentRoot);
                entry.Bind(
                    nicknames[i],
                    ResolveClassName(classIds[i]),
                    levels[i]);
                spawnedEntries.Add(entry);
            }
        }

        private void RenderStatus(string message)
        {
            if (!EnsureBuilt())
                return;

            titleText.text = "ONLINE PLAYERS";
            ClearEntries();

            if (emptyState != null)
                emptyState.SetActive(true);

            SetEmptyStateText(message);
        }

        private string ResolveClassName(string classId)
        {
            if (string.IsNullOrWhiteSpace(classId))
                return "Unknown";

            classLibrary ??= UnityEngine.Resources.Load<PlayerClassLibrarySO>("Databases/PlayerClassLibrary");

            var cfg = classLibrary != null ? classLibrary.FindById(classId) : null;
            if (cfg != null && !string.IsNullOrWhiteSpace(cfg.displayName))
                return cfg.displayName;

            return NicifyClassId(classId);
        }

        private static string NicifyClassId(string classId)
        {
            var source = classId.Replace('_', ' ').Trim();
            if (source.Length == 0)
                return "Unknown";

            var builder = new StringBuilder(source.Length);
            bool makeUpper = true;

            foreach (char c in source)
            {
                if (char.IsWhiteSpace(c) || c == '-')
                {
                    builder.Append(' ');
                    makeUpper = true;
                    continue;
                }

                builder.Append(makeUpper ? char.ToUpperInvariant(c) : c);
                makeUpper = false;
            }

            return builder.ToString();
        }

        private bool EnsureBuilt()
        {
            if (root == null)
            {
                Debug.LogWarning("[OnlinePlayersScreen] Root is not assigned.", this);
                return false;
            }

            if (titleText == null)
            {
                Debug.LogWarning("[OnlinePlayersScreen] Title text is not assigned.", this);
                return false;
            }

            if (contentRoot == null)
            {
                Debug.LogWarning("[OnlinePlayersScreen] Content root is not assigned.", this);
                return false;
            }

            if (entryPrefab == null)
            {
                Debug.LogWarning("[OnlinePlayersScreen] Entry prefab is not assigned.", this);
                return false;
            }

            return true;
        }

        private void ClearEntries()
        {
            for (int i = 0; i < spawnedEntries.Count; i++)
            {
                var entry = spawnedEntries[i];
                if (entry != null)
                    Destroy(entry.gameObject);
            }

            spawnedEntries.Clear();
        }

        private void SetEmptyStateText(string value)
        {
            var emptyText = emptyState != null ? emptyState.GetComponentInChildren<TMP_Text>(true) : null;
            if (emptyText != null)
                emptyText.text = value;
        }
    }
}

using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace Features.Multiplayer.SceneBinding
{
    [RequireComponent(typeof(NetworkObject))]
    public abstract class SceneBoundNetworkControllerBase : NetworkBehaviour
    {
        private readonly SyncVar<string> boundKey = new();
        private readonly System.Collections.Generic.List<ISceneBoundView> boundViews = new();

        private ISceneBoundView pendingBoundView;
        private ISceneBoundView boundView;
        private bool subscribedToViewRegistry;

        protected bool ServerBindingReady { get; private set; }

        public string BoundKey => boundKey.Value;
        protected ISceneBoundView BoundView => boundView;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            boundKey.OnChange += OnBoundKeyChanged;
        }

        public override void OnStopNetwork()
        {
            boundKey.OnChange -= OnBoundKeyChanged;
            base.OnStopNetwork();
        }

        public void InitBinding(ISceneBoundView view)
        {
            if (!InstanceFinder.IsServerStarted)
            {
                Debug.LogWarning($"[{name}] InitBinding must be called on server.", this);
                return;
            }

            if (view == null || string.IsNullOrWhiteSpace(view.BoundKey))
            {
                Debug.LogError($"[{name}] Invalid scene bound view.", this);
                return;
            }

            pendingBoundView = view;

            if (IsServerInitialized)
                CompleteServerBinding();
        }

        public override void OnSpawnServer(NetworkConnection connection)
        {
            base.OnSpawnServer(connection);

            if (pendingBoundView != null)
                CompleteServerBinding();
        }

        public override void OnStartClient()
        {
            base.OnStartClient();

            if (!string.IsNullOrWhiteSpace(boundKey.Value))
                SceneBoundRegistry.RegisterController(boundKey.Value, this);

            Debug.Log($"[SceneBound] Client started controller={name} key={boundKey.Value}", this);

            BindOrSubscribe();
            ApplyStateToView(true);
        }

        public override void OnStopClient()
        {
            UnsubscribeFromViewRegistry();

            if (!string.IsNullOrWhiteSpace(boundKey.Value))
                SceneBoundRegistry.UnregisterController(boundKey.Value, this);

            base.OnStopClient();
        }

        public override void OnStopServer()
        {
            ServerBindingReady = false;
            pendingBoundView = null;

            if (!string.IsNullOrWhiteSpace(boundKey.Value))
                SceneBoundRegistry.UnregisterController(boundKey.Value, this);

            base.OnStopServer();
        }

        private void OnBoundKeyChanged(string previous, string next, bool asServer)
        {
            if (!string.IsNullOrWhiteSpace(previous))
                SceneBoundRegistry.UnregisterController(previous, this);

            if (!string.IsNullOrWhiteSpace(next))
                SceneBoundRegistry.RegisterController(next, this);

            boundView = null;
            BindOrSubscribe();
            ApplyStateToView(true);
        }

        private void CompleteServerBinding()
        {
            if (pendingBoundView == null || string.IsNullOrWhiteSpace(pendingBoundView.BoundKey))
                return;

            boundView = pendingBoundView;
            boundKey.Value = pendingBoundView.BoundKey;
            ServerBindingReady = true;

            SceneBoundRegistry.RegisterController(boundKey.Value, this);

            Debug.Log($"[SceneBound] Server bound controller={name} to key={boundKey.Value}", this);

            OnServerBoundToView(pendingBoundView);
            ApplyStateToView(true);
        }

        private void BindOrSubscribe()
        {
            if (string.IsNullOrWhiteSpace(boundKey.Value))
                return;

            if (SceneBoundRegistry.GetViews(boundKey.Value, boundViews) > 0)
            {
                BindViews();
                return;
            }

            SubscribeToViewRegistry();
        }

        private void BindViews()
        {
            if (boundViews.Count == 0)
                return;

            boundView = boundViews[0];
            UnsubscribeFromViewRegistry();

            OnClientBoundToView(boundView);
        }

        private void SubscribeToViewRegistry()
        {
            if (subscribedToViewRegistry)
                return;

            SceneBoundRegistry.ViewRegistered += OnViewRegistered;
            SceneBoundRegistry.ViewUnregistered += OnViewUnregistered;
            subscribedToViewRegistry = true;
        }

        private void UnsubscribeFromViewRegistry()
        {
            if (!subscribedToViewRegistry)
                return;

            SceneBoundRegistry.ViewRegistered -= OnViewRegistered;
            SceneBoundRegistry.ViewUnregistered -= OnViewUnregistered;
            subscribedToViewRegistry = false;
        }

        private void OnViewRegistered(string key, ISceneBoundView view)
        {
            if (key != boundKey.Value)
                return;

            boundView = null;
            BindOrSubscribe();
            ApplyStateToView(true);
        }

        private void OnViewUnregistered(string key, ISceneBoundView view)
        {
            if (key != boundKey.Value)
                return;

            if (ReferenceEquals(boundView, view))
                boundView = null;

            BindOrSubscribe();
            ApplyStateToView(true);
        }

        protected TView GetView<TView>()
            where TView : class, ISceneBoundView
        {
            BindOrSubscribe();
            return boundView as TView;
        }

        protected void ReapplyStateToView(bool snap = false)
        {
            ApplyStateToView(snap);
        }

        private void ApplyStateToView(bool snap)
        {
            BindOrSubscribe();

            if (!string.IsNullOrWhiteSpace(boundKey.Value) &&
                SceneBoundRegistry.GetViews(boundKey.Value, boundViews) > 0)
            {
                for (int i = 0; i < boundViews.Count; i++)
                    OnApplyStateToView(boundViews[i], snap);

                return;
            }

            if (boundView != null)
                OnApplyStateToView(boundView, snap);
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender = null)
        {
            if (!IsServerInitialized || !ServerBindingReady)
            {
                Debug.LogWarning(
                    $"[SceneBound] Ignored interaction command={command} key={boundKey.Value} " +
                    $"IsServerInitialized={IsServerInitialized} ServerBindingReady={ServerBindingReady}",
                    this
                );
                return;
            }

            Debug.Log($"[SceneBound] Server handling interaction command={command} key={boundKey.Value}", this);
            ServerHandleInteraction(command, sender);
        }

        protected virtual void OnServerBoundToView(ISceneBoundView view) { }

        protected virtual void OnClientBoundToView(ISceneBoundView view) { }

        protected virtual void ServerHandleInteraction(
            SceneBoundInteractionCommand command,
            NetworkConnection sender)
        {
        }

        protected abstract void OnApplyStateToView(ISceneBoundView view, bool snap);
    }
}

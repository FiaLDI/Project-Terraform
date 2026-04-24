using System;
using System.Collections.Generic;
using UnityEngine;

namespace Features.Multiplayer.SceneBinding
{
    public static class SceneBoundRegistry
    {
        private static readonly Dictionary<string, List<ISceneBoundView>> Views = new();
        private static readonly Dictionary<string, SceneBoundNetworkControllerBase> Controllers = new();

        public static event Action<string, ISceneBoundView> ViewRegistered;
        public static event Action<string, ISceneBoundView> ViewUnregistered;

        public static event Action<string, SceneBoundNetworkControllerBase> ControllerRegistered;
        public static event Action<string, SceneBoundNetworkControllerBase> ControllerUnregistered;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Views.Clear();
            Controllers.Clear();

            ViewRegistered = null;
            ViewUnregistered = null;
            ControllerRegistered = null;
            ControllerUnregistered = null;
        }

        public static void RegisterView(ISceneBoundView view)
        {
            if (view == null || string.IsNullOrWhiteSpace(view.BoundKey))
                return;

            if (!Views.TryGetValue(view.BoundKey, out var list))
            {
                list = new List<ISceneBoundView>();
                Views[view.BoundKey] = list;
            }

            CleanupViews(list);

            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], view))
                    return;
            }

            list.Add(view);
            ViewRegistered?.Invoke(view.BoundKey, view);
        }

        public static void UnregisterView(ISceneBoundView view)
        {
            if (view == null || string.IsNullOrWhiteSpace(view.BoundKey))
                return;

            if (!Views.TryGetValue(view.BoundKey, out var list))
                return;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(list[i], view))
                    list.RemoveAt(i);
            }

            if (list.Count == 0)
                Views.Remove(view.BoundKey);

            ViewUnregistered?.Invoke(view.BoundKey, view);
        }

        public static bool TryGetView(string key, out ISceneBoundView view)
        {
            if (Views.TryGetValue(key, out var list))
            {
                CleanupViews(list);

                if (list.Count > 0)
                {
                    view = list[0];
                    return true;
                }
            }

            view = null;
            return false;
        }

        public static int GetViews(string key, List<ISceneBoundView> results)
        {
            if (results == null)
                return 0;

            results.Clear();

            if (!Views.TryGetValue(key, out var list))
                return 0;

            CleanupViews(list);
            results.AddRange(list);
            return results.Count;
        }

        public static bool TryGetView<TView>(string key, out TView view)
            where TView : class, ISceneBoundView
        {
            if (TryGetView(key, out var raw) && raw is TView typed)
            {
                view = typed;
                return true;
            }

            view = null;
            return false;
        }

        public static void RegisterController(string key, SceneBoundNetworkControllerBase controller)
        {
            if (controller == null || string.IsNullOrWhiteSpace(key))
                return;

            Controllers[key] = controller;
            ControllerRegistered?.Invoke(key, controller);
        }

        public static void UnregisterController(string key, SceneBoundNetworkControllerBase controller)
        {
            if (string.IsNullOrWhiteSpace(key))
                return;

            if (Controllers.TryGetValue(key, out var current) && current == controller)
            {
                Controllers.Remove(key);
                ControllerUnregistered?.Invoke(key, controller);
            }
        }

        public static bool TryGetController(string key, out SceneBoundNetworkControllerBase controller)
        {
            if (Controllers.TryGetValue(key, out controller))
            {
                if (controller != null)
                    return true;

                Controllers.Remove(key);
            }

            controller = null;
            return false;
        }

        private static void CleanupViews(List<ISceneBoundView> views)
        {
            for (int i = views.Count - 1; i >= 0; i--)
            {
                if (views[i] is UnityEngine.Object obj && obj == null)
                    views.RemoveAt(i);
            }
        }

        public static bool TryGetController<TController>(string key, out TController controller)
            where TController : SceneBoundNetworkControllerBase
        {
            if (TryGetController(key, out var raw) && raw is TController typed)
            {
                controller = typed;
                return true;
            }

            controller = null;
            return false;
        }
    }
}

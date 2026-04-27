using System;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Application
{
    public static class QuestEventBus
    {
        private sealed class SubscriptionEntry
        {
            public Delegate Original;
            public Action<object, IQuestEvent> Wrapped;
        }

        private static readonly Dictionary<Type, List<SubscriptionEntry>> subscribers
            = new();

        public static void Subscribe<T>(Action<object, T> handler)
            where T : IQuestEvent
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var list))
            {
                list = new List<SubscriptionEntry>();
                subscribers[type] = list;
            }

            list.Add(new SubscriptionEntry
            {
                Original = handler,
                Wrapped = (src, e) => handler(src, (T)e)
            });
        }

        public static void Unsubscribe<T>(Action<object, T> handler)
            where T : IQuestEvent
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var list))
                return;

            list.RemoveAll(entry => Equals(entry.Original, handler));
        }

        public static void Publish(IQuestEvent e)
        {
            var type = e.GetType();

            if (!subscribers.TryGetValue(type, out var list))
                return;

            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
                snapshot[i].Wrapped(e.Source, e);
        }
    }
}

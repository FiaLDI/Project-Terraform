using System;
using System.Collections.Generic;
using Features.Quests.Domain;

namespace Features.Quests.Application
{
    public static class QuestEventBus
    {
        private static readonly Dictionary<Type, List<Action<object, IQuestEvent>>> subscribers
            = new();

        public static void Subscribe<T>(Action<object, T> handler)
            where T : IQuestEvent
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var list))
            {
                list = new List<Action<object, IQuestEvent>>();
                subscribers[type] = list;
            }

            list.Add((src, e) => handler(src, (T)e));
        }

        public static void Unsubscribe<T>(Action<object, T> handler)
            where T : IQuestEvent
        {
            var type = typeof(T);

            if (!subscribers.TryGetValue(type, out var list))
                return;

            list.RemoveAll(a => a.Method == handler.Method);
        }

        public static void Publish(IQuestEvent e)
        {
            var type = e.GetType();

            if (!subscribers.TryGetValue(type, out var list))
                return;

            foreach (var handler in list)
                handler(e.Source, e);
        }
    }
}
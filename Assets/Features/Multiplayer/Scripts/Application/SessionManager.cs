using System.Collections.Generic;
using Multiplayer.Domain;

namespace Multiplayer.Application
{
    public sealed class SessionManager
    {
        private readonly Dictionary<string, PlayerSession> sessions = new();
        private readonly Dictionary<int, string> clientToPersistent = new();

        public PlayerSession HandleLogin(int clientId, string persistentId)
        {
            if (!sessions.TryGetValue(persistentId, out var session))
            {
                session = new PlayerSession(persistentId);
                sessions[persistentId] = session;
            }

            session.BindClient(clientId);
            clientToPersistent[clientId] = persistentId;

            return session;
        }

        public PlayerSession GetSessionByClient(int clientId)
        {
            if (!clientToPersistent.TryGetValue(clientId, out var pid))
                return null;

            return sessions.TryGetValue(pid, out var session)
                ? session
                : null;
        }

       public void HandleDisconnect(int clientId)
        {
            if (!clientToPersistent.TryGetValue(clientId, out var pid))
                return;

            if (sessions.TryGetValue(pid, out var session))
            {
                session.UnbindClient();

                // НЕ удаляем PlayerObject
                // Он остается в мире
            }

            clientToPersistent.Remove(clientId);
        }


        public void ResetAll()
        {
            sessions.Clear();
            clientToPersistent.Clear();
        }

        public IEnumerable<PlayerSession> GetOnlineSessions()
        {
            foreach (var s in sessions.Values)
                if (s.IsOnline)
                    yield return s;
        }

    }
}

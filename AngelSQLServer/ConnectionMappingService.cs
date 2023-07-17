using System.Collections.Concurrent;

namespace AngelSQLServer
{
    public class ConnectionMappingService
    {
        public ConcurrentDictionary<string, string> connections =
            new ConcurrentDictionary<string, string>();

        public void AddConnection(string UserId, string _connectionId)
        {
            connections.TryAdd(UserId, _connectionId);
        }

        public void RemoveConnection(string UserId)
        {
            connections.TryRemove(UserId, out var _);
        }

        public string GetConnection(string UserId)
        {
            connections.TryGetValue(UserId, out var connectionId);
            return connectionId;
        }

        public string GetConnectionCount()
        {
            return connections.Count.ToString();
        }

        public bool IsConnectionId(string UserId)
        {
            if (connections.ContainsKey(UserId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

    }

}

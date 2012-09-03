using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace RealTime.EndPoints
{
    public class ConnectionLookup
    {
        private ConcurrentDictionary<int, string> userToConnection = new ConcurrentDictionary<int, string>();
        private ConcurrentDictionary<string, int> connectionToUser = new ConcurrentDictionary<string, int>();

        public void Add(int userId, string connectionId) 
        {
            if (!userToConnection.TryAdd(userId, connectionId)) {
                userToConnection[userId] = connectionId;
            }
            if (!connectionToUser.TryAdd(connectionId, userId)) {
                connectionToUser[connectionId] = userId;
            }
        }

        public void Update(int userId, string connectionId) 
        {
            userToConnection[userId] = connectionId;
            connectionToUser[connectionId] = userId;
        }

        public string GetConnectionForUser(int userId) 
        {
            string connectionId = null;
            userToConnection.TryGetValue(userId, out connectionId);

            return connectionId;
        }

        public int? GetUserForConnection(string connectionId) 
        {
            int userId = 0;
            connectionToUser.TryGetValue(connectionId, out userId);

            return userId == 0 ? null : new Nullable<int>(userId);
        }

        public void Remove(int userId) 
        {
            string connectionId = null;
            userToConnection.TryRemove(userId, out connectionId);
        }

        public void RemoveConnection(string connectionId) 
        {
            int userId = 0;
            connectionToUser.TryRemove(connectionId, out userId);
        }
    }
}
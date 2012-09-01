using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections.Concurrent;

namespace RealTime.EndPoints
{
    public class ConnectionLookup
    {
        private ConcurrentDictionary<string, string> userToConnection = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, string> connectionToUser = new ConcurrentDictionary<string, string>();

        public void Add(string username, string connectionId) 
        {
            userToConnection.TryAdd(username, connectionId);
            connectionToUser.TryAdd(connectionId, username);            
        }

        public void Update(string username, string connectionId) 
        {
            userToConnection[username] = connectionId;
            connectionToUser[connectionId] = username;
        }

        public string GetConnectionForUser(string username) 
        {
            string connectionId = null;
            userToConnection.TryGetValue(username, out connectionId);

            return connectionId;
        }

        public string GetUserForConnection(string connectionId) 
        {
            string username = null;
            connectionToUser.TryGetValue(connectionId, out username);

            return username;
        }

        public void Remove(string username) 
        {
            string connectionId = null;
            userToConnection.TryRemove(username, out connectionId);
        }

        public void RemoveConnection(string connectionId) 
        {
            string username = null;
            connectionToUser.TryRemove(connectionId, out username);
        }
    }
}
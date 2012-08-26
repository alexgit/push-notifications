using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RealTime.Models;

namespace RealTime.EndPoints
{
    public class RelayEndpoint : PersistentConnection
    {        
        private static ConnectionLookup connectionLookup = new ConnectionLookup();

        protected override Task OnConnectedAsync(IRequest request, string connectionId)
        {
            if (request.User.Identity.IsAuthenticated) 
            {
                var username = request.User.Identity.Name;

                connectionLookup.Add(username, connectionId);
            }
                                
            return Connection.Broadcast("User " + connectionId + " has connected");
        }

        protected override Task OnDisconnectAsync(string connectionId)
        {            
            connectionLookup.Remove(connectionId);

            return Connection.Broadcast("connection severed");
        }

        protected override Task OnReconnectedAsync(IRequest request, IEnumerable<string> groups, string connectionId)
        {
            if (request.User.Identity.IsAuthenticated)
            {
                var username = request.User.Identity.Name;

                connectionLookup.Update(username, connectionId);
            }
            else 
            {
                connectionLookup.RemoveConnection(connectionId);
            }

            return Connection.Broadcast("User " + connectionId + " has connected");
        }
        
        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            if (data == "Greetings")
            {
                return Connection.Send(connectionId, "Hello there");
            }
            else if (data == "newthing")
            {
                return Connection.Broadcast(JsonConvert.SerializeObject(new Thing
                {
                    Id = Guid.NewGuid(),
                    Title = "New Thing",
                    Time = "12:05",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod
                                tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam,
                                quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo
                                consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse
                                cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non
                                proident, sunt in culpa qui officia deserunt mollit anim id est laborum."
                }));
            }
            else if (data.StartsWith("imhandling"))
            {
                var id = data.Split('/')[1];
                return Connection.Broadcast(string.Format("someoneshandling/{0}", id));
            }
            else
            {
                return Task.Factory.StartNew(() => { });
            }
        }
    }
}
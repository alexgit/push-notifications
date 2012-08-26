using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;
using System.Threading.Tasks;

namespace RealTime.EndPoints
{
    public class RelayEndpoint : PersistentConnection
    {
        protected override Task OnConnectedAsync(IRequest request, string connectionId)
        {                                   
            return Connection.Broadcast("User " + connectionId + " has connected");
        }
    }
}
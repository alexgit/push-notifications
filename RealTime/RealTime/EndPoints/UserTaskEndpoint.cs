using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RealTime.Models;
using RealTime.MessageHandlers;
using NServiceBus;
using Messages;
using SignalR.Hubs;

namespace RealTime.EndPoints
{
    public class UserTaskEndpoint : PersistentConnection
    {        
        private readonly ConnectionLookup connectionLookup;
        private readonly TaskDistributor taskDistributor;
        private readonly IBus messageBus;
        private User user;
               
        public UserTaskEndpoint(TaskDistributor taskDistributor, ConnectionLookup connectionLookup, IBus messageBus) 
        {
            this.taskDistributor = taskDistributor;
            this.connectionLookup = connectionLookup;
            this.messageBus = messageBus;
        }

        protected override Task OnConnectedAsync(IRequest request, string connectionId)
        {
            if (request.User.Identity.IsAuthenticated)
            {
                user = (User)HttpContext.Current.User;
                                
                connectionLookup.Add(user.Id, connectionId);

                var tasks = taskDistributor.GetTasksForUser(user.Id);

                if(tasks.Count() > 0) 
                {
                    SendMessage(connectionId, "tasklist", tasks);                    
                }
            }

            return DoNothing();
        }

        protected override Task OnDisconnectAsync(string connectionId)
        {            
            connectionLookup.RemoveConnection(connectionId);

            return Connection.Broadcast("connection severed");
        }

        protected override Task OnReconnectedAsync(IRequest request, IEnumerable<string> groups, string connectionId)
        {
            if (request.User.Identity.IsAuthenticated)
            {
                var user = (User)HttpContext.Current.User;

                connectionLookup.Update(user.Id, connectionId);
            }
            else 
            {
                connectionLookup.RemoveConnection(connectionId);
            }

            return Connection.Broadcast("User " + connectionId + " has reconnected");
        }
        
        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            var messageParts = data.Split('/');
            var command = messageParts[0];
            var payload = messageParts[1];

            var userId = connectionLookup.GetUserForConnection(connectionId);

            if (userId.HasValue) 
            {
                switch (command)
                {
                    case "starttask":
                        StartTask(userId.Value, payload);
                        break;
                    case "aborttask":
                        AbortTask(userId.Value, payload);
                        break;
                    default:
                        break;
                }
            
            }            

            return DoNothing();
        }

        private void AbortTask(int userId, string payload)
        {
            var taskId = Guid.Parse(payload);

            messageBus.Send<AbortTask>(m =>
            {
                m.TaskId = taskId;
                m.UserId = userId;
            });
        }

        private void StartTask(int userId, string payload)
        {
            var taskId = Guid.Parse(payload);            

            messageBus.Send<StartTask>(m =>
            {
                m.TaskId = taskId;
                m.UserId = userId;
            });
        }

        private void SendMessage(string connectionId, string messageType, object messageContent)
        {
            var serialized = JsonConvert.SerializeObject(messageContent);
            var message = string.Format("{0}/{1}", messageType, serialized);

            Connection.Send(connectionId, message);
        }

        private Task DoNothing() 
        {
            return new Task(() => { });
        }
    }
}
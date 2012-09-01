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

namespace RealTime.EndPoints
{
    public class UserTaskEndpoint : PersistentConnection
    {        
        private readonly ConnectionLookup connectionLookup;
        private readonly TaskDistributor taskDistributor;
        private readonly IBus messageBus;
        
        //TODO: need to set up IoC / Dependency Injection with SignalR
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
                var username = request.User.Identity.Name;

                connectionLookup.Add(username, connectionId);

                var userId = GetUserId(username);
                var tasks = taskDistributor.GetTasksForUser(userId);

                if(tasks.Count() > 0) 
                {
                    SendMessage(connectionId, "tasklist", tasks);                    
                }
            }

            return null;
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

            return Connection.Broadcast("User " + connectionId + " has reconnected");
        }
        
        protected override Task OnReceivedAsync(IRequest request, string connectionId, string data)
        {
            var messageParts = data.Split('/');
            var command = messageParts[0];
            var payload = messageParts[1];

            var user = connectionLookup.GetUserForConnection(connectionId);
            var userId = GetUserId(user);

            switch (command) 
            {
                case "starttask":
                    StartTask(userId, payload);
                    break;
                case "aborttask":
                    AbortTask(userId, payload);
                    break;
                default:
                    break;
            }

            return null;
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
 
        private int GetUserId(string username) 
        {
            throw new NotImplementedException();
        }

        private void SendMessage(string connectionId, string messageType, object messageContent)
        {
            var serialized = JsonConvert.SerializeObject(messageContent);
            var message = string.Format("{0}/{1}", messageType, serialized);

            Connection.Send(connectionId, message);
        }   
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR;
using RealTime.MessageHandlers;
using Newtonsoft.Json;

namespace RealTime.EndPoints
{
    public class TaskNotifier : INotifyUsersOfTasks
    {
        private readonly ConnectionLookup connectionLookup;
        
        public TaskNotifier(ConnectionLookup connectionLookup) 
        {
            this.connectionLookup = connectionLookup;
        }

        private IConnection GetConnection() 
        {
            return GlobalHost.ConnectionManager.GetConnectionContext<UserTaskEndpoint>().Connection;
        }

        public void NotifyNewTask(int userToNotify, UserTask task)
        {
            var connection = connectionLookup.GetConnectionForUser(userToNotify);

            if (connection != null)
                SendMessage(connection, "newtask", task);
        }

        public void NotifyTaskStarted(int userToNotify, int userWhoStartedTask, Guid taskId)
        {
            var connection = connectionLookup.GetConnectionForUser(userToNotify);

            if (connection != null)
                SendMessage(connection, "taskstarted", new { taskId });
        }

        public void NotifyTaskAborted(int userToNotify, int userWhoAbortedTask, Guid taskId)
        {
            var connection = connectionLookup.GetConnectionForUser(userToNotify);

            if (connection != null)
                SendMessage(connection, "taskaborted", new { taskId });
        }

        public void NotifyTaskCompleted(int userToNotify, int userWhoCompletedTask, Guid taskId)
        {
            var connection = connectionLookup.GetConnectionForUser(userToNotify);

            if (connection != null)
                SendMessage(connection, "taskcompleted", new { taskId });
        }

        private void SendMessage(string connectionId, string messageType, object messageContent)
        {
            var serialized = JsonConvert.SerializeObject(messageContent);
            var message = string.Format("{0}/{1}", messageType, serialized);

            GetConnection().Send(connectionId, message);
        }
    }
}
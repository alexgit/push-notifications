using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Messages;
using NServiceBus;
using System.Web.Mvc;

namespace RealTime.MessageHandlers
{
    public class TaskDistributor : IHandleMessages<TaskWasCreated>, IHandleMessages<TaskWasStarted>, 
                                        IHandleMessages<TaskWasAborted>, IHandleMessages<TaskWasCompleted>
    {
        private IDictionary<int, TaskQueue> taskQueues = new Dictionary<int, TaskQueue>();
        
        public TaskDistributor(INotifyUsersOfTasks taskNotificaionService, IUserAccountService userAccountService) 
        {
            InitializeQueues(taskNotificaionService, userAccountService);
        }

        private void InitializeQueues(INotifyUsersOfTasks taskNotificationService, IUserAccountService userAccountService)
        {
            foreach (var userId in userAccountService.GetAllUserIds())
            {
                taskQueues.Add(userId, new TaskQueue(userId, taskNotificationService));
            }
        }

        public void Handle(TaskWasCreated message)
        {
            foreach(var user in message.Users) 
            {
                taskQueues[user].Enqueue(new UserTask
                {
                    Id = message.TaskId,
                    Description = message.Description,
                    ActionUrl = message.ActionURL
                });
            }
        }

        public void Handle(TaskWasStarted message)
        {
            var userId = message.StartedBy;
            var taskId = message.TaskId;

            foreach (var kvp in taskQueues.Where(x => x.Key != userId))
            {
                var queue = kvp.Value;
                queue.TaskStarted(taskId, userId);
            }
        }        

        public void Handle(TaskWasAborted message)
        {
            var userId = message.AbortedBy;
            var taskId = message.TaskId;

            foreach (var kvp in taskQueues.Where(x => x.Key != userId))
            {
                var queue = kvp.Value;
                queue.TaskAborted(taskId);
            }
        }

        public void Handle(TaskWasCompleted message)
        {
            var taskId = message.TaskId;

            foreach (var queue in taskQueues.Values)
            {
                if(queue.Contains(taskId))
                    queue.Remove(taskId);
            }
        }

        public UserTask[] GetTasksForUser(int userId) 
        {
            return taskQueues[userId].GetAll();
        }
    }
}
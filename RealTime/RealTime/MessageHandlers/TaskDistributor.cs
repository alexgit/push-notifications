using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Messages;
using NServiceBus;

namespace RealTime.MessageHandlers
{
    public class TaskDistributor : IHandleMessages<TaskWasCreated>, IHandleMessages<TaskWasStarted>, 
                                        IHandleMessages<TaskWasAborted>, IHandleMessages<TaskWasCompleted>
    {
        private IDictionary<int, TaskQueue> taskQueues = new Dictionary<int, TaskQueue>();
        private IUserAccountService userAccountService;

        public TaskDistributor(INotifyUsersOfTasks taskNotificaionService, IUserAccountService userAccountService) 
        {
            this.userAccountService = userAccountService;

            InitializeQueues(taskNotificaionService);
        }

        private void InitializeQueues(INotifyUsersOfTasks taskNotificationService)
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
                queue.Remove(taskId);
            }
        }

        public UserTask[] GetTasksForUser(int userId) 
        {
            return taskQueues[userId].GetAll();
        }
    }
}
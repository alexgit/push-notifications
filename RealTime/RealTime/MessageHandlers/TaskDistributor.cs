using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Messages;
using NServiceBus;
using System.Web.Mvc;
using RealTime.Infrastructure;

namespace RealTime.MessageHandlers
{
    public class TaskDistributor : IHandleMessages<TaskWasCreated>, IHandleMessages<TaskWasStarted>, 
                                        IHandleMessages<TaskWasAborted>, IHandleMessages<TaskWasCompleted>,
                                            IReactToUserLoggedIn, IReactToUserLoggedOff, IReactToUserSessionTimeouts
    {
        private IDictionary<int, ITaskQueue> taskQueues = new Dictionary<int, ITaskQueue>();
        private IQueueFactory queueFactory;
        
        public TaskDistributor(IQueueFactory queueFactory, IUserAccountService userAccountService) 
        {
            this.queueFactory = queueFactory;
            InitializeQueues(userAccountService);
        }

        private void InitializeQueues(IUserAccountService userAccountService)
        {
            foreach (var userId in userAccountService.GetAllUserIds())
            {
                taskQueues.Add(userId, queueFactory.Create<OfflineTaskQueue>(userId));
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
                    ActionUrl = message.ActionURL,
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

        public Action<UserAccounts.SimpleMembershipUser> OnLogin
        {
            get 
            { 
                return user => 
                {
                    var currentQueue = taskQueues[user.Id];

                    SwapQueue(currentQueue).With<OnlineTaskQueue>();                    
                }; 
           } 
        }

        public Action<UserAccounts.SimpleMembershipUser> OnLoggedOff
        {
            get 
            { 
                return user => 
                {
                    var currentQueue = taskQueues[user.Id];

                    SwapQueue(currentQueue).With<OfflineTaskQueue>();
                }; 
            }
        }

        public Action<UserAccounts.SimpleMembershipUser> OnSessionTimeout
        {
            get { return user => OnLoggedOff(user); }
        }

        private QueueSwapDescriptor SwapQueue(ITaskQueue currentQueue)
        {
            return new QueueSwapDescriptor(taskQueues, currentQueue, queueFactory);
        }

        private class QueueSwapDescriptor
        {
            private ITaskQueue queueToSwap;
            private IDictionary<int, ITaskQueue> queues;
            private IQueueFactory queueFactory;

            public QueueSwapDescriptor(IDictionary<int, ITaskQueue> queues, ITaskQueue queueToSwap, IQueueFactory queueFactory) 
            {
                this.queueToSwap = queueToSwap;
                this.queues = queues;
                this.queueFactory = queueFactory;
            }

            public void With<T>() where T : ITaskQueue
            {
                var newQueue = queueFactory.Create<T>(queueToSwap.UserId);

                newQueue.InitializeFrom(queueToSwap);
                
                queues[queueToSwap.UserId] = newQueue;
                
                queueToSwap = null;
            }
        }
    }
}
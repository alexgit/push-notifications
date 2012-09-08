using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public abstract class BaseTaskQueue : ITaskQueue
    {
        protected IQueueStorage queueStorage;
        private readonly int userId;

        public BaseTaskQueue(int userId, IQueueStorage queueStorage)
        {
            this.userId = userId;
            this.queueStorage = queueStorage;
        }

        public int UserId { get { return userId; } }

        public virtual void Enqueue(UserTask task) 
        {
            queueStorage.Add(task);            
        }

        public virtual bool Remove(Guid taskId) 
        {
            return queueStorage.Remove(taskId);
        }

        public virtual void TaskStarted(Guid taskId, int startedBy)
        {
            var task = queueStorage.Get(taskId);

            if (task == null)
                return;

            task.Start(userId);            
        }

        public virtual UserTask[] GetAll() 
        {
            return queueStorage.GetAll();
        }

        public virtual bool Contains(Guid taskId) 
        {
            return queueStorage.Contains(taskId);
        }

        public virtual void TaskAborted(Guid taskId)
        {
            var task = queueStorage.Get(taskId);
            task.Abort();            
        }

        public virtual void InitializeFrom(ITaskQueue otherQueue)
        {
            var tasks = otherQueue.GetAll();
            queueStorage.Init(tasks);
        }         
    }
}
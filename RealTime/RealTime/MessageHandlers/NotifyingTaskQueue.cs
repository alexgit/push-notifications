using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public abstract class NotifyingTaskQueue : BaseTaskQueue
    {
        private INotifyUsersOfTasks taskNotificationService;
                
        public NotifyingTaskQueue(int userId, INotifyUsersOfTasks taskNotificationService, IQueueStorage queueStorage) : base(userId, queueStorage)
        {            
            this.taskNotificationService = taskNotificationService;            
        }        

        public override void Enqueue(UserTask task) 
        {
            base.Enqueue(task);
            taskNotificationService.NotifyNewTask(UserId, task);
        }

        public override bool Remove(Guid taskId) 
        {
            var removed = base.Remove(taskId);

            if(removed)
                taskNotificationService.NotifyTaskCompleted(UserId, 0, taskId); //TODO: who completed?

            return removed;
        }

        public override void TaskStarted(Guid taskId, int startedBy)
        {
            base.TaskStarted(taskId, startedBy);
            taskNotificationService.NotifyTaskStarted(UserId, startedBy, taskId);
        }

        public override void TaskAborted(Guid taskId)
        {
            base.TaskAborted(taskId);
            taskNotificationService.NotifyTaskAborted(UserId, 0, taskId);
        }
    }
}
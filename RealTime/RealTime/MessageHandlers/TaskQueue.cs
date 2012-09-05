using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealTime.MessageHandlers
{
    public class TaskQueue
    {
        private INotifyUsersOfTasks taskNotificationService;
        private readonly int userId;

        private IList<UserTask> tasks = new List<UserTask>();

        public TaskQueue(int userId, INotifyUsersOfTasks taskNotificationService)
        {            
            this.userId = userId;
            this.taskNotificationService = taskNotificationService;
        }

        public int UserId { get { return userId; } }

        public void Enqueue(UserTask task) 
        {
            tasks.Add(task);
            taskNotificationService.NotifyNewTask(userId, task);
        }

        public bool Remove(Guid taskId) 
        {
            var taskToRemove = tasks.Where(x => x.Id == taskId).First();
            tasks.Remove(taskToRemove);

            taskNotificationService.NotifyTaskCompleted(userId, 0, taskId);

            return true;
        }

        public void TaskStarted(Guid taskId, int startedBy)
        {
            var task = tasks.Where(x => x.Id == taskId).FirstOrDefault();

            if (task == null)
                return;

            task.Start(userId);
            taskNotificationService.NotifyTaskStarted(userId, startedBy, taskId);
        }

        public UserTask[] GetAll() 
        {
            return tasks.ToArray();
        }        

        internal void TaskAborted(Guid taskId)
        {
            var task = tasks.Where(x => x.Id == taskId).First();
            task.Abort();

            taskNotificationService.NotifyTaskAborted(userId, 0, taskId);
        }
    }
}

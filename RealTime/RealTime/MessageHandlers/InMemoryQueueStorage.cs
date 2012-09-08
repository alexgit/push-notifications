using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public class InMemoryQueueStorage : IQueueStorage
    {
        private ISet<UserTask> tasks = new HashSet<UserTask>();

        public void Add(UserTask task)
        {
            tasks.Add(task);
        }

        public UserTask Get(Guid taskId)
        {
            return tasks.FirstOrDefault(x => x.Id == taskId);
        }

        public bool Remove(Guid taskId)
        {
            return tasks.Remove(Get(taskId));
        }

        public bool Contains(Guid taskId)
        {
            return tasks.Contains(Get(taskId));
        }

        public UserTask[] GetAll()
        {
            return tasks.ToArray();
        }
        
        public void Init(params UserTask[] userTasks)
        {
            tasks = new HashSet<UserTask>(userTasks);
        }
    }
}
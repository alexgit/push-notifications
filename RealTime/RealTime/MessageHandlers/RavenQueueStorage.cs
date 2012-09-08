using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client;

namespace RealTime.MessageHandlers
{
    public class RavenQueueStorage : IQueueStorage
    {
        private readonly IDocumentStore documentStore;
        private readonly int userId;

        public RavenQueueStorage(int userId, IDocumentStore documentStore) 
        {
            this.userId = userId;
            this.documentStore = documentStore;
        }

        public void Add(UserTask task)
        {
            using (var session = documentStore.OpenSession())
            {
                var queue = session.Load<RavenTaskQueue>(userId);
                queue.Tasks.Add(task);                
                session.SaveChanges();
            }
        }

        public UserTask Get(Guid taskId)
        {
            using (var session = documentStore.OpenSession())
            {
                return session.Load<RavenTaskQueue>(userId).Tasks.FirstOrDefault(x => x.Id == taskId);
            }
        }

        public bool Remove(Guid taskId)
        {
            using (var session = documentStore.OpenSession())
            {
                var queue = session.Load<RavenTaskQueue>(userId);
                queue.Tasks.Remove(queue.Tasks.First(x => x.Id == taskId));
                session.SaveChanges();
            }

            return true;            
        }

        public bool Contains(Guid taskId)
        {
            return Get(taskId) != null;
        }

        public UserTask[] GetAll()
        {
            using (var session = documentStore.OpenSession())
            {
                return session.Load<RavenTaskQueue>(userId).Tasks.ToArray();
            }
        }

        public void Init(params UserTask[] userTasks)
        {
            using (var session = documentStore.OpenSession())
            {
                var queue = session.Load<RavenTaskQueue>(userId) ?? new RavenTaskQueue(userId);
                    
                if(userTasks.Count() > 0)
                    queue.Tasks = new HashSet<UserTask>(userTasks);

                session.Store(queue);
                session.SaveChanges();
            }
        }

        private class RavenTaskQueue
        {
            public RavenTaskQueue() 
            {

            }

            public RavenTaskQueue(int userId) : this(userId, new HashSet<UserTask>())
            {

            }

            public RavenTaskQueue(int userId, IEnumerable<UserTask> tasks)
            {
                Id = userId;
                Tasks = new HashSet<UserTask>(tasks);
            }            

            public int Id { get; set; }
            public ISet<UserTask> Tasks { get; set; }
        }
    }    
}
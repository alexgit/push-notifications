using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client;

namespace RealTime.MessageHandlers
{
    public class DefaultQueueFactory : IQueueFactory
    {
        private INotifyUsersOfTasks taskNotifier;
        private IDocumentStore documentStore;

        public DefaultQueueFactory(INotifyUsersOfTasks taskNotifier, IDocumentStore documentStore)
        {
            this.taskNotifier = taskNotifier;
            this.documentStore = documentStore;
        }
        
        public ITaskQueue Create<T>(int userId) where T : ITaskQueue
        {
            if (typeof(T) == typeof(OnlineTaskQueue)) 
            {
                return new OnlineTaskQueue(userId, taskNotifier);
            }
            else if (typeof(T) == typeof(OfflineTaskQueue))
            {
                return new OfflineTaskQueue(userId, new RavenQueueStorage(userId, documentStore));
            }

            throw new ArgumentException(string.Format("don't know how to create a instance of {0}", typeof(T)));
        }
    }
}
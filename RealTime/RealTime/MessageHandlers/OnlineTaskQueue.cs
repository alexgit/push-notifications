using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealTime.MessageHandlers
{
    public class OnlineTaskQueue : NotifyingTaskQueue
    {
        public OnlineTaskQueue(int userId, INotifyUsersOfTasks taskNotifier) : this(userId, taskNotifier, new InMemoryQueueStorage())
        {            
        }

        public OnlineTaskQueue(int userId, INotifyUsersOfTasks taskNotifier, IQueueStorage queueStorage) : base(userId, taskNotifier, queueStorage) 
        {
        }        
    }
}

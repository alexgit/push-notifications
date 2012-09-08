using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public class OfflineTaskQueue : BaseTaskQueue
    {
        public OfflineTaskQueue(int userId, RavenQueueStorage queueStorage) : base(userId, queueStorage)
        {
            queueStorage.Init();
        }
    }
}
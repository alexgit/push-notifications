using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public interface IQueueStorage
    {
        void Add(UserTask task);
        UserTask Get(Guid taskId);
        bool Remove(Guid taskId);
        bool Contains(Guid taskId);
        UserTask[] GetAll();

        void Init(params UserTask[] userTasks);
    }
}
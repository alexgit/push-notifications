using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public interface IQueueFactory
    {
        ITaskQueue Create<T>(int userId) where T : ITaskQueue;
    }
}
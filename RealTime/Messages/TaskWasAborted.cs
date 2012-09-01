using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface TaskWasAborted : IMessage
    {
        Guid TaskId { get; set; }
        int AbortedBy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface TaskWasStarted : IMessage
    {
        Guid TaskId { get; set; }
        int StartedBy { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface TaskAlreadyCompleted : IMessage
    {
        Guid TaskId { get; set; }
        int CompletedBy { get; set; }
    }
}

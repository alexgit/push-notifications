using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface TaskWasCreated : IMessage
    {
        Guid TaskId { get; set; }

        string Description { get; set; }

        string ActionURL { get; set; }

        int[] Users { get; set; }
    }
}

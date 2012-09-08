using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface ICreateANewTask : IMessage
    {
        Guid CorrelationId { get; set; }
        string Description { get; set; }
        string ActionUrl { get; set; }
        int[] Users { get; set; }
    }
}

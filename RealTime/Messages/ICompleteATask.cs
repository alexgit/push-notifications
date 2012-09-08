using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface ICompleteATask : IMessage
    {
        Guid CorrelationId { get; set; }
        int CompletedBy { get; set; }
    }
}

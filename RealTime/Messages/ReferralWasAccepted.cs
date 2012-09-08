using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface ReferralWasAccepted : IMessage, ICompleteATask
    {
        Guid ReferralId { get; set; }
        
        int AcceptedByTeam { get; set; }

        int AcceptedByUser { get; set; }
    }
}

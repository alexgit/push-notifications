using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;

namespace Messages
{
    public interface ClientWasReferredFromLASARTeam : IMessage
    {
        int ClientId { get; set; }
        Guid ReferralId { get; set; }

        string Forename { get; set; }
        string Surname { get; set; }

        string ReferredFor { get; set; }
        DateTime ReferralDate { get; set; }
        string PrimaryProblemSubstance { get; set; }
        string Referrer { get; set; }

        int FromTeamId { get; set; }
        int ToTeamId { get; set; }
    }
}

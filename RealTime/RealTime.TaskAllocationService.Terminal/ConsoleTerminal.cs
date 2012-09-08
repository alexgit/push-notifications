using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using Messages;

namespace RealTime.TaskAllocationService.Terminal
{
    public class ConsoleTerminal : IWantToRunAtStartup
    {
        public IBus Bus { get; set; }

        public void Run()
        {
            var line = string.Empty;

            while (!(new [] { "exit", "quit" }).Contains(line = Console.ReadLine())) 
            {
                if (line.StartsWith("complete"))
                {
                    var parts = line.Split(' ');

                    var correlationId = Guid.Parse(parts[1]);
                    var userId = int.Parse(parts[2]);

                    Bus.Publish<ReferralWasAccepted>(m =>
                    {
                        m.ReferralId = Guid.NewGuid();
                        m.AcceptedByTeam = 1;
                        m.AcceptedByUser = userId;

                        m.CompletedBy = userId;
                        m.CorrelationId = correlationId;
                    });
                }
                else if(line.StartsWith("new"))
                {

                    var data = line.Split(' ');

                    var referralId = Guid.Parse(data[1]);

                    Bus.Publish<ClientWasReferred>(m =>
                    {
                        m.ClientId = 1;
                        m.Forename = "Joe";
                        m.FromTeamId = 2;
                        m.ToTeamId = 1;
                        m.Surname = "Pesci";
                        m.Referrer = "Test Referrer";
                        m.ReferralDate = DateTime.Now;
                        m.ReferralId = referralId;
                        m.PrimaryProblemSubstance = "Crack";

                        m.ActionUrl = "actionUrl";
                        m.CorrelationId = referralId;
                        m.Description = "Client XYZ was referred from team ABC.";
                        m.Users = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
                    });
                }                
            }
        }

        public void Stop()
        {
            Console.Clear();
        }
    }
}

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

                    var taskId = Guid.Parse(parts[1]);
                    var userId = int.Parse(parts[2]);

                    Bus.Publish<ReferralWasAccepted>(m =>
                    {
                        m.TaskId = taskId; //todo: correlation here needs to happen on the referral id, not on the taskid
                        m.ReferralId = Guid.NewGuid();
                        m.AcceptedByTeam = 1;
                        m.AcceptedByUser = userId;
                    });
                }
                else if(line.StartsWith("new"))
                {
                    Bus.Publish<ClientWasReferred>(m =>
                    {
                        m.ClientId = 1;
                        m.Forename = "Joe";
                        m.FromTeamId = 2;
                        m.ToTeamId = 1;
                        m.Surname = "Pesci";
                        m.Referrer = "Test Referrer";
                        m.ReferralDate = DateTime.Now;
                        m.ReferralId = Guid.NewGuid();
                        m.PrimaryProblemSubstance = "Crack";
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

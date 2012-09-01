using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using NServiceBus.Saga;
using Messages;


namespace TaskAllocationService.MessageHandlers
{
    public class TaskSaga : Saga<Task>, ISagaStartedBy<ClientWasReferredFromLASARTeam>,
                                            IHandleMessages<StartTask>,
                                                IHandleMessages<AbortTask>, 
                                                    IHandleMessages<LASARReferralWasAccepted>,
                                                        IHandleTimeouts<TaskTimeout>
    {
        private readonly ITeamService teamService;
        private readonly IBus messageBus;
        
        public TaskSaga(ITeamService userLocationService) 
        {
            this.teamService = userLocationService;
        }

        public void Handle(ClientWasReferredFromLASARTeam message)
        {
            var taskId = Guid.NewGuid();
            this.Data = new Task
            {
                TaskId = taskId,
                ActionURL = string.Format("http://environment/providerservice/acceptlasarreferral.castle?taskId={0}&referralId={1}", taskId, message.ReferralId),
                Description = "This is a LASAR referral from Team A bla bla",
                Users = teamService.GetUsersForTeam(message.ToTeamId)
            };            

            messageBus.Publish<TaskWasCreated>(m => {
                m.TaskId = Data.TaskId;
                m.Users = Data.Users;
                m.Description = Data.Description;
                m.ActionURL = Data.ActionURL;
            });
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<TaskWasStarted>(x => x.TaskId, y => y.TaskId);
            ConfigureMapping<TaskWasAborted>(x => x.TaskId, y => y.TaskId);
            ConfigureMapping<LASARReferralWasAccepted>(x => x.TaskId, y => y.TaskId);
        }

        public void Handle(StartTask message)
        {
            if (Data.InProgress)
            {
                ReplyToOriginator<TaskAlreadyStarted>(m => { 
                    m.TaskId = message.TaskId; 
                    m.StartedBy = Data.HandledBy.Value; 
                });
            }
            else if (Data.Completed) 
            {
                ReplyToOriginator<TaskAlreadyCompleted>(m => { 
                    m.TaskId = message.TaskId; 
                    m.CompletedBy = Data.CompletedBy.Value; 
                });
            }
            else 
            {
                Data.Start(message.UserId);

                RequestUtcTimeout<TaskTimeout>(TimeSpan.FromMinutes(30));

                messageBus.Publish<TaskWasStarted>(m => {
                    m.TaskId = message.TaskId;
                    m.StartedBy = message.UserId;
                });
            }
        }

        public void Handle(AbortTask message)
        {
            Data.Abort(message.UserId);

            messageBus.Publish<TaskWasAborted>(m =>
            {
                m.TaskId = message.TaskId;
                m.AbortedBy = message.UserId;
            });
        }

        public void Handle(LASARReferralWasAccepted message)
        {
            Data.Complete(message.AcceptedByUser);
            MarkAsComplete();

            messageBus.Publish<TaskWasCompleted>(m => {
                m.TaskId = message.TaskId;
                m.CompletedBy = message.AcceptedByUser;
            });
        }

        public void Timeout(TaskTimeout state)
        {
            var taskId = Data.TaskId;
            var userId = Data.HandledBy.Value;

            Data.Abort(Data.HandledBy.Value);

            messageBus.Publish<TaskWasAborted>(m =>
            {
                m.TaskId = taskId;
                m.AbortedBy = userId;
            });
        }
    }
}

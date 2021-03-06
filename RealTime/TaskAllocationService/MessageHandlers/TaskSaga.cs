﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using NServiceBus.Saga;
using Messages;


namespace TaskAllocationService.MessageHandlers
{
    public class TaskSaga : Saga<TaskData>, ISagaStartedBy<ICreateANewTask>,
                                            IHandleMessages<StartTask>,
                                                IHandleMessages<AbortTask>, 
                                                    IHandleMessages<ICompleteATask>,
                                                        IHandleTimeouts<TaskTimeout>
    {
        private readonly ITeamService teamService;
        private readonly IBus messageBus;

        public TaskSaga() 
        {

        }

        public TaskSaga(ITeamService userLocationService, IBus messageBus) 
        {
            this.teamService = userLocationService;
            this.messageBus = messageBus;
        }

        public void Handle(ICreateANewTask message)
        {           
            var taskId = Guid.NewGuid();
            this.Data = new TaskData
            {
                TaskId = taskId,
                ActionURL = message.ActionUrl,
                Description = message.Description,
                Users = message.Users,
                CorrelationId = message.CorrelationId
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
            ConfigureMapping<StartTask>(x => x.TaskId, y => y.TaskId);
            ConfigureMapping<AbortTask>(x => x.TaskId, y => y.TaskId);
            ConfigureMapping<ICompleteATask>(x => x.CorrelationId, y => y.CorrelationId);
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

        public void Handle(ICompleteATask message)
        {
            Data.Complete(message.CompletedBy);
            MarkAsComplete();

            messageBus.Publish<TaskWasCompleted>(m => {
                m.TaskId = Data.TaskId;
                m.CompletedBy = message.CompletedBy;
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

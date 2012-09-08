using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NServiceBus;
using NServiceBus.Saga;

namespace TaskAllocationService
{
    public class TaskData : IContainSagaData
    {
        public Guid Id { get; set; }
        
        public string OriginalMessageId { get; set; }

        public string Originator { get; set; }

        public Guid CorrelationId { get; set; }

        public Guid TaskId { get; set; }

        public string Description { get; set; }

        public string ActionURL { get; set; }

        public int[] Users { get; set; }

        public DateTime? DateCompleted { get; private set; }

        public bool Completed { get; private set; }

        public int? CompletedBy { get; private set; }

        public int? HandledBy { get; private set; }
        
        public bool InProgress { get; private set; }

        public void Complete(int userId) 
        {
            if (InProgress && HandledBy != userId)
                throw new InvalidOperationException("A task a started by one user cannot be completed by another.");

            if (Completed)
                throw new InvalidOperationException("Called Complete() on an already completed task.");

            ClearInProgress();

            this.Completed = true;
            this.CompletedBy = userId;
            this.DateCompleted = DateTime.Now;
        }        

        public void Start(int userId) 
        {
            InProgress = true;
            HandledBy = userId;
        }

        public void Abort(int userId) 
        {
            ClearInProgress();
        }

        public void ClearInProgress() 
        {
            InProgress = false;
            HandledBy = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public class UserTask
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string ActionUrl { get; set; }

        public bool InProgress { get; private set; }
        public int? HandledBy { get; private set; }

        public void Start(int userId)
        {
            HandledBy = userId;
            InProgress = true;
        }

        public void Abort()
        {
            HandledBy = null;
            InProgress = false;
        }
    }
}
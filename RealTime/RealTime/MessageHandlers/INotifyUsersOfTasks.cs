using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RealTime.MessageHandlers
{
    public interface INotifyUsersOfTasks
    {
        void NotifyNewTask(int userToNotify, UserTask task);

        void NotifyTaskStarted(int userToNotify, int userWhoStartedtask, Guid taskId);

        void NotifyTaskAborted(int userToNotify, int userWhoAbortedTask, Guid taskId);

        void NotifyTaskCompleted(int userToNotify, int userWhoCompletedTask, Guid taskId);
    }
}
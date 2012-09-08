using System;
namespace RealTime.MessageHandlers
{
    public interface ITaskQueue
    {
        bool Contains(Guid taskId);
        void Enqueue(UserTask task);
        UserTask[] GetAll();
        bool Remove(Guid taskId);
        void TaskStarted(Guid taskId, int startedBy);
        void TaskAborted(Guid taskId);
        int UserId { get; }

        void InitializeFrom(ITaskQueue otherQueue);
    }
}

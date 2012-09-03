using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RealTime.MessageHandlers
{
    public interface IUserAccountService
    {
        int[] GetAllUserIds();
    }

    public class DummyUserAccountService : IUserAccountService
    {
        public int[] GetAllUserIds()
        {
            return Enumerable.Range(1, 20).ToArray();
        }
    }
}

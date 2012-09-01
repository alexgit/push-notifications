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
}

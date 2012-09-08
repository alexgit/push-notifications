using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RealTime.UserAccounts;

namespace RealTime.Infrastructure
{
    public interface IReactToUserLoggedOff
    {
        Action<SimpleMembershipUser> OnLoggedOff { get; }
    }
}

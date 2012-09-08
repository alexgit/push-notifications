using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RealTime.UserAccounts;

namespace RealTime.Infrastructure
{
    public interface IReactToUserLoggedIn 
    {
        Action<SimpleMembershipUser> OnLogin { get; }
    }
}
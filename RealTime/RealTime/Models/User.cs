using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace RealTime.Models
{
    public class User : IPrincipal
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public IIdentity Identity { get; set; }

        public User(int userId, string username)
        {
            Id = userId;
            Identity = new GenericIdentity(username);
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }
}
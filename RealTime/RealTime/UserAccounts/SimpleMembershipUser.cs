using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace RealTime.UserAccounts
{
    public class SimpleMembershipUser : MembershipUser
    {
        public SimpleMembershipUser(UserDTO userDTO) : base(userDTO.ProviderName, userDTO.UserName, userDTO.ProviderUserKey, userDTO.Email,
            userDTO.PasswordQuestion, userDTO.Comment, userDTO.IsApproved, userDTO.IsLockedOut, userDTO.CreationDate, userDTO.LastLoginDate, userDTO.LastActivityDate,
                userDTO.LastPasswordChangedDate, userDTO.LastLockoutDate) 
        {
            Password = userDTO.Password;
            Id = userDTO.Id;
        }

        public string Password { get; private set; }

        public int Id { get; private set; }

        public override string GetPassword()
        {
            return Password;
        }

        public override string GetPassword(string passwordAnswer)
        {
            return GetPassword();
        }

        public override string ResetPassword()
        {
            return Password = "newPassword";
        }

        public override string ResetPassword(string passwordAnswer)
        {
            return ResetPassword();
        }
    }

    public struct UserDTO 
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastActivityDate { get; set; }
        public DateTime LastLockoutDate { get; set; }
        public DateTime LastLoginDate { get; set; }
        public DateTime LastPasswordChangedDate { get; set; }
        public string PasswordQuestion { get; set; }
        public string ProviderName { get; set; }
        public object ProviderUserKey { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }        
    }
}
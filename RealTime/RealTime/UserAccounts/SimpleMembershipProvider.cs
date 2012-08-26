using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace RealTime.UserAccounts
{
    public class SimpleMembershipProvider : MembershipProvider
    {
        private static string userAccountsFilename = @"c:\users.db";

        private static IDictionary<string, MembershipUser> userDb = new ConcurrentDictionary<string, MembershipUser>();

        static SimpleMembershipProvider() 
        {
            InitializeUserDb();
        }

        private static void InitializeUserDb() 
        {
            var lines = new List<string>();
            using (var reader = new StreamReader(userAccountsFilename))
            {
                var line = string.Empty;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            foreach (var l in lines)
            {
                var tuple = l.Split(new[] { "=>" }, StringSplitOptions.None);

                var username = tuple[0].Trim();
                var jsonSerializedUser = tuple[1].Trim();

                var userDTO = JsonConvert.DeserializeObject<UserDTO>(jsonSerializedUser);
                
                userDb.Add(username, new SimpleMembershipUser(userDTO));
            }
        }

        private void SaveChanges() 
        {
            using (var writer = new StreamWriter(userAccountsFilename, append: false))
            {
                foreach (var kvp in userDb) 
                {
                    var username = kvp.Key;
                    var serializedUser = JsonConvert.SerializeObject(kvp.Value);
                    writer.WriteLine(string.Format("{0}=>{1}", username, serializedUser));
                }
            }
        }

        public override string ApplicationName
        {
            get
            {
                return "RealTime";
            }
            set
            {
                
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            var success = userDb[username].ChangePassword(oldPassword, newPassword);

            if (success)
                SaveChanges();

            return success;
        }
        
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            var success = userDb[username].ChangePasswordQuestionAndAnswer(password, newPasswordQuestion, newPasswordAnswer);

            if (success)
                SaveChanges();

            return success;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var userDTO = new UserDTO 
            {
                ProviderName = "SimpleMembershipProvider",
                UserName = username,
                Password = password,
                ProviderUserKey = providerUserKey, 
                Email = email, 
                PasswordQuestion = passwordQuestion, 
                Comment = string.Empty, 
                IsApproved = isApproved, 
                IsLockedOut = false, 
                LastActivityDate = DateTime.Now,
                LastLockoutDate = DateTime.MinValue,
                LastLoginDate = DateTime.Now,
                LastPasswordChangedDate = DateTime.MinValue
            };
            var newUser = new SimpleMembershipUser(userDTO);

            status = MembershipCreateStatus.Success;

            userDb.Add(username, newUser);

            SaveChanges();

            return newUser;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            try
            {
                userDb.Remove(username);
            }
            catch
            {
                return false;
            }

            SaveChanges();
            return true;
        }

        public override bool EnablePasswordReset
        {
            get { return false; }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return false; }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            var coll = new MembershipUserCollection();

            foreach (var user in userDb.Values) 
            {
                coll.Add(user);
            }

            totalRecords = userDb.Values.Count;

            return coll;
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            return userDb[username].GetPassword(answer);
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return userDb[username];
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return 10; }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return 0; }
        }

        public override int MinRequiredPasswordLength
        {
            get { return 1; }
        }

        public override int PasswordAttemptWindow
        {
            get { return 30; }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { return MembershipPasswordFormat.Clear; }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return false; }
        }

        public override string ResetPassword(string username, string answer)
        {
            var newpassword = userDb[username].ResetPassword(answer);

            SaveChanges();

            return newpassword;
        }

        public override bool UnlockUser(string userName)
        {
            return userDb[userName].UnlockUser();
        }

        public override void UpdateUser(MembershipUser user)
        {
            userDb[user.UserName] = user;

            SaveChanges();
        }

        public override bool ValidateUser(string username, string password)
        {
            MembershipUser user = null;
            try { user = userDb[username]; }
            catch { }

            return user != null && user.GetPassword() == password;
        }        
    }
}
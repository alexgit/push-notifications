using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.IO;
using Newtonsoft.Json;

namespace RealTime.UserAccounts
{
    public class SimpleMembershipProvider : MembershipProvider
    {
        private static string userAccountsFilename = "users.db";

        private IDictionary<string, MembershipUser> userDb;

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            base.Initialize(name, config);

            userDb = new Dictionary<string, MembershipUser>();

            var lines = new List<string>();
            using(var reader = new StreamReader(userAccountsFilename)) 
            {                
                var line = string.Empty;
                while ((line = reader.ReadLine()) != null) 
                {
                    lines.Add(line);
                }
            }

            foreach (var l in lines) 
            {
                var tuple = l.Split(':');

                var username = tuple[0];
                var jsonSerializedUser = tuple[1];

                var user = JsonConvert.DeserializeObject<MembershipUser>(jsonSerializedUser);

                userDb.Add(username, user);
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
                    writer.WriteLine(string.Format("{0}:{1}", username, serializedUser));
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
            var newUser = new MembershipUser("SimpleProvider", username, providerUserKey, email, passwordQuestion, string.Empty, isApproved, false, DateTime.Now, DateTime.Now, DateTime.Now, DateTime.MinValue, DateTime.MinValue);

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
            return true;
        }        
    }
}
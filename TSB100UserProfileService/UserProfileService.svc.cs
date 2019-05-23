using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using Serilog;
using TSB100UserProfileService.DataTransferObjects;
using TSB100UserProfileService.Mapping;

namespace TSB100UserProfileService
{

    public class UserProfileService : IUserProfileService
    {
        private UserProfileEntities db;
        private UserMapper _mapper;
        private UserValidator _validator;

        public UserProfileService()
        {
            db = new UserProfileEntities();
            _mapper = new UserMapper();
            _validator = new UserValidator();
        }

        //----------------------------------------------------------------------------------------
        // Create & Update
        //----------------------------------------------------------------------------------------
        public User CreateUser(NewUser newUserFromWeb)
        {
            Log.Information($"In USerProfileService.CreateUser(): Request recieved with NewUser {JsonConvert.SerializeObject(newUserFromWeb)}");

            if (!_validator.ValidateNewUser(newUserFromWeb))
            {
                return null;
            }

            int userId;

            using (var loginService = new LoginServiceRef.LoginServiceClient())
            {
                var newUser = new LoginServiceRef.NewUser
                {
                    Email = newUserFromWeb.Email,
                    Firstname = newUserFromWeb.FirstName,
                    Surname = newUserFromWeb.Surname,
                    Password = newUserFromWeb.Password,
                    Username = newUserFromWeb.Username
                };

                var returnUser = loginService.CreateUser(newUser);

                if (returnUser == null)
                {
                    // Logging that something went wrong when trying to save the new user in the other service
                    Log.Warning($"An attempt to create an account with the following values failed: {JsonConvert.SerializeObject(newUser)}");

                    return null;
                }

                // Catches the newly created user's id
                userId = returnUser.ID;
            }

            using (db)
            {
                var userDb = new UserDb();
                db.UserDb.Add(userDb);

                _mapper.MapNewUserToModel(newUserFromWeb, userDb);
                userDb.UserId = userId;

                if (!UpdateDatabase())
                {
                    // Logging that something went wrong when trying to save the new user in the other service
                    Log.Warning($"An attempt to create a profile with the following values failed: {JsonConvert.SerializeObject(newUserFromWeb)}");
                    return null;
                }

                // Return a User object so that one may add more profile data
                var user = _mapper.MapToWebService(userDb);
                return user;
            }
        }

        public bool UpdateUser(User user)
        {
            Log.Information($"In USerProfileService.UpdateUser(): Request recieved with User {JsonConvert.SerializeObject(user)}");
            // TODO: Login Service does not have an UpdateUser() function - cannot pass parameters (password, email, username, first and lastname) to that service, so that they are updated there
            // TODO: validate like with NewUser (but with the User class)

            if (!_validator.ValidateUser(user))
            {
                return false;
            }

            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.UserId == user.Id
                              select u).FirstOrDefault();

                if (dbUser == null)
                {
                    // TODO: Create logging
                    return false;
                }

                _mapper.MapUserToModel(user, dbUser);
                db.Entry(dbUser).State = EntityState.Modified;

                // Saves the changes that have been made, and returns true if succeeded, and false if not
                return UpdateDatabase();
            }
        }

        //----------------------------------------------------------------------------------------
        // Delete
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes a userProfile from database (but not from the login service).
        /// </summary>
        public bool DeleteUserProfile(int userId)
        {
            Log.Information($"In USerProfileService.DeleteUserProfile(): Request recieved with userId {userId}");
            using (db)
            {
                // Linq expression using expression:
                //var dbUser = (from u in db.UserDb
                //              where u.UserId == userId
                //              select u).FirstOrDefault();

                // Linq expression using fluent code:
                var dbUser = db.UserDb
                    .Where(u => u.UserId == userId)
                    .FirstOrDefault();

                if (dbUser == null)
                {
                    Log.Warning($"In USerProfileService.DeleteUserProfile(): Unable to delete userProfile with userId {userId}. UserId not found in database");
                    return false;
                }
                db.Entry(dbUser).State = EntityState.Deleted;

                // Saves the changes that have been made, and returns true if succeeded, and false if not
                var profileDeleted = UpdateDatabase();
                if (!profileDeleted)
                {
                    Log.Warning($"In USerProfileService.DeleteUserProfile(): Unable to delete userProfile with userId {userId}. USerProfileService.UpdateDatabase returned false");
                }
                return profileDeleted;
            }
        }


        /// <summary>
        /// Deletes a userProfile from database AND from the login service.
        /// </summary>
        public bool DeleteUser(int userId)
        {
            Log.Information($"In USerProfileService.DeleteUser(): Request recieved with userId {userId}");

            // Delete user from loginService
            var loginDeleted = false;
            using (var loginService = new LoginServiceRef.LoginServiceClient())
            {
                if (!loginService.UserIdExist(userId))
                {
                    Log.Warning($"In USerProfileService.DeleteUser(): Unable to delete user with userId {userId}. UserId not found in loginService");
                }
                else
                {
                    loginDeleted = loginService.DeleteUser(userId);
                    if (!loginDeleted)
                    {
                        Log.Warning($"In USerProfileService.DeleteUser(): Unable to delete user with userId {userId}. loginService.DeleteUser(userId) returned false");
                    }
                }
            }
            // Delete userProfile from database
            var profileDeleted = DeleteUserProfile(userId);

            return loginDeleted && profileDeleted;
        }

        //----------------------------------------------------------------------------------------
        // Get
        //----------------------------------------------------------------------------------------
        public bool UserIdExistsInProfile(int userId)
        {
            Log.Information($"In USerProfileService.UserIdExistsInProfile(): Request recieved with userId {userId}");
            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.UserId == userId
                              select u).FirstOrDefault();
                return (dbUser != null);
            }
        }

        public bool EmailExistsInProfile(string email)
        {
            Log.Information($"In USerProfileService.EmailExistsInProfile(): Request recieved with email {email}");
            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.Email == email
                              select u).FirstOrDefault();
                return (dbUser != null);
            }
        }

        public bool UserNameExistsInProfile(string userName)
        {
            Log.Information($"In USerProfileService.EmailExistsInProfile(): Request recieved with userName {userName}");
            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.Username == userName
                              select u).FirstOrDefault();
                return (dbUser != null);
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            Log.Information($"In USerProfileService.GetAllUsers(): Request recieved");
            using (db)
            {
                var users = new List<User>();
                var dbUsers = db.UserDb.ToList();

                foreach (var dbUser in dbUsers)
                {
                    var user = _mapper.MapToWebService(dbUser);

                    if (user == null)
                    {
                        // TODO: Create logging
                        return null;
                    }
                    users.Add(user);
                }
                return users;
            }
        }

        public User GetUserByUserNameOrEmail(string userName)
        {
            Log.Information($"In USerProfileService.GetUserByUserName(): Request recieved with userName {userName}");
            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.Username == userName
                              select u).FirstOrDefault();
                if (dbUser == null)
                {
                    dbUser = (from u in db.UserDb
                              where u.Email == userName
                              select u).FirstOrDefault();
                    if (dbUser == null)
                    {
                        Log.Warning($"In USerProfileService.GetUserByUserName(): No user found with userName or email {userName}");
                    }
                }
                return _mapper.MapToWebService(dbUser);
            }
        }

        public User GetUserByUserId(int userId)
        {
            Log.Information($"In USerProfileService.GetUserByUserId(): Request recieved with userId {userId}");
            using (db)
            {
                var dbUser = (from u in db.UserDb
                              where u.UserId == userId
                              select u).FirstOrDefault();
                if (dbUser == null)
                {
                    Log.Warning($"In USerProfileService.GetUserByUserId(): No user found with userId {userId}");
                }

                return _mapper.MapToWebService(dbUser);
            }
        }

        //----------------------------------------------------------------------------------------
        // Log functions
        //----------------------------------------------------------------------------------------
        public string GetLatestLog()
        {
            Log.Information($"In USerProfileService.GetTodaysLog(): Request recieved");
            var lastLogFileName = FileReader.SingletonReader.GetDirectory(@"C:\logs\").Last();
            if (lastLogFileName == null)
            {
                return "Error: No log file found";
            }
            var logFileLines = FileReader.SingletonReader.GetTextFileLines(lastLogFileName);
            if (logFileLines == null)
            {
                return "Error: No log file found";
            }

            var logFile = string.Join(System.Environment.NewLine, logFileLines);
            //Debug.WriteLine(logFile);
            return logFile;
        }


        //----------------------------------------------------------------------------------------
        // Helper methods
        //----------------------------------------------------------------------------------------
        #region helperMethods

        /// <summary>
        /// Saves the changes that have been made, and returns true if succeeded, and false if not
        /// </summary>
        /// <returns></returns>
        private bool UpdateDatabase()
        {
            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception ex) when (
                ex is DbUpdateException
                || ex is DbUpdateConcurrencyException
                || ex is DbEntityValidationException
                || ex is NotSupportedException
                || ex is ObjectDisposedException
                || ex is InvalidOperationException
                )
            {
                // An database exception deserves a higher error level than warning. Let's do error level: Error
                Log.Error($"In USerProfileService.UpdateDatabase(): Unable to apply changes in database. Exception of type {ex.GetType().Name} was thrown. Exception: {JsonConvert.SerializeObject(ex)}");
                return false;
            }
        }
        #endregion
    }
}

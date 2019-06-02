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
using TSB100UserProfileService.LoginServiceRef;
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
        public User CreateUser(TSB100UserProfileService.DataTransferObjects.NewUser newUserFromWeb)
        {
            Log.Information($"In USerProfileService.CreateUser(): Request recieved with NewUser {JsonConvert.SerializeObject(newUserFromWeb)}");

            // Step 1: Validate input data
            if (!_validator.ValidateNewUser(newUserFromWeb))
            {
                return null;
            }

            int userId;

            // Step 2: Save NewUser to LoginService
            using (var loginService = new LoginServiceRef.LoginServiceClient())
            {
                LoginServiceRef.NewUser newUser = _mapper.MapDTONewUserToLoginServiceNewUser(newUserFromWeb);
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

            // Step 3: Save NewUser to database
            using (db)
            {
                var userDb = new UserDb();
                db.UserDb.Add(userDb);

                _mapper.MapNewUserToUserDb(newUserFromWeb, userDb);
                userDb.UserId = userId;

                if (!UpdateDatabase())
                {
                    // Logging that something went wrong when trying to save the new user in the other service
                    Log.Warning($"An attempt to create a profile with the following values failed: {JsonConvert.SerializeObject(newUserFromWeb)}");
                    return null;
                }

                // Return a User object so that one may add profile data
                var user = _mapper.MapUserDbToUser(userDb);
                return user;
            }
        }

        public bool UpdateUser(User user)
        {
            Log.Information($"In USerProfileService.UpdateUser(): Request recieved with User {JsonConvert.SerializeObject(user)}");
            // Step 1: Validate input data
            if (!_validator.ValidateUser(user))
            {
                return false;
            }


            // Step 2: Update user in LoginService
            using (var loginService = new LoginServiceRef.LoginServiceClient())
            {
                // If user doesn't exist in LoginService then act accordingly
                if (!loginService.UserIdExist(user.Id))
                {
                    Log.Warning($"In USerProfileService.UpdateUser(): Call to LoginServiceClient.UserIdExist() failed. UserId {user.Id} not found.");
                    if (!UserIdExistsInProfile(user.Id))
                    {
                        return false;
                    }
                    // I a user doess't exist in the loginService, but does exist in our database
                    // then it should also be deleted from our database;
                    if (DeleteUserProfile(user.Id))
                    {
                        Log.Warning($"In USerProfileService.UpdateUser(): Unexpected UserId {user.Id} found in local database. User is now deleted");
                    };
                    return false;
                }
                // Get current user data from LoginService for rollback
                ReturnUser oldLoginServiceUser = _mapper.MapInterfaceUserToReturnUser(loginService.GetUserById(user.Id));

                ReturnUser updatedUser = _mapper.MapUserToReturnUser(user);
                if (!loginService.UpdateAccountInfo(updatedUser))
                {
                    Log.Error($"In USerProfileService.UpdateUser(): Call to LoginServiceClient.UpdateAccountInfo failed. Unable to update user profile.");
                    return false;
                };



                // Step3: Update user in local database
                using (db)
                {
                    var userDb = db.UserDb.FirstOrDefault(u => u.UserId == user.Id);

                    // If no user exists in dbContext, then add one, since there is a user in LoginService
                    if (userDb == null)
                    {
                        userDb = new UserDb();
                        db.UserDb.Add(userDb);
                    }

                    _mapper.MapUserToUserDb(user, userDb);
                    db.Entry(userDb).State = EntityState.Modified;

                    // Save the changes that have been made, and return true if all is well, and false if not
                    if (!UpdateDatabase())
                    {
                        // rollback change to user in LogInService
                        if (!loginService.UpdateAccountInfo(oldLoginServiceUser))
                        {
                            Log.Error($"In USerProfileService.UpdateUser(): Call to LoginServiceClient.UpdateAccountInfo failed. Unable to rollback user profile.");
                            return false;
                        };
                        return false;
                    };
                }
            }
            // All is well. Report successful update
            return true;
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
                //var userDb = (from u in db.UserDb
                //              where u.UserId == userId
                //              select u).FirstOrDefault();

                // Linq expression using fluent code:
                var userDb = db.UserDb.FirstOrDefault(u => u.UserId == userId);


                if (userDb == null)
                {
                    Log.Warning($"In USerProfileService.DeleteUserProfile(): Unable to delete userProfile with userId {userId}. UserId not found in database");
                    return false;
                }
                db.Entry(userDb).State = EntityState.Deleted;

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
                var userDb = (from u in db.UserDb
                              where u.UserId == userId
                              select u).FirstOrDefault();
                return (userDb != null);
            }
        }

        public bool EmailExistsInProfile(string email)
        {
            Log.Information($"In USerProfileService.EmailExistsInProfile(): Request recieved with email {email}");
            using (db)
            {
                var userDb = (from u in db.UserDb
                              where u.Email == email
                              select u).FirstOrDefault();
                return (userDb != null);
            }
        }

        public bool UserNameExistsInProfile(string userName)
        {
            Log.Information($"In USerProfileService.EmailExistsInProfile(): Request recieved with userName {userName}");
            using (db)
            {
                var userDb = (from u in db.UserDb
                              where u.Username.ToLower() == userName.ToLower()
                              select u).FirstOrDefault();
                return (userDb != null);
            }
        }

        public IEnumerable<User> GetAllUsers()
        {
            Log.Information($"In USerProfileService.GetAllUsers(): Request recieved");
            using (db)
            {
                var users = new List<User>();
                var userDbs = db.UserDb.ToList();

                foreach (var userDb in userDbs)
                {
                    var user = _mapper.MapUserDbToUser(userDb);

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
                var userDb = (from u in db.UserDb
                              where u.Username.ToLower() == userName.ToLower()
                              select u).FirstOrDefault();
                if (userDb == null)
                {
                    userDb = (from u in db.UserDb
                              where u.Email == userName
                              select u).FirstOrDefault();
                    if (userDb == null)
                    {
                        Log.Warning($"In USerProfileService.GetUserByUserName(): No user found with userName or email {userName}");
                    }
                }
                return _mapper.MapUserDbToUser(userDb);
            }
        }

        public User GetUserByUserId(int userId)
        {
            Log.Information($"In USerProfileService.GetUserByUserId(): Request recieved with userId {userId}");
            using (db)
            {
                var userDb = (from u in db.UserDb
                              where u.UserId == userId
                              select u).FirstOrDefault();
                if (userDb == null)
                {
                    Log.Warning($"In USerProfileService.GetUserByUserId(): No user found with userId {userId}");
                }

                return _mapper.MapUserDbToUser(userDb);
            }
        }

        //----------------------------------------------------------------------------------------
        // Log handling functions
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
        // Monitoring functions
        //----------------------------------------------------------------------------------------
        public bool IsAlive()
        {
            return true;
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
                // A database exception deserves a higher error level than warning. Let's do error level: Error
                Log.Error($"In USerProfileService.UpdateDatabase(): Unable to apply changes in database. Exception of type {ex.GetType().Name} was thrown. Exception: {JsonConvert.SerializeObject(ex)}");
                return false;
            }
        }
        #endregion
    }
}

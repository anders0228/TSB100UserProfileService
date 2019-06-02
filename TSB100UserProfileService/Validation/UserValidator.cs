using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TSB100UserProfileService.DataTransferObjects;
using TSB100UserProfileService.Validation;
using Serilog;

namespace TSB100UserProfileService
{
    internal class UserValidator
    {
        internal bool ValidateNewUser(NewUser newUser)
        {
            var errors = new List<string>();

            // Step 1: Validate indata format
            StringValidator.NotNull(errors, newUser.Email, "Email");
            StringValidator.Email(errors, newUser.Email);

            StringValidator.NotNull(errors, newUser.FirstName, "FirstName");
            StringValidator.MinMax(errors, newUser.FirstName, 0, 100, "FirstName");

            StringValidator.NotNull(errors, newUser.Surname, "Surname");
            StringValidator.MinMax(errors, newUser.Surname, 0, 100, "Surname");

            StringValidator.NotNull(errors, newUser.Username, "Username");
            StringValidator.MinMax(errors, newUser.Username, 0, 50, "Username");

            // Note: Password is not validated since it isn't saved in the UserProfile Database 
            //       Also we don't know format or min/max length for it

            if (errors.Count > 0)
            {
                Log.Debug(JsonConvert.SerializeObject(errors));
                return false;
            }


            // Step 2: Validate that username or email is not already taken
            using (var loginService = new LoginServiceRef.LoginServiceClient())
            {
                if (loginService.UsernameExist(newUser.Username))
                {
                    errors.Add($"Validation error: Username '{newUser.Username}' is already taken.");
                }
                if (loginService.EmailExist(newUser.Email))
                {
                    errors.Add($"Validation error: Email '{newUser.Email}' is already taken.");
                }
            }
            if (errors.Count > 0)
            {
                Log.Debug(JsonConvert.SerializeObject(errors));
                return false;
            }

            return true;
        }

        internal bool ValidateUser(User user)
        {
            var errors = new List<string>();
            // Step 1: Validate indata format
            if (user.Id < 1)
            {
                errors.Add($"Validation error: Invalid user ID: {user.Id}");
            }

            StringValidator.NotNull(errors, user.Email, "Email");
            StringValidator.Email(errors, user.Email);

            StringValidator.NotNull(errors, user.Name, "Name");
            StringValidator.MinMax(errors, user.Name, 0, 100, "Name");

            StringValidator.NotNull(errors, user.Surname, "Surname");
            StringValidator.MinMax(errors, user.Surname, 0, 100, "Surname");

            StringValidator.NotNull(errors, user.Username, "Username");
            StringValidator.MinMax(errors, user.Username, 0, 50, "Username");

            StringValidator.MinMax(errors, user.Address, 0, 50, "Address");
            StringValidator.MinMax(errors, user.City, 0, 50, "City");
            StringValidator.MinMax(errors, user.Phonenumber, 0, 50, "Phonenumber");
            StringValidator.MinMax(errors, user.Picture, 0, 100, "Picture");
            // TODO: Create validating functions for ZipCode and PersonalCodeNumber

            // Step 2: Check services and databases

            // TODO:  Validate that UserId exists
            // TODO:  Validate that username (if new) is not already taken
            // TODO:  Validate that email (if new) is not already taken
            // This is presently (190602) done by the web site, but not here...

            if (errors.Count > 0)
            {
                Log.Debug(JsonConvert.SerializeObject(errors));
                return false;
            }

            return true;
        }
    }
}
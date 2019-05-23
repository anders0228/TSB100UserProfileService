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

            // TODO: Check login service if username is free or already taken - CHRISTOFFER

            //if (_loginAdapter.UsernameExists(newUser.Username))
            //{
            // TODO: WHEN UsernameExists() IS FINISHED - Log if username already exists instead of adding to the error list
            //    errors.Add($"Validation error: {newUser.Username} already exists.");
            //}

            StringValidator.MinMax(errors, newUser.Email, 0, 50);
            StringValidator.MinMax(errors, newUser.FirstName, 0, 100);
            StringValidator.MinMax(errors, newUser.Surname, 0, 100);
            StringValidator.MinMax(errors, newUser.Username, 0, 50);
            StringValidator.Email(errors, newUser.Email);
            //Password is not validated since it isn't saved in the UserProfile Database -> we don't know min or max length for it

            if (errors.Count > 0)
            {
                // Saves the errors as a long string in json format
                Log.Debug(JsonConvert.SerializeObject(errors));
            }
            return true;
        }

        internal bool ValidateUser(User user)
        {
            var errors = new List<string>();

            StringValidator.MinMax(errors, user.Email, 0, 50);
            StringValidator.MinMax(errors, user.Name, 0, 100);
            StringValidator.MinMax(errors, user.Surname, 0, 100);
            StringValidator.MinMax(errors, user.Picture, 0, 100);
            StringValidator.MinMax(errors, user.Username, 0, 50);
            StringValidator.MinMax(errors, user.Address, 0, 50);
            StringValidator.MinMax(errors, user.City, 0, 50);
            StringValidator.MinMax(errors, user.Phonenumber, 0, 50);
            StringValidator.Email(errors, user.Email);
            // TODO: Create a validating function for int (needed for: ZipCode and PersonalCodeNumber)

            if (errors.Count > 0)
            {
                // Saves the errors as a long string in json format
                Log.Debug(JsonConvert.SerializeObject(errors));
            }
            return true;
        }
    }
}
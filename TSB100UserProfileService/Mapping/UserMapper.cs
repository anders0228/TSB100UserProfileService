using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TSB100UserProfileService.DataTransferObjects;

namespace TSB100UserProfileService.Mapping
{
    internal class UserMapper
    {
        public void MapUserToModel(User user, UserDb userDb)
        {
            if (user == null)
            {
                Log.Warning($"In UserMapper.MapUserToModel(): Unexpected null input. user==null");
                return;
            }
            if (userDb == null)
            {
                Log.Warning($"In UserMapper.MapUserToModel(): Unexpected null input. userDb==null");
                return;
            }
            userDb.CreatedDate = DateTime.Now;
            userDb.Username = user.Username;
            userDb.UserId = user.Id;
            userDb.Address = user.Address;
            userDb.City = user.City;
            userDb.Email = user.Email;
            userDb.FirstName = user.Name;
            userDb.Surname = user.Surname;
            userDb.PersonalIdentityNumber = user.PersonalCodeNumber;
            userDb.PhoneNumber = user.Phonenumber;
            userDb.PictureUrl = user.Picture;
            userDb.ZipCode = user.ZipCode;
        }

        public void MapNewUserToModel(NewUser newUser, UserDb userDb)
        {
            if (newUser == null)
            {
                Log.Warning($"In UserMapper.MapNewUserToModel(): Unexpected null input. newUser==null");
                return;
            }
            if (userDb == null)
            {
                Log.Warning($"In UserMapper.MapNewUserToModel(): Unexpected null input. userDb==null");
                return;
            }
            userDb.CreatedDate = DateTime.Now;
            userDb.Username = newUser.Username;
            userDb.Email = newUser.Email;
            userDb.FirstName = newUser.FirstName;
            userDb.Surname = newUser.Surname;
        }

        public User MapToWebService(UserDb userDb)
        {
            if (userDb == null)
            {
                Log.Warning($"In UserMapper.MapToWebService(): Unexpected null input. userDb==null");
                return null;
            }
            var user = new User
            {
                Username = userDb.Username,
                Id = userDb.UserId,
                Address = userDb.Address,
                City = userDb.City,
                Email = userDb.Email,
                Name = userDb.FirstName,
                Surname = userDb.Surname,
                PersonalCodeNumber = userDb.PersonalIdentityNumber,
                Phonenumber = userDb.PhoneNumber,
                Picture = userDb.PictureUrl,
                ZipCode = userDb.ZipCode,
            };
            return user;
        }
    }
}


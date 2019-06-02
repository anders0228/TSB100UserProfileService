using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TSB100UserProfileService.DataTransferObjects;
using TSB100UserProfileService.LoginServiceRef;

namespace TSB100UserProfileService.Mapping
{
    internal class UserMapper
    {
        // We do have a lot of user related objects!!
        // Here is a list of them: 
        //   TSB100UserProfileService.UserDb
        //   TSB100UserProfileService.DataTransferObjects.NewUser
        //   TSB100UserProfileService.DataTransferObjects.User
        //   TSB100UserProfileService.LoginServiceRef.NewUser
        //   TSB100UserProfileService.LoginServiceRef.ReturnUser
        //   TSB100UserProfileService.LoginServiceRef.InterfaceUser

        public void MapUserToUserDb(User user, UserDb userDb)
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

        public void MapNewUserToUserDb(DataTransferObjects.NewUser newUser, UserDb userDb)
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

        internal LoginServiceRef.NewUser MapDTONewUserToLoginServiceNewUser(DataTransferObjects.NewUser newUserFromWeb)
        {
            return new LoginServiceRef.NewUser
            {
                Email = newUserFromWeb.Email,
                Firstname = newUserFromWeb.FirstName,
                Surname = newUserFromWeb.Surname,
                Password = newUserFromWeb.Password,
                Username = newUserFromWeb.Username
            };
        }

        public User MapUserDbToUser(UserDb userDb)
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

        internal ReturnUser MapInterfaceUserToReturnUser(InterfaceUser interfaceUser)
        {
            return new ReturnUser()
            {
                Email = interfaceUser.Email,
                Firstname = interfaceUser.Firstname,
                ID = interfaceUser.ID,
                Surname = interfaceUser.Surname,
                Username = interfaceUser.Username,
            };
        }

        internal ReturnUser MapUserToReturnUser(User user)
        {
            return new ReturnUser
            {
                ID = user.Id,
                Username = user.Username,
                Email = user.Email,
                Firstname = user.Name,
                Surname = user.Surname,
            };
        }
    }
}


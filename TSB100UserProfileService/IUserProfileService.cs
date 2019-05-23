using System.Collections.Generic;
using System.ServiceModel;
using TSB100UserProfileService.DataTransferObjects;

namespace TSB100UserProfileService
{
    [ServiceContract]
    public interface IUserProfileService
    {
        //----------------------------------------------------------------------------------------
        // Create & Update
        //----------------------------------------------------------------------------------------
        [OperationContract]
        User CreateUser(NewUser newUser);

        [OperationContract]
        bool UpdateUser(User user);

        //----------------------------------------------------------------------------------------
        // Delete
        //----------------------------------------------------------------------------------------
        /// <summary>
        /// Deletes a userProfile from database (but not from the login service).
        /// </summary>
        [OperationContract]
        bool DeleteUserProfile(int userId);

        /// <summary>
        /// Deletes a userProfile from database AND from the login service.
        /// </summary>
        [OperationContract]
        bool DeleteUser(int userId);

        //----------------------------------------------------------------------------------------
        // Get
        //----------------------------------------------------------------------------------------
        [OperationContract]
        bool UserIdExistsInProfile(int userId);

        [OperationContract]
        bool EmailExistsInProfile(string email);

        [OperationContract]
        bool UserNameExistsInProfile(string userName);

        [OperationContract]
        IEnumerable<User> GetAllUsers();

        [OperationContract]
        User GetUserByUserNameOrEmail(string username);

        [OperationContract]
        User GetUserByUserId(int userId);

        //----------------------------------------------------------------------------------------
        // Log functions
        //----------------------------------------------------------------------------------------
        [OperationContract]
        string GetLatestLog();
       
    }
}

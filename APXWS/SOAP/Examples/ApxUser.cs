
namespace Advent.ApxSoap
{
    using Advent.ApxSoap.ApxWS;
    using System;

    class UserExamples
    {
        /// <summary>
        /// This example shows how to create a new user
        /// </summary>
        /// <param name="apxWS"></param>
        public static void CreateNewUser(ApxWS.ApxWS apxWS)
        {
            string random = string.Format("Test{0}", new Random().Next());

            UserQueryOptions queryOptions = new UserQueryOptions();
            UserQueryResult queryResult;

            Status status = apxWS.User_GetAll(ref queryOptions, out queryResult);

            UserPutOptions putOps = new UserPutOptions();
            UserPutResult putRlt;
            User user = new User();
            user._DBAction = DBAction.Insert;

            user._UpdatedFields = new UserUpdatedFields();
            #region updated fields
            user._UpdatedFields.LastName = true;
            user.LastNameIsNull = false;
            user.LastName = random;

            user._UpdatedFields.LoginName = true;
            user.LoginNameIsNull = false;
            user.LoginName = "example\\abc";

            user._UpdatedFields.IsActive = true;
            user.IsActive = true;

            user._UpdatedFields.IsOSLogin = true;
            user.IsOSLogin = true;
            user.IsOSLoginIsNull = false;

            user._UpdatedFields.AccessAllUserGroups = true;
            user.AccessAllUserGroups = true;

            user._UpdatedFields.AccessAllUsersRoleID = true;
            user.AccessAllUsersRoleID = "Portfolio Manager";
            user.AccessAllUsersRoleIDIsNull = false;

            user._UpdatedFields.CanAccessAllUsersPrivateData = true;
            user.CanAccessAllUsersPrivateData = true;
            user.CanAccessAllUsersPrivateDataIsNull = false;

            user._UpdatedFields.DefaultRoleID = true;
            user.DefaultRoleID = "Portfolio Manager";

            user._UpdatedFields.PrivateDataRoleId = true;
            user.PrivateDataRoleId = "Portfolio Manager";
            user.PrivateDataRoleIdIsNull = false;
            #endregion
            status = apxWS.User_Put(ref putOps, user, out putRlt);

            user._UpdatedFields.EmailAddress = true;
            user.EmailAddressIsNull = false;
            user.EmailAddress = random + "@example.com";
            status = apxWS.User_Put(ref putOps, user, out putRlt);
        }

        /// <summary>
        /// This example shows how to update a user
        /// </summary>
        /// <param name="apxWS"></param>
        public static void UpdateUser(ApxWS.ApxWS apxWS)
        {
            UserQueryOptions qOptions = new UserQueryOptions();
            UserQueryResult qResult;
            Status qStatus = apxWS.User_GetByLoginName(ref qOptions, "abc", out qResult);

            if (qStatus.StatusCode == StatusCodes.Success && qResult.UserList != null && qResult.UserList.Length != 0)
            {
                User user = qResult.UserList[0];

                UserPutOptions pOptions = new UserPutOptions();
                user._DBAction = DBAction.Update;

                user._UpdatedFields = new UserUpdatedFields();

                user._UpdatedFields.EmailAddress = true;
                user.EmailAddressIsNull = false;
                user.EmailAddress = "abc@example.com";

                UserPutResult pResult;
                var status = apxWS.User_Put(ref pOptions, user, out pResult);
            }
        }

        /// <summary>
        /// This example shows how to delete a user
        /// </summary>
        /// <param name="apxWS"></param>
        public static void DeleteUser(ApxWS.ApxWS apxWS)
        {
            UserQueryOptions qOptions = new UserQueryOptions();
            UserQueryResult qResult;
            Status qStatus = apxWS.User_GetByLoginName(ref qOptions, "abc", out qResult);

            if (qStatus.StatusCode == StatusCodes.Success && qResult.UserList != null && qResult.UserList.Length != 0)
            {
                User user = qResult.UserList[0];

                UserPutOptions pOptions = new UserPutOptions();
                user._DBAction = DBAction.Delete;

                UserPutResult pResult;
                apxWS.User_Put(ref pOptions, user, out pResult);
            }
        }

        /// <summary>
        /// This example shows how to reset empty user emails
        /// </summary>
        /// <param name="apxWS"></param>
        public static void ResetEmptyUserEmails(ApxWS.ApxWS apxWS)
        {
            UserQueryOptions qOptions = new UserQueryOptions();
            UserQueryResult qResult = null;
            var qStatus = apxWS.User_GetAll(ref qOptions, out qResult);
            if (qStatus.Success)
            {
                foreach (User user in qResult?.UserList)
                {
                    if (user.EmailAddressIsNull)
                    {
                        user.EmailAddress = user.LoginName.Replace('\\', '.') + "@example.com";
                        user.EmailAddressIsNull = false;
                        user._UpdatedFields.EmailAddress = true;
                        user._DBAction = DBAction.Merge;
                        UserPutOptions pOptions = new UserPutOptions();
                        UserPutResult pResult = null;
                        var pStatus = apxWS.User_Put(ref pOptions, user, out pResult);
                        if (!pStatus.Success)
                        {
                            Console.WriteLine("Fail to update email for user {0}", user.LoginName);
                            Console.WriteLine("Exception: {0}", pStatus.ExceptionText);
                            foreach (StatusMessage msg in pStatus.EntityStatusDetails.MessageList)
                            {
                                Console.WriteLine("{0} - {1}", msg.FieldTag, msg.Message);
                            }
                        }
                        else
                        {
                            Console.WriteLine("An example email address {0} is set for User {1}");
                        }
                    }
                }
            }
        }
    }
}

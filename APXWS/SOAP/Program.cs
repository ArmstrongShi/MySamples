using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Advent.ApxSoap
{
    class Program
    {
        static void Main(string[] args)
        {
            AuthClient client = new AuthClient("http://vmapxba9.advent.com");
            ApxWS.ApxWS apxws = client.Login("api", "advs"); // DB User
            //apxws = client.Login(); // Win NT user

            ApxWS.UserQueryOptions queryOptions = new ApxWS.UserQueryOptions();
            ApxWS.UserQueryResult queryResult;
            ApxWS.Status status = apxws.User_GetAll(ref queryOptions, out queryResult);
        }

        private static void Sample_TokenBasedAuth()
        {
            ApxWS.ApxWS apxws = new ApxWS.ApxWS();
            apxws.AccessToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IkNBNkMxN0ZCMDFCRTg1RUU2QjJCMjAwNTE2ODZBRkYwN0M5REMwQTciLCJ0eXAiOiJKV1QiLCJ4NXQiOiJ5bXdYLXdHLWhlNXJLeUFGRm9hdjhIeWR3S2MifQ.eyJuYmYiOjE1NzQzMTY4NDUsImV4cCI6MTU3NDMyNDA0NSwiaXNzIjoiaHR0cDovL3ZtYXB4YmE5LmFkdmVudC5jb20vb2F1dGgiLCJhdWQiOlsiaHR0cDovL3ZtYXB4YmE5LmFkdmVudC5jb20vb2F1dGgvcmVzb3VyY2VzIiwiQVBYX0FQSSJdLCJjbGllbnRfaWQiOiJyby5hcHhhcGljbGllbnQiLCJyb2xlIjoiYXBpIiwic3ViIjoiZTAwMDE0NDgtZjc5Ny00NTYyLTk0NGUtNjBlNjZmNWQ3MDk3IiwiYXV0aF90aW1lIjoxNTc0MzE2ODQ1LCJpZHAiOiJsb2NhbCIsIm5hbWUiOiJhcGkiLCJlbWFpbCI6ImFwaUBhZHZlbnQuY29tIiwic2NvcGUiOlsiYXB4YXBpIiwib2ZmbGluZV9hY2Nlc3MiXSwiYW1yIjpbInB3ZCJdfQ.Po5uTfJZkA3RKQjG40O1URl1yprFgDY-FRZyops0keSKDxXuR_g-RJQwAY6opUn1R8kckStHx5Wvyvp9FGxK70hCxBhH-1UojM5lUsOwa-5AnJbwbHZe7Rj8a5CAdaoWYgBx3UetkyjDyvt4W6-uwtBQROQrVt2O9Gk6Bnvr-wkcXLaQflKNZtFLZfeRqB_9z8xrqt9T43rVRig8FRWZNbnR4MGlkBVX6Px_cVYPs5WstZNrgWmCsk8rjNSsq-fTedvmzoAaxf9SDZ5jM1yA4u0j5S1CsEiLzfe819v5zppco2-ZvQSXgwGAq4kJSkSMJe1eAVwDCniq870ODi1d7A";

            ApxWS.ContactQueryOptions queryOptions = new ApxWS.ContactQueryOptions();
            ApxWS.ContactQueryResult queryResult;

            ApxWS.Status status = apxws.Contact_GetAll(ref queryOptions, out queryResult);
        }

        private static void Sample_Contact(ApxWS.ApxWS apxWS)
        {
            ApxWS.ContactQueryOptions queryOptions = new ApxWS.ContactQueryOptions();
            ApxWS.ContactQueryResult queryResult;

            // You must already setup a contact with code "C1"
            ApxWS.Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "D0U0100012", out queryResult);

            ApxWS.Contact contact = queryResult.ContactList[0];
            contact._DBAction = ApxWS.DBAction.Update;
            contact._UpdatedFields = new ApxWS.ContactUpdatedFields();

            // To clear a string field, set it to an empty string.
            contact.LastNameIsNull = false;
            contact._UpdatedFields.LastName = true;
            contact.LastName = contact.LastName;


            ApxWS.ContactPutOptions putOps = new ApxWS.ContactPutOptions();
            ApxWS.ContactPutResult putRlt = new ApxWS.ContactPutResult();

            status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

            if (!status.Success)
            {
                Console.WriteLine(status.ExceptionText);
            }
        }
        /// <summary>
        /// This sample include 3 cases for Salentica
        /// Case 1: Change default address from Home to Business
        /// Case 2: Update Home address line 2 to a new value "abc"
        /// Case 3: Clear Business address line 3
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Sample_ContactAddress(ApxWS.ApxWS apxWS)
        {
            ApxWS.ContactQueryOptions queryOptions = new ApxWS.ContactQueryOptions();
            // To return addresses along with contact, you must set this field to true
            queryOptions.IncludeContactAddressList = true;
            ApxWS.ContactQueryResult queryResult;

            // Get a contact by contact code
            ApxWS.Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);
            if (status.Success)
            {
                ApxWS.Contact contact = queryResult.ContactList[0];

                contact._DBAction = ApxWS.DBAction.Update;
                contact._UpdatedFields = new ApxWS.ContactUpdatedFields();
                foreach (ApxWS.ContactAddress address in contact.ContactAddressList)
                {
                    #region Case 1: Change default address from Home to Business
                    // Assumption: default address was Home address
                    if (address.AddressLabel == "Home")
                    {
                        // this is to indicate that Home address is to be updated
                        address._DBAction = ApxWS.DBAction.Update;

                        // this is to indicate that IsDefaultAddress is not null
                        address.IsDefaultAddressIsNull = false;
                        // this is to indicate that IsDefaultAddress is to be updated
                        address._UpdatedFields.IsDefaultAddress = true;
                        // this is to set IsDefault Address to false
                        address.IsDefaultAddress = false;

                        #region Case 2: Update Home address line 2 to a new value "abc"
                        // this is to set new value to AddressLine2
                        address.AddressLine2 = "abc";
                        // this is to indicate that AddressLine2 is to be updated
                        address._UpdatedFields.AddressLine2 = true;
                        #endregion
                    }

                    //Assumption: new default address is Business address
                    if (address.AddressLabel == "Business")
                    {
                        // this is to indicate that Business address is to be updated
                        address._DBAction = ApxWS.DBAction.Update;

                        // this is to indicate that IsDefaultAddress is not null
                        address.IsDefaultAddressIsNull = false;
                        // this is to indicate that IsDefaultAddress is to be updated
                        address._UpdatedFields.IsDefaultAddress = true;
                        // this is to set IsDefault Address to true
                        address.IsDefaultAddress = true;

                        #region Case 3: Clear Business address line 3
                        // this is to set new value to AddressLine2
                        address.AddressLine3 = string.Empty;
                        // this is to indicate that AddressLine2 is to be updated
                        address._UpdatedFields.AddressLine3 = true;
                        #endregion
                    }
                    #endregion
                }

                ApxWS.ContactPutOptions putOps = new ApxWS.ContactPutOptions();
                ApxWS.ContactPutResult putRlt = new ApxWS.ContactPutResult();

                status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

                if (!status.Success)
                {
                    Console.WriteLine(status.ExceptionText);
                }
            }
        }

        /// <summary>
        /// This sample shows how to clear fields
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Sample_ClearContactFields(ApxWS.ApxWS apxWS)
        {
            ApxWS.ContactQueryOptions queryOptions = new ApxWS.ContactQueryOptions();
            ApxWS.ContactQueryResult queryResult;

            // You must already setup a contact with code "C1"
            ApxWS.Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);

            ApxWS.Contact contact = queryResult.ContactList[0];
            contact._DBAction = ApxWS.DBAction.Update;
            contact._UpdatedFields = new ApxWS.ContactUpdatedFields();

            // To clear a string field, set it to an empty string.
            contact.LastNameIsNull = false;
            contact._UpdatedFields.LastName = true;
            contact.LastName = ""; // Or contact.LastName = String.Empty

            // To clear a date field, set it to the specific date "1/1/1753".
            contact.BirthdateIsNull = false;
            contact._UpdatedFields.Birthdate = true;
            contact.Birthdate = "1/1/1753 08:00:00Z"; // You must use UTC time here.

            // To clear a numeric field, set it to an invalid value.
            // For example, CallInterval must be a positive number between 0 and 32767, then -1 is invalid.
            contact.CallIntervalIsNull = false;
            contact._UpdatedFields.CallInterval = true;
            contact.CallInterval = -1;

            // To clear a numeric field that allows both positive and nagative values, set it to 0 will clear this field.
            contact.TaxBracketIsNull = false;
            contact._UpdatedFields.TaxBracket = true;
            contact.TaxBracket = 0;

            foreach (ApxWS.ApiCustomField field in contact.ContactCustomFields)
            {
                // You must already setup and activate a custom field in Date type
                if (string.Equals(field.Tag, "DateField", StringComparison.CurrentCultureIgnoreCase))
                {
                    // To clear a custom date field, set it to the specific date "1/1/1753".
                    field.UpdatedField = true;
                    field.ValueIsNull = false;
                    field.Value = "1/1/1753 08:00:00z"; // You must use UTC time here.
                }

                // You must already setup and activate a custom field in Text type
                if (string.Equals(field.Tag, "TextField", StringComparison.CurrentCultureIgnoreCase))
                {
                    // To clear a custom string field, set it to an empty string.
                    field.UpdatedField = true;
                    field.ValueIsNull = false;
                    field.Value = ""; // // Or field.Value = String.Empty
                }

                // You must already setup and activate a custom field in Numeric type
                if (string.Equals(field.Tag, "numField", StringComparison.CurrentCultureIgnoreCase))
                {
                    Console.WriteLine("numField: {0}", field.Value);
                    // To clear a custom numeric field, set it to -1.0E300.
                    field.UpdatedField = true;
                    field.ValueIsNull = false;
                    field.Value = "-1.0E300"; // clear Numeric field
                }
            }

            ApxWS.ContactPutOptions putOps = new ApxWS.ContactPutOptions();
            ApxWS.ContactPutResult putRlt = new ApxWS.ContactPutResult();

            status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

            if (!status.Success)
            {
                Console.WriteLine(status.ExceptionText);
            }
        }

        /// <summary>
        /// This sample shows how to add a new interested party to contact
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Sample_AddNewInterestedParty(ApxWS.ApxWS apxWS)
        {
            ApxWS.ContactQueryOptions queryOptions = new ApxWS.ContactQueryOptions();
            queryOptions.IncludeInterestedPartyList = true;

            ApxWS.ContactQueryResult queryResult;
            // You must already setup a contact with code "C1"
            ApxWS.Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);
            ApxWS.Contact contact = queryResult.ContactList[0];

            ApxWS.ContactPutOptions putOptions = new ApxWS.ContactPutOptions();
            ApxWS.ContactPutResult putResult;
            contact._DBAction = ApxWS.DBAction.Update;
            contact._UpdatedFields = new ApxWS.ContactUpdatedFields();

            // Create a list of InterestedParty and copy all existing interested parities to the list.
            ApxWS.InterestedParty[] ipList = new ApxWS.InterestedParty[contact.InterestedPartyList.Length + 1];
            for (int i = 0; i < contact.InterestedPartyList.Length; i++)
            {
                ipList[i] = contact.InterestedPartyList[i];
            }

            // Create a new interested party.
            ApxWS.InterestedParty ip = new ApxWS.InterestedParty();
            ip._DBAction = ApxWS.DBAction.Merge;
            // You must already setup the portfolio with code "P1"
            ip.PortfolioCode = "P1";
            // Contact code must be same as what you used above.
            ip.ContactCode = "C1";

            // Add the new interested party to the end of list.
            ipList[ipList.Length - 1] = ip;

            contact._IncludesInterestedPartyList = true;
            contact.InterestedPartyList = ipList;

            status = apxWS.Contact_Put(ref putOptions, contact, out putResult);
        }

        /// <summary>
        /// This sample shows how to create a new user
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Sample_CreateNewUser(ApxWS.ApxWS apxWS)
        {
            string random = string.Format("Test{0}", new Random().Next());

            ApxWS.UserQueryOptions queryOptions = new ApxWS.UserQueryOptions();
            ApxWS.UserQueryResult queryResult;

            ApxWS.Status status = apxWS.User_GetAll(ref queryOptions, out queryResult);

            ApxWS.UserPutOptions putOps = new ApxWS.UserPutOptions();
            ApxWS.UserPutResult putRlt;
            ApxWS.User user = new ApxWS.User();
            user._DBAction = ApxWS.DBAction.Insert;

            user._UpdatedFields = new ApxWS.UserUpdatedFields();
            #region updated fields
            user._UpdatedFields.LastName = true;
            user.LastNameIsNull = false;
            user.LastName = random;

            user._UpdatedFields.LoginName = true;
            user.LoginNameIsNull = false;
            user.LoginName = "advent\\bxli";

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
            user.EmailAddress = random + "@advent.com";
            status = apxWS.User_Put(ref putOps, user, out putRlt);
        }

        private static void Sample_UpdateUser(ApxWS.ApxWS apxWS)
        {
            ApxWS.UserQueryOptions qOptions = new ApxWS.UserQueryOptions();
            ApxWS.UserQueryResult qResult;
            ApxWS.Status qStatus= apxWS.User_GetByLoginName(ref qOptions, "pm", out qResult);

            if (qStatus.StatusCode == ApxWS.StatusCodes.Success && qResult.UserList != null && qResult.UserList.Length != 0)
            {
                ApxWS.User user = qResult.UserList[0];

                ApxWS.UserPutOptions pOptions = new ApxWS.UserPutOptions();
                user._DBAction = ApxWS.DBAction.Update;

                user._UpdatedFields = new ApxWS.UserUpdatedFields();

                user._UpdatedFields.EmailAddress = true;
                user.EmailAddressIsNull = false;
                user.EmailAddress = string.Empty;

                ApxWS.UserPutResult pResult;
                var status = apxWS.User_Put(ref pOptions, user, out pResult);

                user.EmailAddress = "new@advent.com";
                status = apxWS.User_Put(ref pOptions, user, out pResult);
            }
        }

        private static void Sample_DeleteUser(ApxWS.ApxWS apxWS)
        {
            ApxWS.UserQueryOptions qOptions = new ApxWS.UserQueryOptions();
            ApxWS.UserQueryResult qResult;
            ApxWS.Status qStatus = apxWS.User_GetByLoginName(ref qOptions, "test", out qResult);

            if (qStatus.StatusCode == ApxWS.StatusCodes.Success && qResult.UserList != null && qResult.UserList.Length != 0)
            {
                ApxWS.User user = qResult.UserList[0];

                ApxWS.UserPutOptions pOptions = new ApxWS.UserPutOptions();
                user._DBAction = ApxWS.DBAction.Delete;

                ApxWS.UserPutResult pResult;
                apxWS.User_Put(ref pOptions, user, out pResult);
            }
        }

        /// <summary>
        /// This sample shows how to create a new activity
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Sample_CreateNewActivity(ApxWS.ApxWS apxWS)
        {
            ApxWS.Activity email = new ApxWS.Activity();
            email._DBAction = ApxWS.DBAction.Insert;
            email._UpdatedFields = new ApxWS.ActivityUpdatedFields();

            email.ActivityTypeID = "Email";
            email._UpdatedFields.ActivityTypeID = true;

            email.OwnerCode = "api";
            email.OwnerCodeIsNull = false;
            email._UpdatedFields.OwnerCode = true;

            email.EmailSenderAddress = "from@test.com";
            email.EmailSenderAddressIsNull = false;
            email._UpdatedFields.EmailSenderAddress = true;

            email.EmailRecipientListTo = "to@test.com;to2@test.com";
            email.EmailRecipientListToIsNull = false;
            email._UpdatedFields.EmailRecipientListTo = true;

            email.EmailRecipientListCC = "cc@test.com;cc2@test.com";
            email.EmailRecipientListCCIsNull = false;
            email._UpdatedFields.EmailRecipientListCC = true;

            email.EmailRecipientListBCC = "bcc@test.com;bcc2@test.com";
            email.EmailRecipientListBCCIsNull = false;
            email._UpdatedFields.EmailRecipientListBCC = true;

            email.EmailSubject = "subject";
            email.EmailSubjectIsNull = false;
            email._UpdatedFields.EmailSubject = true;

            email.EmailBodyText = "body text";
            email.EmailBodyTextIsNull = false;
            email._UpdatedFields.EmailBodyText = true;

            email.EmailSentTime = "2015-12-10 18:00:00Z";
            email.EmailSentTimeIsNull = false;
            email._UpdatedFields.EmailSentTime = true;

            email.EmailReceivedTime = "2015-12-10 18:01:00Z";
            email.EmailReceivedTimeIsNull = false;
            email._UpdatedFields.EmailReceivedTime = true;

            ApxWS.ActivityPutOptions ops = new ApxWS.ActivityPutOptions();
            ApxWS.ActivityPutResult pResult;
            ApxWS.Status status = apxWS.Activity_Put(ref ops, email, out pResult);
        }
    }
}

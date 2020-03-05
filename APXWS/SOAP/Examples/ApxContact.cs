
namespace Advent.ApxSoap.Examples
{
    using Advent.ApxSoap.ApxWS;
    using System;

    class ContactExamples
    {
        /// <summary>
        /// This example shows how to find and update a contact
        /// </summary>
        /// <param name="apxWS"></param>
        private static void Contact(ApxWS apxWS)
        {
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            ContactQueryResult queryResult;

            // You must already setup a contact with code "C1"
            Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "D0U0100012", out queryResult);

            Contact contact = queryResult.ContactList[0];
            contact._DBAction = DBAction.Update;
            contact._UpdatedFields = new ContactUpdatedFields();

            // To clear a string field, set it to an empty string.
            contact.LastNameIsNull = false;
            contact._UpdatedFields.LastName = true;
            contact.LastName = contact.LastName;

            ContactPutOptions putOps = new ContactPutOptions();
            ContactPutResult putRlt = new ContactPutResult();

            status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

            if (!status.Success)
            {
                Console.WriteLine(status.ExceptionText);
            }
        }

        /// <summary>
        /// This example include 3 cases
        /// Case 1: Change default address from Home to Business
        /// Case 2: Update Home address line 2 to a new value "abc"
        /// Case 3: Clear Business address line 3
        /// </summary>
        /// <param name="apxWS"></param>
        private static void ContactAddress(ApxWS apxWS)
        {
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            // To return addresses along with contact, you must set this field to true
            queryOptions.IncludeContactAddressList = true;
            ContactQueryResult queryResult;

            // Get a contact by contact code
            Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);
            if (status.Success)
            {
                Contact contact = queryResult.ContactList[0];

                contact._DBAction = DBAction.Update;
                contact._UpdatedFields = new ContactUpdatedFields();
                foreach (ContactAddress address in contact.ContactAddressList)
                {
                    #region Case 1: Change default address from Home to Business
                    // Assumption: default address was Home address
                    if (address.AddressLabel == "Home")
                    {
                        // this is to indicate that Home address is to be updated
                        address._DBAction = DBAction.Update;

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
                        address._DBAction = DBAction.Update;

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

                ContactPutOptions putOps = new ContactPutOptions();
                ContactPutResult putRlt = new ContactPutResult();

                status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

                if (!status.Success)
                {
                    Console.WriteLine(status.ExceptionText);
                }
            }
        }

        /// <summary>
        /// This example shows how to clear fields
        /// </summary>
        /// <param name="apxWS"></param>
        private static void ClearContactFields(ApxWS apxWS)
        {
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            ContactQueryResult queryResult;

            // You must already setup a contact with code "C1"
            Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);

            Contact contact = queryResult.ContactList[0];
            contact._DBAction = DBAction.Update;
            contact._UpdatedFields = new ContactUpdatedFields();

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

            foreach (ApiCustomField field in contact.ContactCustomFields)
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
                    field.Value = "";  // Or field.Value = String.Empty
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

            ContactPutOptions putOps = new ContactPutOptions();
            ContactPutResult putRlt = new ContactPutResult();

            status = apxWS.Contact_Put(ref putOps, contact, out putRlt);

            if (!status.Success)
            {
                Console.WriteLine(status.ExceptionText);
            }
        }

        /// <summary>
        /// This example shows how to add a new interested party to contact
        /// </summary>
        /// <param name="apxWS"></param>
        private static void AddNewInterestedParty(ApxWS apxWS)
        {
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            queryOptions.IncludeInterestedPartyList = true;

            ContactQueryResult queryResult;
            // You must already setup a contact with code "C1"
            Status status = apxWS.Contact_GetByContactCode(ref queryOptions, "C1", out queryResult);
            Contact contact = queryResult.ContactList[0];

            ContactPutOptions putOptions = new ContactPutOptions();
            ContactPutResult putResult;
            contact._DBAction = DBAction.Update;
            contact._UpdatedFields = new ContactUpdatedFields();

            // Create a list of InterestedParty and copy all existing interested parities to the list.
            InterestedParty[] ipList = new InterestedParty[contact.InterestedPartyList.Length + 1];
            for (int i = 0; i < contact.InterestedPartyList.Length; i++)
            {
                ipList[i] = contact.InterestedPartyList[i];
            }

            // Create a new interested party.
            InterestedParty ip = new InterestedParty();
            ip._DBAction = DBAction.Merge;
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
        /// This example shows how to share an address from an owner contact
        /// </summary>
        /// <param name="apxWS"></param>
        public void ShareAddress(ApxWS apxWS)
        {
            // query address owner and its address to share
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            queryOptions.IncludeContactAddressList = true;
            ContactQueryResult queryResult = null;
            var qStatus = apxWS.Contact_GetByContactCode(ref queryOptions, "owner", out queryResult);
            Contact owner = queryResult.ContactList[0];
            // owner's address
            ContactAddress address = owner.ContactAddressList[0];

            ContactPutOptions putOptions = new ContactPutOptions();
            ContactPutResult putResult = null;

            // the contact that shares owner's address
            Contact contact = new Contact();
            contact.ContactCode = "Sharing";
            contact._DBAction = DBAction.Update;

            address.ContactCode = contact.ContactCode;
            address._UpdatedFields.ContactCode = true;
            address.AddressLabel = "Business";
            address._UpdatedFields.AddressLabel = true;
            address._UpdatedFields.AddressGUID = true;
            address._DBAction = DBAction.Insert;

            contact.ContactAddressList = new ContactAddress[] { address };
            contact._IncludesContactAddressList = true;

            var pStatus = apxWS.Contact_Put(ref putOptions, contact, out putResult);
        }
    }
}

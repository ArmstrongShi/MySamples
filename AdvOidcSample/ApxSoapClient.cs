

namespace AdvOidcSample
{
    using ApxSoap;
    using System;

    public class ApxSoapClient
    {
        private ApxWS apxws;

        public ApxSoapClient(string baseAddress, string accessToken)
        {
            this.apxws = new ApxWS();
            this.apxws.AccessToken = accessToken;

            Uri serviceUri = null;
            if (Uri.TryCreate(new Uri(baseAddress), "apxlogin/services/V2/ApxWS.asmx", out serviceUri))
            {
                this.apxws.Url = serviceUri.AbsoluteUri;
            }
        }

        public void GetContacts()
        {
            Console.WriteLine("===========Start SOAP Request===========");
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            ContactQueryResult queryResult = null;
            var status = this.apxws.Contact_GetAll(ref queryOptions, out queryResult);
            
            if (queryResult.ContactList==null || queryResult.ContactList.Length==0)
            {
                Console.WriteLine("No Contacts");
            }
            foreach (Contact contact in queryResult.ContactList)
            {
                Console.WriteLine("ContactCode={0}", contact.ContactCode);
            }
            Console.WriteLine("===========End SOAP Request===========");
        }

        public void ShareAddress()
        {
            // query address owner and its address to share
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            queryOptions.IncludeContactAddressList = true;
            ContactQueryResult queryResult = null;
            var qStatus = this.apxws.Contact_GetByContactCode(ref queryOptions, "owner", out queryResult);
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
            
            var pStatus = this.apxws.Contact_Put(ref putOptions, contact, out putResult);
        }

        public void SetExampleUserEmail()
        {
            UserQueryOptions qOptions = new UserQueryOptions();
            UserQueryResult qResult = null;
            var qStatus = this.apxws.User_GetAll(ref qOptions, out qResult);
            if (qStatus.Success)
            {
                foreach (User user in qResult?.UserList)
                {
                    if (user.EmailAddressIsNull)
                    {
                        string exampleEmail = user.LoginName.Replace('\\', '.') + "@example.com";
                        user.EmailAddress = exampleEmail;
                        user.EmailAddressIsNull = false;
                        user._UpdatedFields.EmailAddress = true;
                        user._DBAction = DBAction.Merge;
                        UserPutOptions pOptions = new UserPutOptions();
                        UserPutResult pResult = null;
                        var pStatus = this.apxws.User_Put(ref pOptions, user, out pResult);
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

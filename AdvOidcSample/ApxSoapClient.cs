

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

        public ApxSoapClient(string baseAddress)
        {
            this.apxws = new ApxWS();
            this.apxws.AccessToken = null;
            this.apxws.UseDefaultCredentials = true;
            Uri serviceUri = null;
            if (Uri.TryCreate(new Uri(baseAddress), "apx/services/V2/ApxWS.asmx", out serviceUri))
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
    }
}

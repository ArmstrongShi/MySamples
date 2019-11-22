

namespace AdvOidcSample
{
    using ApxSoap;
    using System;

    public class ApxSoapClient
    {
        ApxWS client = new ApxWS();

        public ApxSoapClient(string baseAddress, string accessToken)
        {
            this.client.AccessToken = accessToken;
            Uri serviceUri = null;
            if (Uri.TryCreate(new Uri(baseAddress), "apxlogin/services/V2/ApxWS.asmx", out serviceUri))
            {
                this.client.Url = serviceUri.AbsoluteUri;
            }
        }

        public void GetContacts()
        {
            ContactQueryOptions queryOptions = new ContactQueryOptions();
            ContactQueryResult queryResult = null;
            var status = this.client.Contact_GetAll(ref queryOptions, out queryResult);
            Console.WriteLine("===========Start Printing Contacts===========");
            if (queryResult.ContactList==null || queryResult.ContactList.Length==0)
            {
                Console.WriteLine("No Contacts");
            }
            foreach (Contact contact in queryResult.ContactList)
            {
                Console.WriteLine("ContactCode={0}", contact.ContactCode);
            }
            Console.WriteLine("===========End Printing Contacts===========");
        }
    }
}

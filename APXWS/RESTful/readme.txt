
ApxWS.cs - this class is to wrap HttpClient and your logics to handle http requests and responses.

AuthClient.cs - this class is to wrap 
	- GetApxAuthenticationConfiguration : This method is to get a URL of toke issuer (aka Identity Server URL), so that the code knows which Identity Server current APX is connecting to.
	- RequestToken : This class two methods to request token from Identity Server (1) password flow and (2) Windows Auth flow. If you want to use other flows, please refer to https://github.com/IdentityModel/ for more examples.
	- CreateApxWS: Create ApxWS instance and assign access token.
	- Login : this method is to wrap "RequestToken" and "CreateApxWS"
	- Logout : this method is to end APX user session
Program.cs
	- You need to initialize AuthClient instance, with a URL of APX Web Server, e.g. https://apx.company.com
	- Call Login method to get a token and initialize ApxWS instance.
	- Then you can add your logic to handle APX activities, contacts or users.
	- After your logic is done, call Logout method to end APX user session.
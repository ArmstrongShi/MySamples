
Authentication
	- ApxWS.cs : After you add ApxWS web reference, you must create this partial class to override GetWebRequest to add access token to http headers.
	- AuthClient.cs : 
		- GetApxAuthenticationConfiguration : This method is to get a URL of toke issuer (aka Identity Server URL), so that the code knows which Identity Server current APX is connecting to.
		- RequestToken : This class two methods to request token from Identity Server (1) password flow and (2) Windows Auth flow. If you want to use other flows, please refer to https://github.com/IdentityModel/ for more examples.
		- CreateApxWS: Create ApxWS instance and assign access token.
		- Login : this method is to wrap "RequestToken" and "CreateApxWS"
		- Logout : this method is to end APX user session
Examples
	- You can build your code as before and just pass the ApxWS instance as input.

Program.cs
	- You need to initialize AuthClient instance, with a URL of APX Web Server, e.g. https://apx.company.com
	- Call Login method to get a token and initialize ApxWS instance.
	- Then you can add your logic to handle APX activities, contacts or users.
	- After your logic is done, call Logout method to end APX user session.



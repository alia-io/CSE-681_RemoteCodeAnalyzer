///////////////////////////////////////////////////////////////////////////////////
///                                                                             ///
///  IAuthentication.cs - IAuthentication service contract,                     ///
///                 defines login, new user, and logout requests                ///
///                                                                             ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                 ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10      ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                ///
///                 Software Modeling and Analysis, 2021                        ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu            ///
///                                                                             ///
///////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module defines the IAuthentication service contract, which is responsible
 *   for handling user requests to login, create a new user, and logout. Requests
 *   sent from a client will be processed by the host server, and the response
 *   (if any) will be sent back to the client.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IAuthentication> authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
 *   IAuthentication authenticator = authenticationFactory.CreateChannel();
 *   bool login = authenticator.Login((AuthenticationRequest) credentials);
 *   bool newUser = authenticator.NewUser((AuthenticationRequest) credentials);
 *   authenticator.Logout((string) username);
 */

using System.ServiceModel;

namespace RCALibrary
{
    [ServiceContract]
    public interface IAuthentication
    {
        [OperationContract]
        bool Login(AuthenticationRequest credentials);

        [OperationContract]
        bool NewUser(AuthenticationRequest credentials);

        [OperationContract]
        void Logout(string username);
    }
}

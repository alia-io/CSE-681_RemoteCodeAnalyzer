/////////////////////////////////////////////////////////////////////////////////
///                                                                           ///
///  Authentication.cs - Fulfills IAuthentication operation contracts         ///
///                                                                           ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019               ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10    ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:              ///
///                 Software Modeling and Analysis, 2021                      ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu          ///
///                                                                           ///
/////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module fulfills the IAuthentication operation contracts to login, create a new user,
 *   and logout. It uses the secret.xml file to authenticate user logins and requests to add
 *   a new user. It maintains the list of active users through login and logout requests.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IAuthentication> authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
 *   IAuthentication authenticator = authenticationFactory.CreateChannel();
 *   bool login = authenticator.Login((AuthenticationRequest) credentials);
 *   bool newUser = authenticator.NewUser((AuthenticationRequest) credentials);
 *   authenticator.Logout((string) username);
 */

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.ServiceModel;
using System.Collections.Generic;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class Authentication : IAuthentication
    {
        // Saved list of logged in usernames
        private static readonly HashSet<string> loggedInUsers = new HashSet<string>();

        /* Verifies that the user exists and that the password provided equals the saved password */
        public bool Login(AuthenticationRequest credentials)
        {
            XDocument secret = LoadSecret();
            IEnumerable<string> password;

            Host.LogNewRequest("Login");

            if (secret == null) return false; // Loading secret file failed

            // Find the username in the secret file, and retrieve its saved password
            password = from XElement element in secret.Elements("userlist").Elements("user")
                       where element.Attribute("name").Value.Equals(credentials.Username)
                       select element.Attribute("password").Value;

            // Make sure one user was found with matching password, and that the user is not already logged in
            if (password.Count() == 1 && password.First().Equals(credentials.Password) && !loggedInUsers.Contains(credentials.Username))
            {
                loggedInUsers.Add(credentials.Username);
                return true;
            }

            return false;
        }

        /* Verifies that the user does not exist and that the provided passwords are equal, then creates the new user */
        public bool NewUser(AuthenticationRequest credentials)
        {
            XDocument secret = LoadSecret();
            IEnumerable<string> user;

            Host.LogNewRequest("New User");

            if (credentials.Username.Length < 1 || credentials.Password.Length < 1) return false; // Don't allow blank username or password
            if (secret == null) return false; // Loading secret file failed

            // Check if the username exists in the secret file
            user = from XElement element in secret.Elements("userlist").Elements("user")
                   where element.Attribute("name").Value.Equals(credentials.Username)
                   select element.Attribute("name").Value;

            if (user.Count() != 0) return false; // User already exists

            // Create the user directory
            try
            {
                Directory.CreateDirectory(".\\root\\" + credentials.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create new user directory.\n{0}", e.ToString());
                return false;
            }

            // Make sure provided passwords are equal
            if (credentials.Password.Equals(credentials.ConfirmPassword))
                return AddUser(secret, credentials); // Attempt to add the user

            // If add user failed, delete the user directory
            try
            {
                Directory.Delete(".\\root\\" + credentials.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to remove user directory.\n{0}", e.ToString());
            }

            return false;
        }

        /* Removes the username from the list of logged in users */
        public void Logout(string username)
        {
            Host.LogNewRequest("Logout");
            loggedInUsers.Remove(username);
        }

        /* Attempts to load the data from the secret file */
        private XDocument LoadSecret()
        {
            try
            {
                return XDocument.Load(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load secret file.\n{0}", e.ToString());
                return null;
            }
        }

        /* Attempts to add the new user by adding to the secret file, the metadata file, creating the user directory, and updating the navigators */
        private bool AddUser(XDocument secret, AuthenticationRequest credentials)
        {
            if (Host.AddNewUser(credentials.Username)) // Add user to metadata file
            {
                // Add user to secret file
                secret.Element("userlist").Add(new XElement("user",
                    new XAttribute("name", credentials.Username),
                    new XAttribute("password", credentials.Password)));

                try
                {
                    secret.Save(".\\root\\secret.xml");
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save secret file.\n{0}", e.ToString());
                    Host.RemoveUser(credentials.Username); // Remove user from metadata file
                }
            }
            return false;
        }
    }
}

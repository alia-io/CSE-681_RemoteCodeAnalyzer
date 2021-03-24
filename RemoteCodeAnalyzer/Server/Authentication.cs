using System;
using System.IO;
using System.Xml.Linq;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    class Authentication : IAuthentication
    {
        private static HashSet<string> loggedInUsers = new HashSet<string>();

        public bool Login(AuthenticationRequest credentials)
        {
            XDocument secret;
            IEnumerable<string> password;

            Console.WriteLine("Login Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            // Use to test what happens while server is fulfilling contract
            //Thread.Sleep(1000);

            try
            {
                secret = XDocument.Load(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load secret file.\n{0}", e.ToString());
                return false;
            }

            password = from XElement element in secret.Elements("userlist").Elements("user")
                       where element.Attribute("name").Value.Equals(credentials.Username)
                       select element.Attribute("password").Value;

            if (password.Count() == 1 && password.First().Equals(credentials.Password) && !loggedInUsers.Contains(credentials.Username))
            {
                loggedInUsers.Add(credentials.Username);
                return true;
            }

            return false;
        }

        public bool NewUser(AuthenticationRequest credentials)
        {
            XDocument secret;
            IEnumerable<string> user;

            Console.WriteLine("New User Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);
            
            // Use to test what happens while server is fulfilling contract
            //Thread.Sleep(1000);

            try
            {
                secret = XDocument.Load(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to load secret file.\n{0}", e.ToString());
                return false;
            }

            user = from XElement element in secret.Elements("userlist").Elements("user")
                   where element.Attribute("name").Value.Equals(credentials.Username)
                   select element.Attribute("name").Value;

            try
            {
                Directory.CreateDirectory(".\\root\\" + credentials.Username);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create new user directory.\n{0}", e.ToString());
                return false;
            }

            // TODO: add some requirements for username and password
            if (user.Count() == 0 && credentials.Password.Equals(credentials.ConfirmPassword))
                return AddUser(secret, credentials);

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

        public void Logout(string username) => loggedInUsers.Remove(username);

        private bool AddUser(XDocument secret, AuthenticationRequest credentials)
        {
            if (Host.AddNewUser(credentials.Username))
            {
                secret.Element("userlist").Add(new XElement("user",
                new XAttribute("name", credentials.Username),
                new XAttribute("password", credentials.Password)));

                secret.Save(".\\root\\secret.xml");

                return true;
            }

            return false;
        }
    }
}

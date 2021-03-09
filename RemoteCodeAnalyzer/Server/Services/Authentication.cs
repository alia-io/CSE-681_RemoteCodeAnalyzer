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
        public bool Login(AuthenticationRequest credentials)
        {
            int test = 3;
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

            if (password.Count() == 1 && password.First().Equals(credentials.Password))
            {
                // TODO: make sure this user isn't already logged in / connected
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

            while (Directory.Exists(".\\root\\" + credentials.Username))
            {
                try
                {
                    Directory.Delete(".\\root\\" + credentials.Username);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to remove user directory.\n{0}", e.ToString());
                }
            }

            return false;
        }

        private bool AddUser(XDocument secret, AuthenticationRequest credentials)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            XElement newRoot;

            lock (Host.DirectoryTreeLock)
            {
                Host.DirectoryTree.Element("root").Add(
                    new XElement("user",
                        new XAttribute("name", credentials.Username),
                        new XAttribute("date", date),
                        new XAttribute("projects", 0)));
                try
                {
                    Host.DirectoryTree.Save(".\\root\\metadata.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                    return false;
                }

                newRoot = new XElement(Host.DirectoryTree.Root);
            }

            lock (Host.NavigatorsLock) // Set the Root of all active Navigation instances
                foreach (Navigation navigator in Host.Navigators)
                    navigator.UpdateRoot(newRoot);

            secret.Element("userlist").Add(new XElement("user",
                new XAttribute("name", credentials.Username),
                new XAttribute("password", credentials.Password)));
            secret.Save(".\\root\\secret.xml");

            return true;
        }
    }
}

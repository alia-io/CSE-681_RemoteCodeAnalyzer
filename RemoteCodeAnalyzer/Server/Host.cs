using System;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net;
using System.Net.Sockets;
using System.IO;
using RCALibrary;

namespace Server
{
    class Host
    {
        public static readonly object DirectoryTreeLock = new object();
        public static readonly object NavigatorsLock = new object();
        public static XDocument DirectoryTree = null;
        public static List<Navigation> Navigators = new List<Navigation>();

        static void Main()
        {
            // Make sure all necessary directories and files exist
            CheckRoot();
            CheckSecret();
            CheckMetadata();

            Console.WriteLine("Initializing the Authentication service.");
            Uri authenticationAddress = new Uri("http://localhost:8000/Authentication/");
            ServiceHost authenticator = new ServiceHost(typeof(Authentication), authenticationAddress);

            // Use to test what happens while server is fulfilling contract
            //WSHttpBinding binding = new WSHttpBinding();
            //binding.SendTimeout = new TimeSpan(1, 0, 0);

            try
            {
                // Use to test what happens while server is fulfilling contract
                //navigator.AddServiceEndpoint(typeof(IAuthentication), binding, authenticationAddress);
                authenticator.AddServiceEndpoint(typeof(IAuthentication), new WSHttpBinding(SecurityMode.None), authenticationAddress);
                authenticator.Open();
                Console.WriteLine("The Authentication service is ready.");
                Console.WriteLine();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                authenticator.Abort();
            }

            Console.WriteLine("Initializing the Navigation service.");
            Uri navigationAddress = new Uri("http://localhost:8000/Navigation/");
            ServiceHost navigator = new ServiceHost(typeof(Navigation), navigationAddress);

            try
            {
                navigator.AddServiceEndpoint(typeof(INavigation), new WSHttpBinding(), navigationAddress);
                navigator.Open();
                Console.WriteLine("The Navigation service is ready.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start the Navigation Service: {0}", e.ToString());
                navigator.Abort();
            }

            Console.WriteLine("Initializing the Upload service.");
            Uri uploadAddress = new Uri("http://localhost:8000/Upload/");
            ServiceHost uploader = new ServiceHost(typeof(Upload), uploadAddress);

            try
            {
                uploader.AddServiceEndpoint(typeof(IUpload), new WSHttpBinding(SecurityMode.None), uploadAddress);
                uploader.Open();
                Console.WriteLine("The Upload service is ready.");
                Console.WriteLine();
            }
            catch (CommunicationException ce)
            {
                Console.WriteLine("An exception occurred: {0}", ce.Message);
                uploader.Abort();
            }

            Console.ReadKey();
            authenticator.Close();
            navigator.Close();
            uploader.Close();
        }

        private static void CheckRoot()
        {
            while (!Directory.Exists(".\\root"))
            {
                try
                {
                    Directory.CreateDirectory(".\\root");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to create root directory.\n{0}", e.ToString());
                }
            }
        }

        private static void CheckSecret()
        {
            if (!File.Exists(".\\root\\secret.xml"))
            {
                XDocument secret = new XDocument(new XElement("userlist"));
                while (!File.Exists(".\\root\\secret.xml"))
                {
                    try
                    {
                        secret.Save(".\\root\\secret.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to create secret file.\n{0}", e.ToString());
                    }
                }
            }
        }

        private static void CheckMetadata()
        {
            if (File.Exists(".\\root\\metadata.xml"))
            {
                while (DirectoryTree == null)
                {
                    try
                    {
                        DirectoryTree = XDocument.Load(".\\root\\metadata.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to load metadata file.\n{0}", e.ToString());
                    }
                }
            }
            else
            {
                DirectoryTree = new XDocument(new XElement("root"));
                while (!File.Exists(".\\root\\metadata.xml"))
                {
                    try
                    {
                        DirectoryTree.Save(".\\root\\metadata.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to save new metadata file.\n{0}", e.ToString());
                    }
                }
            }
        }
    }
}

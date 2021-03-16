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
        private static readonly object DirectoryTreeLock = new object();
        private static readonly object NavigatorsLock = new object();
        private static XDocument DirectoryTree = null;
        private static readonly List<Navigation> Navigators = new List<Navigation>();

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
                uploader.AddServiceEndpoint(typeof(IUpload), new WSHttpBinding(), uploadAddress);
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

        public static void AddNavigator(Navigation navigator)
        {
            lock (NavigatorsLock) Navigators.Add(navigator);
        }

        public static void RemoveNavigator(Navigation navigator)
        {
            lock (NavigatorsLock) Navigators.Remove(navigator);
        }

        public static XElement CopyRoot()
        {
            XElement root;
            lock (DirectoryTreeLock) root = new XElement(DirectoryTree.Root); // Deep-copy of the root element of the tree
            return root;
        }

        public static void UpdateNavigators(XElement newRoot)
        {
            lock (NavigatorsLock) // Set the Root of all active Navigation instances
                foreach (Navigation navigator in Navigators)
                    navigator.UpdateRoot(newRoot);
        }

        public static bool AddNewUser(string username)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            XElement newRoot;

            lock (DirectoryTreeLock)
            {
                DirectoryTree.Element("root").Add(
                    new XElement("user",
                        new XAttribute("name", username),
                        new XAttribute("date", date),
                        new XAttribute("projects", 0)));
                try
                {
                    DirectoryTree.Save(".\\root\\metadata.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                    return false;
                }

                newRoot = new XElement(DirectoryTree.Root);
            }

            Task.Run(() => UpdateNavigators(newRoot));

            return true;
        }

        public static bool AddNewProject(XElement project)
        {
            IEnumerable<XElement> findUser;
            IEnumerable<XElement> matchingProjects;
            XElement user;
            XElement newRoot = null;

            lock (DirectoryTreeLock)
            {
                findUser = from XElement element in DirectoryTree.Elements("root").Elements("user")
                           where element.Attribute("name").Value.Equals(project.Attribute("author").Value)
                           select element;

                if (findUser.Count() == 1)
                {
                    user = findUser.First();

                    matchingProjects = from XElement element in user.Elements("project")
                                       where element.Attribute("name").Value.Equals(project.Attribute("name").Value)
                                       select element;

                    if (matchingProjects.Count() == 0)
                    {
                        user.Attribute("projects").Value = (int.Parse(user.Attribute("projects").Value) + 1).ToString();
                        user.AddFirst(new XElement(project));

                        try
                        {
                            Directory.CreateDirectory(".\\root\\" + project.Attribute("author").Value + "\\" + project.Attribute("name").Value);

                            try
                            {
                                DirectoryTree.Save(".\\root\\metadata.xml");
                                newRoot = new XElement(DirectoryTree.Root);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                                while (Directory.Exists(".\\root\\" + project.Attribute("author").Value + "\\" + project.Attribute("name").Value))
                                {
                                    try
                                    {
                                        Directory.Delete(".\\root\\" + project.Attribute("author").Value + "\\" + project.Attribute("name").Value);
                                    }
                                    catch (Exception f)
                                    {
                                        Console.WriteLine("Failed to remove project directory.\n{0}", f.ToString());
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to create new project directory.\n{0}", e.ToString());
                        }
                    }
                }
            }

            if (newRoot != null)
            {
                Task.Run(() => UpdateNavigators(newRoot));
                return true;
            }

            return false;
        }

        public static XElement GetNewVersion(string username, string projectName, string timestamp)
        {
            IEnumerable<XElement> findProject;
            XElement version = null;
            int versionNumber;

            lock (DirectoryTreeLock)
            {
                findProject = from XElement element in DirectoryTree.Elements("root").Elements("user").Elements("project")
                              where element.Attribute("author").Value.Equals(username) && element.Attribute("name").Value.Equals(projectName)
                              select element;

                if (findProject.Count() == 1)
                {
                    versionNumber = findProject.First().Elements("version").Count();

                    try
                    {
                        Directory.CreateDirectory(".\\root\\" + username + "\\" + projectName + "\\" + versionNumber);

                        version = new XElement("version",
                            new XAttribute("name", projectName),
                            new XAttribute("number", versionNumber),
                            new XAttribute("author", username),
                            new XAttribute("date", timestamp)
                        );
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to create new version directory.\n{0}", e.ToString());
                    }
                }
            }

            return version;
        }

        public static bool AddNewVersion(XElement version, XElement functionAnalysis, XElement relationshipAnalysis, List<XElement> codeFiles)
        {
            IEnumerable<XElement> findProject;
            XElement project;
            XElement newRoot = null;

            lock (DirectoryTreeLock)
            {
                findProject = from XElement user in DirectoryTree.Elements("root").Elements("user")
                              where user.Attribute("name").Value.Equals(version.Attribute("author").Value)
                              from XElement element in user.Elements("project")
                              where element.Attribute("name").Value.Equals(version.Attribute("name").Value)
                              select element;

                if (findProject.Count() == 1)
                {
                    project = findProject.First();

                    // TODO: version.Add(new XElement(functionAnalysis));
                    // TODO: version.Add(new XElement(relationshipAnalysis));

                    foreach (XElement element in codeFiles)
                        version.Add(new XElement(element));

                    if (project.Elements().Count() == 0)
                        project.Add(new XElement(version));
                    else
                        project.Elements().First().AddBeforeSelf(new XElement(version));

                    try
                    {
                        DirectoryTree.Save(".\\root\\metadata.xml");
                        newRoot = new XElement(DirectoryTree.Root);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to save new files to metadata file.\n{0}", e.ToString());
                    }
                }
            }

            if (newRoot != null)
            {
                Task.Run(() => UpdateNavigators(newRoot));
                return true;
            }

            return false;
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

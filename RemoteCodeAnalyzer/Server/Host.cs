///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  Host.cs - Central server class, initializes services and handles central data      ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   The module starts the server and all four services, ensuring that expected data exists.
 *   It is also used to store and operate on central data to maintain thread safety. Methods
 *   may be called by threads belonging to different clients, so any operations on data in
 *   the Host class must be thread safe. Any data sent through these methods should be
 *   cloned or copied, and the calling module should not use the clone again.
 * 
 *   Public Interface
 *   ----------------
 *   Host.Main();
 *   Host.AddNavigator((Navigation) navigator);
 *   Host.RemoveNavigator((Navigation) navigator);
 *   Host.UpdateNavigators((XElement) newRoot);
 *   XElement root = Host.CopyRoot();
 *   XElement newProject = Host.CreateProject((XElement) user, (XElement) project);
 *   XElement newVersion = Host.GetNewVersion((string) username, (string) projectName, (string) timestamp);
 *   bool add = Host.AddNewUser((string) username);
 *   bool remove = Host.RemoveUser((string) username);
 *   bool add = Host.AddNewProject((XElement) project);
 *   bool add = Host.AddNewVersion((XElement) version);
 *   Host.LogNewRequest((string) requestType);
 */

using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using System.ServiceModel;
using RCALibrary;

namespace Server
{
    /* Central server class which initializes and hosts all services, and maintains and protects data in multithreaded environment */
    public static class Host
    {
        private static readonly object DirectoryTreeLock = new object();
        private static readonly object NavigatorsLock = new object();
        private static XDocument DirectoryTree = null;
        private static readonly List<Navigation> Navigators = new List<Navigation>();
        private static ServiceHost authenticator;
        private static ServiceHost navigator;
        private static ServiceHost uploader;
        private static ServiceHost filereader;

        /* Initializes the host server */
        public static void Main()
        {
            // Make sure all necessary directories and files exist
            CheckRoot();
            CheckSecret();
            CheckMetadata();

            // Start all the services
            StartAuthenticator();
            StartNavigator();
            StartUploader();
            StartFileReader();
            
            // Keep the server open
            Console.ReadKey();
            authenticator.Close();
            navigator.Close();
            uploader.Close();
            filereader.Close();
        }

        /* Opens the IAuthentication service host */
        private static void StartAuthenticator()
        {
            Console.WriteLine("Initializing the Authentication service.");
            WSHttpBinding binding = new WSHttpBinding();
            Uri authenticationAddress = new Uri("http://localhost:8000/Authentication/");
            authenticator = new ServiceHost(typeof(Authentication), authenticationAddress);
            binding.OpenTimeout = new TimeSpan(1, 0, 0);
            binding.CloseTimeout = new TimeSpan(1, 0, 0);
            binding.SendTimeout = new TimeSpan(1, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);

            try
            {
                authenticator.AddServiceEndpoint(typeof(IAuthentication), binding, authenticationAddress);
                authenticator.Open();
                Console.WriteLine("The Authentication service is ready.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start the Authentication service: {0}", e.ToString());
                authenticator.Abort();
            }
        }

        /* Opens the INavigation service host */
        private static void StartNavigator()
        {
            Console.WriteLine("Initializing the Navigation service.");
            WSHttpBinding binding = new WSHttpBinding();
            Uri navigationAddress = new Uri("http://localhost:8000/Navigation/");
            navigator = new ServiceHost(typeof(Navigation), navigationAddress);
            binding.OpenTimeout = new TimeSpan(1, 0, 0);
            binding.CloseTimeout = new TimeSpan(1, 0, 0);
            binding.SendTimeout = new TimeSpan(1, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);

            try
            {
                navigator.AddServiceEndpoint(typeof(INavigation), binding, navigationAddress);
                navigator.Open();
                Console.WriteLine("The Navigation service is ready.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start the Navigation service: {0}", e.ToString());
                navigator.Abort();
            }
        }

        /* Opens the IUpload service host */
        private static void StartUploader()
        {
            Console.WriteLine("Initializing the Upload service.");
            WSHttpBinding binding = new WSHttpBinding();
            Uri uploadAddress = new Uri("http://localhost:8000/Upload/");
            uploader = new ServiceHost(typeof(Upload), uploadAddress);
            binding.OpenTimeout = new TimeSpan(1, 0, 0);
            binding.CloseTimeout = new TimeSpan(1, 0, 0);
            binding.SendTimeout = new TimeSpan(1, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);

            try
            {
                uploader.AddServiceEndpoint(typeof(IUpload), binding, uploadAddress);
                uploader.Open();
                Console.WriteLine("The Upload service is ready.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start the Upload service: {0}", e.ToString());
                uploader.Abort();
            }
        }

        /* Opens the IReadFile service host */
        private static void StartFileReader()
        {
            Console.WriteLine("Initializing the ReadFile service.");
            WSHttpBinding binding = new WSHttpBinding();
            Uri readFileAddress = new Uri("http://localhost:8000/ReadFile/");
            filereader = new ServiceHost(typeof(ReadFile), readFileAddress);
            binding.OpenTimeout = new TimeSpan(1, 0, 0);
            binding.CloseTimeout = new TimeSpan(1, 0, 0);
            binding.SendTimeout = new TimeSpan(1, 0, 0);
            binding.ReceiveTimeout = new TimeSpan(1, 0, 0);

            try
            {
                filereader.AddServiceEndpoint(typeof(IReadFile), binding, readFileAddress);
                filereader.Open();
                Console.WriteLine("The ReadFile service is ready.");
                Console.WriteLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to start the ReadFile service: {0}", e.ToString());
                filereader.Abort();
            }
        }

        /* Adds a client's navigator to the master navigators list */
        public static void AddNavigator(Navigation navigator)
        {
            lock (NavigatorsLock) Navigators.Add(navigator);
        }

        /* Removes a client's navigator to the master navigators list */
        public static void RemoveNavigator(Navigation navigator)
        {
            lock (NavigatorsLock) Navigators.Remove(navigator);
        }

        /* Updates all active navigators in the list with the new navigation tree, when the tree changes */
        public static void UpdateNavigators(XElement newRoot)
        {
            lock (NavigatorsLock) // Set the Root of all active Navigation instances
                foreach (Navigation navigator in Navigators)
                    navigator.UpdateRoot(newRoot);
        }

        /* Returns a deep copy of the root element of the navigation tree for concurrent use by multiple clients */
        public static XElement CopyRoot()
        {
            XElement root;
            lock (DirectoryTreeLock) root = new XElement(DirectoryTree.Root);
            return root;
        }

        /* Adds new project to metadata file and creates project directory */
        private static XElement CreateProject(XElement user, XElement project)
        {
            // Increment the number of projects the user owns
            user.Attribute("projects").Value = (int.Parse(user.Attribute("projects").Value) + 1).ToString();
            user.AddFirst(new XElement(project)); // Add the newest project first

            try
            {
                // Create the project directory
                Directory.CreateDirectory(".\\root\\" + project.Attribute("author").Value + "\\" + project.Attribute("name").Value);

                try
                {
                    DirectoryTree.Save(".\\root\\metadata.xml"); // Save the metadata file
                    return new XElement(DirectoryTree.Root); // Return a clone of the root
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                    // Delete the project directory
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
            catch (Exception e)
            {
                Console.WriteLine("Failed to create new project directory.\n{0}", e.ToString());
            }

            return null;
        }

        /* Creates new version directory */
        public static XElement GetNewVersion(string username, string projectName, string timestamp)
        {
            IEnumerable<XElement> findProject;
            XElement version = null;
            int versionNumber;

            lock (DirectoryTreeLock)
            {
                // Find the matching project belonging to the user
                findProject = from XElement element in DirectoryTree.Elements("root").Elements("user").Elements("project")
                              where element.Attribute("author").Value.Equals(username) && element.Attribute("name").Value.Equals(projectName)
                              select element;

                if (findProject.Count() == 1) // Make sure the project exists
                {
                    versionNumber = findProject.First().Elements("version").Count(); // Get the number of versions of the project

                    // Create the version directory
                    try
                    {
                        Directory.CreateDirectory(".\\root\\" + username + "\\" + projectName + "\\" + versionNumber);
                        // Create the version element
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

        /* Adds a new user's directory to the metadata file and updates navigators */
        public static bool AddNewUser(string username)
        {
            string date = DateTime.Now.ToString("yyyyMMdd");
            XElement newRoot = null;

            lock (DirectoryTreeLock)
            {
                // Add the new user
                DirectoryTree.Element("root").Add(
                    new XElement("user",
                        new XAttribute("name", username),
                        new XAttribute("date", date),
                        new XAttribute("projects", 0)));
                try
                {
                    DirectoryTree.Save(".\\root\\metadata.xml");
                    newRoot = new XElement(DirectoryTree.Root); // Clone the root node of the updated tree
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                }
            }

            if (newRoot != null)
            {
                Task.Run(() => UpdateNavigators(newRoot)); // Update navigation trees
                return true;
            }

            return false;
        }

        /* Removes the user directory from the metadata file and updates the navigators */
        public static bool RemoveUser(string username)
        {
            IEnumerable<XElement> user;
            XElement newRoot = null;
            
            lock (DirectoryTreeLock)
            {
                // Find the user node
                user = from XElement element in DirectoryTree.Element("root").Elements("user")
                       where element.Attribute("name").Value.Equals(username)
                       select element;

                if (user.Count() > 0) // Check that user exists
                {
                    foreach (XElement element in user) user.Remove(); // Remove the user
                    
                    try
                    {
                        DirectoryTree.Save(".\\root\\metadata.xml");
                        newRoot = new XElement(DirectoryTree.Root); // Clone the root node of the updated tree
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to save removed user to metadata file.\n{0}", e.ToString());
                    }
                }
            }

            if (newRoot != null)
            {
                Task.Run(() => UpdateNavigators(newRoot)); // Update navigation trees
                return true;
            }

            return false;
        }

        /* Checks whether new project can be created; if so, creates it and updates navigators */
        public static bool AddNewProject(XElement project)
        {
            IEnumerable<XElement> findUser;
            IEnumerable<XElement> matchingProjects;
            XElement user;
            XElement newRoot = null;

            lock (DirectoryTreeLock)
            {
                // Find the user node
                findUser = from XElement element in DirectoryTree.Elements("root").Elements("user")
                           where element.Attribute("name").Value.Equals(project.Attribute("author").Value)
                           select element;

                if (findUser.Count() == 1) // Check that the user exists
                {
                    user = findUser.First();

                    // Check for any projects by that user with the same name
                    matchingProjects = from XElement element in user.Elements("project")
                                       where element.Attribute("name").Value.Equals(project.Attribute("name").Value)
                                       select element;

                    // If the user has no projects with the name, create the new project
                    if (matchingProjects.Count() == 0) newRoot = CreateProject(user, project);
                }
            }

            if (newRoot != null) // Update navigation trees
            {
                Task.Run(() => UpdateNavigators(newRoot));
                return true;
            }

            return false;
        }

        /* Adds new version to metadata file and updates navigators */
        public static bool AddNewVersion(XElement version)
        {
            IEnumerable<XElement> findProject;
            XElement project;
            XElement newRoot = null;

            lock (DirectoryTreeLock)
            {
                // Find the project with a matching name created by this user
                findProject = from XElement user in DirectoryTree.Elements("root").Elements("user")
                              where user.Attribute("name").Value.Equals(version.Attribute("author").Value)
                              from XElement element in user.Elements("project")
                              where element.Attribute("name").Value.Equals(version.Attribute("name").Value)
                              select element;

                if (findProject.Count() == 1) // Make sure the project exists
                {
                    project = findProject.First();

                    // Add the newest version first
                    if (project.Elements().Count() == 0) project.Add(version);
                    else project.Elements().First().AddBeforeSelf(new XElement(version));

                    try
                    {
                        DirectoryTree.Save(".\\root\\metadata.xml");
                        newRoot = new XElement(DirectoryTree.Root);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to save new files to metadata file.\n{0}", e.ToString());
                        return false;
                    }
                }
            }

            if (newRoot != null) // Update navigation trees
            {
                Task.Run(() => UpdateNavigators(newRoot));
                return true;
            }

            return false;
        }

        /* Writes to console upon receiving a new request */
        public static void LogNewRequest(string requestType) =>
            Console.WriteLine(requestType + " Request Received from IP Address: {0}",
                (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

        /* Creates root directory, if it does not exist */
        private static void CheckRoot()
        {
            if (!Directory.Exists(".\\root"))
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

        /* Creates secret file, if it does not exist */
        private static void CheckSecret()
        {
            if (!File.Exists(".\\root\\secret.xml"))
            {
                XDocument secret = new XDocument(new XElement("userlist"));
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

        /* Creates the metadata file if it does not exist, and loads it */
        private static void CheckMetadata()
        {
            if (File.Exists(".\\root\\metadata.xml")) // Open the metadatafile
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
            else // Create the metadata file
            {
                DirectoryTree = new XDocument(new XElement("root"));
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

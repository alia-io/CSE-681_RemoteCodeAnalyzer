/////////////////////////////////////////////////////////////////////////////////
///                                                                           ///
///  Navigation.cs - Fulfills INavigation operation contracts                 ///
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
 *   This module fulfills the INavigation operation contracts to initialize the user's current
 *   directory on login, to navigate into a child directory, to navigate back to a parent
 *   directory, and to reset the navigator on logout. It maintains a directory tree that is
 *   updated whenever any user causes a change to it, and it maintains the user's currently
 *   occupied directory.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<INavigation> navigationFactory = new ChannelFactory<INavigation>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Navigation/"));
 *   INavigation navigator = navigationFactory.CreateChannel();
 *   DirectoryData data = navigator.Initialize((string) username);
 *   DirectoryData data = navigator.NavigateInto((string) identifier);
 *   DirectoryData data = navigator.NavigateBack();
 *   navigator.RemoveNavigator();
 */

using System.Linq;
using System.Xml.Linq;
using System.ServiceModel;
using System.Collections.Generic;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Navigation : INavigation
    {
        private readonly object ThisLock = new object();    // Lock for Root and Current (they're edited together, so may share a lock)
        private XElement Root;                              // More thread-safe version of directory tree while in PerSession mode
        private XElement CurrentDirectory;                  // More thread-safe version of current directory
        private string User;                                // Username

        /* Sets navigator to user's directory upon login */
        public DirectoryData Initialize(string username)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory;

            Host.LogNewRequest("Initialize");
            User = username;
            Host.AddNavigator(this); // Add navigator to master list of navigators

            lock (ThisLock)
            {
                Root = Host.CopyRoot(); // Deep-copy of the root element of the tree

                // Find the user's directory
                findDirectory = from XElement element in Root.Elements("user")
                                where element.Attribute("name").Value.Equals(username)
                                select element;

                if (findDirectory.Count() == 1) // Make sure user directory exists
                {
                    // Set saved navigation data and return data
                    CurrentDirectory = new XElement(findDirectory.First());
                    directory = new DirectoryData(new XElement(CurrentDirectory));
                    foreach (XElement child in CurrentDirectory.Elements())
                        directory.AddChild(new XElement(child));
                }
                else CurrentDirectory = null;
            }

            return directory;
        }

        /* Navigates the user into a specified child directory */
        public DirectoryData NavigateInto(string identifier)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory = null;
            string type = CurrentDirectory.Name.ToString();

            Host.LogNewRequest("Navigate Into");

            lock (ThisLock)
            {
                // Find the requested directory
                if (type.Equals("root") || type.Equals("user"))
                {
                    findDirectory = from XElement element in CurrentDirectory.Elements()
                                    where element.Attribute("name").Value.Equals(identifier)
                                    select element;
                }
                else if (type.Equals("project"))
                {
                    findDirectory = from XElement element in CurrentDirectory.Elements("version")
                                    where int.Parse(element.Attribute("number").Value) == int.Parse(identifier)
                                    select element;
                }

                if (findDirectory != null && findDirectory.Count() == 1) // Make sure directory exists
                {
                    // Set saved navigation data and return data
                    CurrentDirectory = new XElement(findDirectory.First());
                    directory = new DirectoryData(new XElement(CurrentDirectory));
                    foreach (XElement child in CurrentDirectory.Elements())
                        directory.AddChild(new XElement(child));
                }
            }

            return directory;
        }

        /* Navigates the user back to the current directory's parent */
        public DirectoryData NavigateBack()
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory = null;
            string type;

            Host.LogNewRequest("Navigate Back");

            lock (ThisLock)
            {
                type = CurrentDirectory.Name.ToString();
                if (!type.Equals("root")) // Can't navigate back out of root directory
                {
                    // Find the parent based on the current directory type
                    if (type.Equals("user"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                                  select user.Parent;
                    }
                    else if (type.Equals("project"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("author").Value)
                                  from XElement project in user.Elements("project")
                                  where project.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                                  select project.Parent;
                    }
                    else if (type.Equals("version"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("author").Value)
                                  from XElement project in user.Elements("project")
                                  where project.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                                  select project;
                    }

                    if (findDirectory != null && findDirectory.Count() == 1)
                    {
                        // Set saved navigation data and return data
                        CurrentDirectory = new XElement(findDirectory.First());
                        directory = new DirectoryData(new XElement(CurrentDirectory));
                        foreach (XElement child in CurrentDirectory.Elements())
                            directory.AddChild(new XElement(child));
                    }
                }
            }

            return directory;
        }

        /* Removes this navigator from the master list of navigators */
        public void RemoveNavigator()
        {
            Host.LogNewRequest("Remove Navigator");
            Host.RemoveNavigator(this);
        }

        /* Updates the local copy of the directory tree, as well as the current directory (if applicable) */
        public void UpdateRoot(XElement newRoot)
        {
            IEnumerable<XElement> current = null;
            string type;

            lock (ThisLock)
            {
                Root = new XElement(newRoot);
                type = CurrentDirectory.Name.ToString();

                // Update CurrentDirectory, depending on its type
                if (type.Equals("root"))
                {
                    CurrentDirectory = new XElement(Root);
                    return;
                }
                else if (type.Equals("user"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                              select user;
                }
                else if (type.Equals("project"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("author").Value)
                              from XElement project in user.Elements("project")
                              where project.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                              select project;
                }
                else if (type.Equals("version"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(CurrentDirectory.Attribute("author").Value)
                              from XElement version in user.Elements("project").Elements("version")
                              where version.Attribute("name").Value.Equals(CurrentDirectory.Attribute("name").Value)
                              select version;
                }
             
                if (current != null && current.Count() == 1) CurrentDirectory = current.First();
            }
        }
    }
}

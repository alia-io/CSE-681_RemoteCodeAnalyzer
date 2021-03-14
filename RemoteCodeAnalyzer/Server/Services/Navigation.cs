using RCALibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml.Linq;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    class Navigation : INavigation
    {
        private readonly object ThisLock = new object(); // Lock for Root and Current
        private XElement Root; // More thread-safe version of directory tree while in PerSession mode
        private XElement Current; // More thread-safe version of current directory
        private string User; // Username

        public void Remove()
        {
            Host.RemoveNavigator(this);
        }

        public DirectoryData Initialize(string username)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory = null;

            Console.WriteLine("Initialize Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            User = username;

            Host.AddNavigator(this);

            lock (ThisLock)
            {
                Root = Host.CopyRoot(); // Deep-copy of the root element of the tree

                findDirectory = from XElement element in Root.Elements("user")
                                where element.Attribute("name").Value.Equals(username)
                                select element;

                if (findDirectory != null && findDirectory.Count() == 1)
                {
                    Current = new XElement(findDirectory.First());

                    directory = new DirectoryData(new XElement(Current));

                    foreach (XElement child in Current.Elements())
                        directory.Children.Add(child);
                }
                else Current = null;
            }

            return directory;
        }

        public DirectoryData NavigateInto(string identifier)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory = null;
            string type = Current.Name.ToString();

            Console.WriteLine("Navigate Into Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            lock (ThisLock)
            {
                if (type.Equals("root") || type.Equals("user"))
                {
                    findDirectory = from XElement element in Current.Elements()
                                    where element.Attribute("name").Value.Equals(identifier)
                                    select element;
                }
                else if (type.Equals("version"))
                {
                    findDirectory = from XElement element in Current.Elements()
                                    where element.Attribute("version").Value.Equals(identifier)
                                    select element;
                }

                if (findDirectory != null && findDirectory.Count() == 1)
                {
                    Current = new XElement(findDirectory.First());

                    directory = new DirectoryData(new XElement(Current));

                    foreach (XElement child in Current.Elements())
                        directory.Children.Add(child);
                }
            }

            return directory;
        }

        public DirectoryData NavigateBack()
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory = null;
            string type = Current.Name.ToString();

            Console.WriteLine("Navigate Back Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            lock (ThisLock)
            {
                if (!type.Equals("root"))
                {
                    if (type.Equals("user"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                                  select user.Parent;
                    }
                    else if (type.Equals("project"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(Current.Attribute("author").Value)
                                  from XElement project in user.Elements("project")
                                  where project.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                                  select project.Parent;
                    }
                    else if (type.Equals("version"))
                    {
                        findDirectory = from XElement user in Root.Elements("user")
                                  where user.Attribute("name").Value.Equals(Current.Attribute("author").Value)
                                  from XElement version in user.Elements("project").Elements("version")
                                  where version.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                                  select version.Parent;
                    }

                    if (findDirectory != null && findDirectory.Count() == 1)
                    {
                        Current = new XElement(findDirectory.First());

                        directory = new DirectoryData(new XElement(Current));

                        foreach (XElement child in Current.Elements())
                            directory.Children.Add(child);
                    }
                }
            }

            return directory;
        }

        public void UpdateRoot(XElement newRoot)
        {
            lock (ThisLock)
            {
                string type = Current.Name.ToString();
                IEnumerable<XElement> current = null;

                Root = new XElement(newRoot);

                // Update Current
                if (type.Equals("root"))
                {
                    Current = new XElement(Root);
                    return;
                }
                else if (type.Equals("user"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                              select user;
                }
                else if (type.Equals("project"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(Current.Attribute("author").Value)
                              from XElement project in user.Elements("project")
                              where project.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                              select project;
                }
                else if (type.Equals("version"))
                {
                    current = from XElement user in Root.Elements("user")
                              where user.Attribute("name").Value.Equals(Current.Attribute("author").Value)
                              from XElement version in user.Elements("project").Elements("version")
                              where version.Attribute("name").Value.Equals(Current.Attribute("name").Value)
                              select version;
                }
             
                if (current != null && current.Count() == 1) Current = current.First();
            }
        }
    }
}

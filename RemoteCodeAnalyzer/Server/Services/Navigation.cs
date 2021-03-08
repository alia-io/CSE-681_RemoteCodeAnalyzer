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
        private readonly object RootLock = new object();
        private XElement Root; // More thread-safe version of directory tree while in PerSession mode
        private XElement Current; // Thread-safe version of current directory
        private string User; // Username

        public void Remove()
        {
            lock (Host.NavigatorsLock) Host.Navigators.Remove(this);
        }

        public DirectoryData Initialize(string username)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory;
            IEnumerable<XElement> findChildren;
            string date;
            string date1;

            Console.WriteLine("Initialize Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            User = username;

            lock (Host.DirectoryTreeLock)
                lock (RootLock)
                    Root = new XElement(Host.DirectoryTree.Root); // Deep-copy of the root element of the tree

            lock (Host.NavigatorsLock) Host.Navigators.Add(this);

            lock (RootLock)
            {
                findDirectory = from XElement element in Root.Elements()
                                where element.Name.ToString().Equals("user")
                                where element.Attribute("name").Value.Equals(username)
                                select element;

                if (findDirectory.Count() == 1)
                {
                    Current = new XElement(findDirectory.First());

                    findChildren = from XElement element in Current.Elements()
                                   where element.Name.ToString().Equals("project")
                                   select element;

                    date = Current.Attribute("date").Value;
                    directory = new DirectoryData();
                    directory.CurrentDirectory = new UserDirectory(Current.Attribute("name").Value, 
                        new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2))), 
                        int.Parse(Current.Attribute("projects").Value));
                    directory.Children = new List<IFileSystemItem>();

                    foreach (XElement child in findChildren)
                    {
                        date = child.Attribute("created").Value;
                        date1 = child.Attribute("edited").Value;
                        directory.Children.Add(new ProjectDirectory(child.Attribute("name").Value, child.Attribute("author").Value, 
                            new DateTime(int.Parse(date.Substring(0, 4)), int.Parse(date.Substring(4, 2)), int.Parse(date.Substring(6, 2))),
                            new DateTime(int.Parse(date1.Substring(0, 4)), int.Parse(date1.Substring(4, 2)), int.Parse(date1.Substring(6, 2)))));
                    }
                }
                else Current = null;
            }

            return directory;
        }

        public DirectoryData Navigate()
        {

            return new DirectoryData();
        }

        public void UpdateRoot(XElement newRoot)
        {
            lock (RootLock) Root = newRoot;
        }
    }
}

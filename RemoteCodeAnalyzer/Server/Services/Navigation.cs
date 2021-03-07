using System;
using System.IO;
using System.Xml.Linq;
using System.ServiceModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel.Channels;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    class Navigation : INavigation
    {
        private object RootLock = new object();
        private XElement Root; // More thread-safe version of directory tree while in PerSession mode
        private XElement Current; // Thread-safe version of current directory
        private string User; // Username

        public Navigation()
        {
            lock (Host.DirectoryTreeLock)
                lock (RootLock)
                    Root = new XElement(Host.DirectoryTree.Root); // Deep-copy of the root element of the tree

            lock (Host.NavigatorsLock) Host.Navigators.Add(this);
        }

        public DirectoryData Initialize(string username)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory;
            IEnumerable<XElement> findChildren;

            Console.WriteLine("Initialize Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            User = username;

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
                                   where element.Name.Equals("project")
                                   select element;

                    directory = new DirectoryData();
                    directory.CurrentDirectory = new UserDirectory(Current.Attribute("name").Value, Current.Attribute("date").Value, int.Parse(Current.Attribute("projects").Value));
                    directory.Children = new List<IFileSystemItem>();

                    foreach (XElement child in findChildren)
                        directory.Children.Add(new ProjectDirectory(child.Attribute("name").Value, child.Attribute("author").Value, child.Attribute("created").Value, child.Attribute("edited").Value));
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

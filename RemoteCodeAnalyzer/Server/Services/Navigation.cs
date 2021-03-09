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
            lock (Host.NavigatorsLock) Host.Navigators.Remove(this);
        }

        public DirectoryData Initialize(string username)
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory;

            Console.WriteLine("Initialize Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            User = username;

            lock (Host.DirectoryTreeLock)
                lock (ThisLock)
                    Root = new XElement(Host.DirectoryTree.Root); // Deep-copy of the root element of the tree

            lock (Host.NavigatorsLock) Host.Navigators.Add(this);

            lock (ThisLock)
            {
                findDirectory = from XElement element in Root.Elements("user")
                                where element.Attribute("name").Value.Equals(username)
                                select element;

                if (findDirectory.Count() == 1)
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

        public DirectoryData NavigateInto(string name)
        {

            return new DirectoryData(new XElement(""));
        }

        public DirectoryData NavigateBack()
        {
            DirectoryData directory = null;
            IEnumerable<XElement> findDirectory;

            Console.WriteLine("Navigate Back Request Received from IP Address: {0}", (OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty).Address);

            lock (ThisLock)
            {
                if (!Current.Name.ToString().Equals("root"))
                {
                    findDirectory = from XElement element in Root.Descendants(Current.Name)
                                    where XNode.DeepEquals(element, Current)
                                    select element.Parent;

                    if (findDirectory.Count() == 1)
                    {
                        Current = new XElement(findDirectory.First());

                        directory = new DirectoryData(new XElement(Current));

                        foreach (XElement child in Current.Elements())
                            directory.Children.Add(child);
                    }
                    else Current = null;
                }
            }

            return directory;
        }

        public void UpdateRoot(XElement newRoot)
        {
            lock (ThisLock) Root = new XElement(newRoot);
        }
    }
}

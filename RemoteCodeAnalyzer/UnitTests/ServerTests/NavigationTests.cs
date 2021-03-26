using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ServiceModel;
using RCALibrary;
using Server;

namespace UnitTests.ServerTests
{
    [TestClass]
    public class NavigationTests
    {
        [TestMethod]
        public void InitializeTest()
        {
            DirectoryData expectedResponse;
            DirectoryData actualResponse;
            XElement expectedCurrentDirectory = new XElement("user",
                new XAttribute("name", "testname_initialize"),
                new XAttribute("date", DateTime.Now.ToString("yyyyMMdd")),
                new XAttribute("projects", 0));
            XElement expectedChild1 = new XElement("project",
                new XAttribute("name", "testproject1_initialize"),
                new XAttribute("created", DateTime.Now.ToString("yyyyMMdd")),
                new XAttribute("edited", DateTime.Now.ToString("yyyyMMdd")),
                new XAttribute("author", "testname_initialize"));
            XElement expectedChild2 = new XElement("project",
                new XAttribute("name", "testproject2_initialize"),
                new XAttribute("created", DateTime.Now.ToString("yyyyMMdd")),
                new XAttribute("edited", DateTime.Now.ToString("yyyyMMdd")),
                new XAttribute("author", "testname_initialize"));

            expectedCurrentDirectory.Add(expectedChild1, expectedChild2);

            SetUpInitializeTest(expectedCurrentDirectory, out Task host, out INavigation navigator);

            expectedResponse = new DirectoryData(expectedCurrentDirectory);
            expectedResponse.Children.Add(expectedChild1);
            expectedResponse.Children.Add(expectedChild2);

            try
            {
                actualResponse = navigator.Initialize("testname_initialize");
                Assert.AreEqual(expectedResponse.CurrentDirectory.Name.ToString(), actualResponse.CurrentDirectory.Name.ToString());
                Assert.AreEqual(expectedResponse.CurrentDirectory.Attribute("name").Value, actualResponse.CurrentDirectory.Attribute("name").Value);
                Assert.AreEqual(expectedResponse.CurrentDirectory.Attribute("projects").Value, actualResponse.CurrentDirectory.Attribute("projects").Value);
                Assert.AreEqual(expectedResponse.Children.Count(), actualResponse.Children.Count());
                Assert.AreEqual(expectedResponse.Children[0].Name.ToString(), actualResponse.Children[0].Name.ToString());
                Assert.AreEqual(expectedResponse.Children[0].Attribute("name").Value, actualResponse.Children[0].Attribute("name").Value);
                Assert.AreEqual(expectedResponse.Children[0].Attribute("author").Value, actualResponse.Children[0].Attribute("author").Value);
                Assert.AreEqual(expectedResponse.Children[1].Name.ToString(), actualResponse.Children[1].Name.ToString());
                Assert.AreEqual(expectedResponse.Children[1].Attribute("name").Value, actualResponse.Children[1].Attribute("name").Value);
                Assert.AreEqual(expectedResponse.Children[1].Attribute("author").Value, actualResponse.Children[1].Attribute("author").Value);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to navigation initialize service: {0}", e.ToString());
                Assert.Fail();
            }
            finally
            {
                CleanUpInitializeTest();
                if (host.IsCompleted) host.Dispose();
            }
        }

        private void SetUpInitializeTest(XElement expectedCurrentDirectory, out Task host, out INavigation navigator)
        {
            ChannelFactory<INavigation> navigationFactory;
            IEnumerable<XElement> testUser;
            XDocument metadata;

            host = null;
            navigator = null;

            if (File.Exists(".\\root\\metadata.xml"))
            {
                try
                {
                    metadata = XDocument.Load(".\\root\\metadata.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to open metadata file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }

                testUser = from XElement user in metadata.Element("root").Elements("user")
                           where user.Attribute("name").Value.Equals("testname_initialize")
                           select user;

                if (testUser.Count() == 0)
                {
                    metadata.Element("root").Add(expectedCurrentDirectory);

                    try
                    {
                        metadata.Save(".\\root\\metadata.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to save metadata file: {0}", e.ToString());
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                metadata = new XDocument(new XElement("root"));

                metadata.Element("root").Add(new XElement(expectedCurrentDirectory));

                try
                {
                    metadata.Save(".\\root\\metadata.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to save metadata file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }
            }

            if (!Directory.Exists(".\\root\\testname_initialize"))
            {
                try
                {
                    Directory.CreateDirectory(".\\root\\testname_initialize");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to create user directory: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }
            }

            host = Task.Run(() => Host.Main());

            navigationFactory = new ChannelFactory<INavigation>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Navigation/"));
            navigator = navigationFactory.CreateChannel();
        }

        private void CleanUpInitializeTest()
        {
            IEnumerable<XElement> testUser;
            XDocument metadata;

            if (File.Exists(".\\root\\metadata.xml"))
            {
                try
                {
                    metadata = XDocument.Load(".\\root\\metadata.xml");

                    testUser = from XElement user in metadata.Element("root").Elements("user")
                               where user.Attribute("name").Value.Equals("testname_initialize")
                               select user;

                    if (testUser.Count() == 0)
                    {
                        foreach (XElement user in testUser) user.Remove();

                        try
                        {
                            metadata.Save(".\\root\\metadata.xml");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Unable to save metadata file: {0}", e.ToString());
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to open metadata file: {0}", e.ToString());
                }
            }

            if (Directory.Exists(".\\root\\testname_initialize"))
            {
                try
                {
                    Directory.Delete(".\\root\\testname_initialize");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to create user directory: {0}", e.ToString());
                }
            }
        }
    }
}
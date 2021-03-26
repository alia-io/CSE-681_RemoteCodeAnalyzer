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
    public class AuthenticationTests
    {
        [TestMethod]
        public void LoginTest()
        {
            bool expectedResponse = true;
            bool actualResponse;

            SetUpLoginTest(out Task host, out IAuthentication authenticator);

            if (host == null || authenticator == null) return;

            try
            {
                actualResponse = authenticator.Login(new AuthenticationRequest("testname_login", "testpass_login"));
                Assert.AreEqual(expectedResponse, actualResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to authentication login service: {0}", e.ToString());
                Assert.Fail();
            }
            finally
            {
                CleanUpLoginTest();
                if (host.IsCompleted) host.Dispose();
            }
        }

        [TestMethod]
        public void NewUserTest()
        {
            bool expectedResponse = true;
            bool actualResponse;

            SetUpNewUserTest(out Task host, out IAuthentication authenticator);
            
            try
            {
                actualResponse = authenticator.NewUser(new AuthenticationRequest("testname_newuser", "testpass_newuser", "testpass_newuser")); 
                Assert.AreEqual(expectedResponse, actualResponse);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to authentication new user service: {0}", e.ToString());
                Assert.Fail();
            }
            finally
            {
                CleanUpNewUserTest();
                if (host.IsCompleted) host.Dispose();
            }
        }

        private void SetUpLoginTest(out Task host, out IAuthentication authenticator)
        {
            ChannelFactory<IAuthentication> authenticationFactory;
            IEnumerable<XElement> testUser;
            XDocument secret;

            host = null;
            authenticator = null;

            if (File.Exists(".\\root\\secret.xml"))
            {
                try
                {
                    secret = XDocument.Load(".\\root\\secret.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to open secret file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }

                testUser = from XElement user in secret.Element("userlist").Elements("user")
                           where user.Attribute("name").Value.Equals("testname_login")
                           select user;

                if (testUser.Count() == 0)
                {
                    secret.Element("userlist").Add(new XElement("user", new XAttribute("name", "testname_login"), new XAttribute("password", "testpass_login")));

                    try
                    {
                        secret.Save(".\\root\\secret.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to save secret file: {0}", e.ToString());
                        Assert.Fail();
                        return;
                    }
                }
            }
            else
            {
                secret = new XDocument(new XElement("userlist"));

                secret.Element("userlist").Add(new XElement("user", new XAttribute("name", "testname_login"), new XAttribute("password", "testpass_login")));

                try
                {
                    secret.Save(".\\root\\secret.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to save secret file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }
            }

            host = Task.Run(() => Host.Main());

            authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
            authenticator = authenticationFactory.CreateChannel();
        }

        private void CleanUpLoginTest()
        {
            IEnumerable<XElement> testUser;
            XDocument secret;

            try
            {
                secret = XDocument.Load(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to load secret file: {0}", e.ToString());
                return;
            }

            testUser = from XElement user in secret.Element("userlist").Elements("user")
                       where user.Attribute("name").Value.Equals("testname_login")
                       select user;

            foreach (XElement user in testUser) user.Remove();

            try
            {
                secret.Save(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to save secret file: {0}", e.ToString());
            }
        }

        private void SetUpNewUserTest(out Task host, out IAuthentication authenticator)
        {
            ChannelFactory<IAuthentication> authenticationFactory;
            IEnumerable<XElement> testUser;
            XDocument metadata;
            XDocument secret;

            host = null;
            authenticator = null;

            if (File.Exists(".\\root\\secret.xml"))
            {
                try
                {
                    secret = XDocument.Load(".\\root\\secret.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to load secret file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }

                testUser = from XElement user in secret.Element("userlist").Elements("user")
                           where user.Attribute("name").Value.Equals("testname_newuser")
                           select user;

                foreach (XElement user in testUser) user.Remove();

                try
                {
                    secret.Save(".\\root\\secret.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to save secret file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }
            }

            if (File.Exists(".\\root\\metadata.xml"))
            {
                try
                {
                    metadata = XDocument.Load(".\\root\\metadata.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to load metadata file: {0}", e.ToString());
                    Assert.Fail();
                    return;
                }

                testUser = from XElement user in metadata.Element("root").Elements("user")
                           where user.Attribute("name").Value.Equals("testname_newuser")
                           select user;

                foreach (XElement user in testUser) user.Remove();

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

            if (Directory.Exists(".\\root\\testname_newuser"))
            {
                try
                {
                    Directory.Delete(".\\root\\testname_newuser");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to delete user directory {0}: ", e.ToString());
                    Assert.Fail();
                    return;
                }
            }

            host = Task.Run(() => Host.Main());

            authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
            authenticator = authenticationFactory.CreateChannel();
        }

        private void CleanUpNewUserTest()
        {
            IEnumerable<XElement> testUser;
            XDocument metadata;
            XDocument secret;

            if (File.Exists(".\\root\\secret.xml"))
            {
                try
                {
                    secret = XDocument.Load(".\\root\\secret.xml");

                    testUser = from XElement user in secret.Element("userlist").Elements("user")
                               where user.Attribute("name").Value.Equals("testname_newuser")
                               select user;

                    foreach (XElement user in testUser) user.Remove();

                    try
                    {
                        secret.Save(".\\root\\secret.xml");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to save secret file: {0}", e.ToString());
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to load secret file: {0}", e.ToString());
                }
            }

            if (File.Exists(".\\root\\metadata.xml"))
            {
                try
                {
                    metadata = XDocument.Load(".\\root\\metadata.xml");

                    testUser = from XElement user in metadata.Element("root").Elements("user")
                               where user.Attribute("name").Value.Equals("testname_newuser")
                               select user;

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
                catch (Exception e)
                {
                    Console.WriteLine("Unable to load metadata file: {0}", e.ToString());
                }
            }

            if (Directory.Exists(".\\root\\testname_newuser"))
            {
                try
                {
                    Directory.Delete(".\\root\\testname_newuser");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to delete user directory {0}: ", e.ToString());
                }
            }
        }
    }
}

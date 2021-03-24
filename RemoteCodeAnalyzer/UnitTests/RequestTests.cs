using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RCALibrary;
using Server;
using System.ServiceModel;
using System.Collections;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class RequestTests
    {
        [TestMethod]
        public void Test1()
        {
            ChannelFactory<IAuthentication> authenticationFactory;
            IAuthentication authenticator;
            XDocument secret;
            Task host;
            IEnumerable<XElement> testUser;
            bool expectedResponse = true;
            bool actualResponse;

            try
            {
                secret = XDocument.Load(".\\root\\secret.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open secret file: {0}", e.ToString());
                Assert.AreEqual(0, 1);
                return;
            }

            testUser = from XElement user in secret.Element("userlist").Elements("user")
                       where user.Attribute("name").Value.Equals("testname1")
                       select user;

            if (testUser.Count() == 0)
            {
                secret.Element("userlist").Add(new XElement("user", new XAttribute("name", "testname1"), new XAttribute("password", "testpass1")));

                try
                {
                    secret.Save(".\\root\\secret.xml");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to save secret file: {0}", e.ToString());
                    Assert.AreEqual(0, 1);
                    return;
                }
            }

            host = Task.Run(() => Host.Main());

            authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
            authenticator = authenticationFactory.CreateChannel();

            try
            {
                actualResponse = authenticator.Login(new AuthenticationRequest { Username = "testname1", Password = "testpass1", ConfirmPassword = "" });
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connecting to authentication login service: {0}", e.ToString());
                Assert.AreEqual(0, 1);
                return;
            }
            finally
            {
                // TODO: somehow close the host
            }

            Assert.AreEqual(expectedResponse, actualResponse);
            return;
        }

    }
}

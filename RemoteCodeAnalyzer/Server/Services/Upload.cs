using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class Upload : IUpload
    {
        //private XElement Home;
        public XElement NewProject(string username, string projectName)
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmm");
            IEnumerable<XElement> findUser;
            IEnumerable<XElement> matchingProjects;
            XElement user;
            XElement newRoot = null;
            XElement project = null;

            lock (Host.DirectoryTreeLock)
            {
                findUser = from XElement element in Host.DirectoryTree.Elements("root").Elements("user")
                       where element.Attribute("name").Value.Equals(username)
                       select element;

                if (findUser.Count() == 1)
                {
                    user = findUser.First();

                    matchingProjects = from XElement element in user.Elements("project")
                                       where element.Attribute("name").Value.Equals(projectName)
                                       select element;

                    if (matchingProjects.Count() == 0)
                    {
                        project = new XElement("project",
                            new XAttribute("name", projectName),
                            new XAttribute("author", username),
                            new XAttribute("created", time),
                            new XAttribute("edited", time));

                        user.Attribute("projects").Value = (int.Parse(user.Attribute("projects").Value) + 1).ToString();

                        user.Add(new XElement(project));

                        try
                        {
                            Directory.CreateDirectory(".\\root\\" + username + "\\" + projectName);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to create new project directory.\n{0}", e.ToString());
                            return null;
                        }

                        try
                        {
                            Host.DirectoryTree.Save(".\\root\\metadata.xml");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to save new user to metadata file.\n{0}", e.ToString());
                            while (Directory.Exists(".\\root\\" + username + "\\" + projectName))
                            {
                                try
                                {
                                    Directory.Delete(".\\root\\" + username + "\\" + projectName);
                                }
                                catch (Exception f)
                                {
                                    Console.WriteLine("Failed to remove project directory.\n{0}", f.ToString());
                                }
                            }
                            return null;
                        }

                        project = new XElement(project);
                        newRoot = new XElement(Host.DirectoryTree.Root);
                    }
                }
            }

            if (newRoot != null) Task.Run(() => Host.UpdateNavigators(newRoot));

            return project;
        }
    }
}

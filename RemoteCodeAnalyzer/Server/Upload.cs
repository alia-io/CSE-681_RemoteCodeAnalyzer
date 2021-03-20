using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Runtime.Serialization;
using CodeAnalyzer;
using RCALibrary;

namespace Server
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession, IncludeExceptionDetailInFaults = true)]
    public class Upload : IUpload
    {
        private XElement currentVersion;    // Current version undergoing upload and analysis
        private string currentFilePath;     // Path of the current file being written to
        private readonly List<XElement> currentFiles = new List<XElement>();   // List of files in current upload

        public XElement NewProject(string username, string projectName)
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmm");

            XElement project = new XElement("project",
                new XAttribute("name", projectName),
                new XAttribute("author", username),
                new XAttribute("created", time),
                new XAttribute("edited", time));

            if (Host.AddNewProject(project)) return project;
            else return null;
        }

        public bool NewUpload(string username, string projectName)
        {
            currentVersion = Host.GetNewVersion(username, projectName, DateTime.Now.ToString("yyyyMMddHHmm"));

            if (currentVersion == null) return false;

            // TODO: create new codeanalyzer

            return true;
        }

        public void UploadBlock(FileBlock block)
        {
            if (block.Number == 0)
            {
                if (currentFilePath != null)
                {
                    // TODO: enqueue currentFilePath in CodeAnalyzer Queue; signal CodeAnalyzer to start analyzing it
                }

                currentFilePath = ".\\root\\" + currentVersion.Attribute("author").Value 
                    + "\\" + currentVersion.Attribute("name").Value + 
                    "\\" + currentVersion.Attribute("number").Value + "\\" + block.FileName;

                currentFiles.Add(new XElement("code",
                    new XAttribute("name", block.FileName.Substring(0, block.FileName.LastIndexOf('.'))),
                    new XAttribute("type", block.FileName.Substring(block.FileName.LastIndexOf('.') + 1)),
                    new XAttribute("project", currentVersion.Attribute("name").Value),
                    new XAttribute("version", currentVersion.Attribute("number").Value),
                    new XAttribute("author", currentVersion.Attribute("author").Value),
                    new XAttribute("date", currentVersion.Attribute("date").Value),
                    new XAttribute("path", currentFilePath)
                ));
            }

            using (FileStream s = new FileStream(currentFilePath, FileMode.Append))
            {
                s.Write(block.Buffer, 0, block.Length);
            }
        }

        public XElement CompleteUpload()
        {
            XElement version = currentVersion;
            string author = version.Attribute("author").Value;
            string project = version.Attribute("name").Value;
            string number = version.Attribute("number").Value;
            string date = version.Attribute("date").Value;

            Executive analyzer = new Executive(currentFiles.Count, ".\\root\\" + author + "\\" + project + "\\" + number, project, number);

            _ = analyzer.EnqueueInputFiles(currentFiles);

            analyzer.PerformCodeAnalysis();

            Host.AddNewVersion(currentVersion, currentFiles,
                new XElement("analysis",
                    new XAttribute("type", "function"),
                    new XAttribute("project", project),
                    new XAttribute("version", version),
                    new XAttribute("author", author),
                    new XAttribute("date", date)),
                new XElement("analysis",
                    new XAttribute("type", "relationship"),
                    new XAttribute("project", project),
                    new XAttribute("version", version),
                    new XAttribute("author", author),
                    new XAttribute("date", date)));

            currentVersion = null;
            currentFilePath = null;
            currentFiles.Clear();

            return version;
        }
    }
}

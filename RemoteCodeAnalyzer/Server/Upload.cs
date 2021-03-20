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
            string directoryPath = ".\\root\\" + version.Attribute("author").Value
                + "\\" + version.Attribute("name").Value
                + "\\" + version.Attribute("number").Value;
            Executive analyzer = new Executive(currentFiles.Count, directoryPath);

            Task.Run(() =>
            {
                foreach (XElement file in currentFiles)
                {
                    analyzer.EnqueueInputFile(file.Attribute("name") + "." + file.Attribute("type"));
                }
            });

            analyzer.PerformCodeAnalysis();

            // TODO: make sure code analysis worked

            Host.AddNewVersion(currentVersion, new XElement("FA"), new XElement("RA"), currentFiles);

            currentVersion = null;
            currentFilePath = null;
            currentFiles.Clear();

            return version;
        }
    }
}

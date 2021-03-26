/////////////////////////////////////////////////////////////////////////////////
///                                                                           ///
///  Upload.cs - Fulfills IUpload operation contracts                         ///
///                                                                           ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019               ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10    ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:              ///
///                 Software Modeling and Analysis, 2021                      ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu          ///
///                                                                           ///
/////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module fulfills the IUpload operation contracts to create a new project,
 *   initialize a new upload, upload file blocks, and complete an upload. A is created
 *   independently of any other requests or uploads. A new upload should always be
 *   requested before uploading any file blocks. File blocks should then be sent repeatedly
 *   until the end of the last file has been reached. Finally, a request to complete the
 *   upload should always be sent after the last file block.
 * 
 *   Public Interface
 *   ----------------
 *   ChannelFactory<IUpload> uploadFactory = new ChannelFactory<IUpload>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Upload/"));
 *   INavigation uploader = uploadFactory.CreateChannel();
 *   XElement project = uploader.NewProject((string) username, (string) projectName);
 *   bool upload = uploader.NewUpload((string) username, (string) projectName);
 *   uploader.UploadBlock((FileBlock) block);
 *   XElement version = uploader.CompleteUpload();
 */

using System;
using System.IO;
using System.Xml.Linq;
using System.Collections.Generic;
using System.ServiceModel;
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

        /* Adds the new project directory to the user's directory and updates the metadata file */
        public XElement NewProject(string username, string projectName)
        {
            string time = DateTime.Now.ToString("yyyyMMddHHmm");

            // Create the project element
            XElement project = new XElement("project",
                new XAttribute("name", projectName),
                new XAttribute("author", username),
                new XAttribute("created", time),
                new XAttribute("edited", time));

            Host.LogNewRequest("New Project");
            
            if (Host.AddNewProject(project)) return project; // Try to add the new project
            else return null;
        }

        /* Sets the fields with saved data required for an ongoing upload spread over multiple requests */
        public bool NewUpload(string username, string projectName)
        {
            Host.LogNewRequest("New Upload");

            if (currentVersion != null) return false; // Upload is already in progress

            currentVersion = Host.GetNewVersion(username, projectName, DateTime.Now.ToString("yyyyMMddHHmm"));

            if (currentVersion == null) return false;
            else return true;
        }

        /* Appends the block to the end of the current file; if block is the start of a new file, creates the file and sets local data */
        public void UploadBlock(FileBlock block)
        {
            Host.LogNewRequest("Upload Block");

            if (block.Number == 0) // Block is the start of a new file
            {
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
                s.Write(block.Buffer, 0, block.Length); // Append the block to the current file
        }

        /* Sends all files to be analyzed by CodeAnalyzer; adds the new version and files to metadata file */
        public XElement CompleteUpload()
        {
            XElement version = currentVersion;
            string author = version.Attribute("author").Value;
            string project = version.Attribute("name").Value;
            string number = version.Attribute("number").Value;
            string date = version.Attribute("date").Value;

            // New CodeAnalyzer instance
            Executive analyzer = new Executive(".\\root\\" + author + "\\" + project + "\\" + number, project, number);

            Host.LogNewRequest("Complete Upload");

            if (!analyzer.PerformCodeAnalysis(currentFiles)) // Start CodeAnalyzer
            {
                currentVersion = null;
                currentFilePath = null;
                currentFiles.Clear();
                return null;
            }

            // Add new files to the version

            version.Add(new XElement("analysis",
                new XAttribute("type", "function"),
                new XAttribute("project", project),
                new XAttribute("version", number),
                new XAttribute("author", author),
                new XAttribute("date", date)));

            version.Add(new XElement("analysis",
                new XAttribute("type", "relationship"),
                new XAttribute("project", project),
                new XAttribute("version", number),
                new XAttribute("author", author),
                new XAttribute("date", date)));

            foreach (XElement codeFile in currentFiles) version.Add(codeFile);

            // Reset saved local data
            currentVersion = null;
            currentFilePath = null;
            currentFiles.Clear();

            if (Host.AddNewVersion(new XElement(version))) return version; // Update directory tree
            else return null;
        }
    }
}

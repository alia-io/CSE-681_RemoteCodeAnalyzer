///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  App.xaml.cs - Central client class: controls active windows, state,                ///
///                 initializes all communication channels and handles requests         ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   The module starts the client and all four service channels. It initially opens the login
 *   window, and then fulfills user actions that cause windows to change. It is responsible for
 *   saving user and directory state, as well as handling all communication with the server.
 * 
 *   Public Interface
 *   ----------------
 *   App app = this;
 *   bool start = app.MainApplication((string) user);
 *   app.LoginWindow();
 *   app.ExitMainWindow();
 *   bool login = app.RequestLogin((string) username, (string) password);
 *   bool newUser = app.RequestNewUser((string) username, (string) password, (string) confirmPassword);
 *   DirectoryData data = app.RequestNavigateInto((string) directoryIdentifier);
 *   DirectoryData data = app.RequestNavigateBack();
 *   XElement project = app.RequestNewProject((string) projectName);
 *   bool upload = app.RequestUpload((string) projectName, (List<string>) files);
 *   XElement complete = app.RequestCompleteUpload();
 *   bool read = app.RequestAnalysisFile((string) filename, out (string) fileText, out (XElement) metadata);
 *   bool read = app.RequestCodeFile((string) filename, out (string) fileText);
 */

using System;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using RCALibrary;

namespace Client
{
    /* Client startup class, responsible for maintaining state and providing communication interface with services */
    public partial class App : Application
    {
        // Saved state data
        public string User { get; private set; }
        public XElement Directory { get; private set; }

        // Service channels
        private IAuthentication authenticator;
        private INavigation navigator;
        private IUpload uploader;
        private IReadFile filereader;

        // Windows
        private LoginWindow lw;
        private MainWindow mw;

        /* Starts the client by connecting to authentication service and loading login window */
        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            lw = new LoginWindow(this);

            ChannelFactory<IAuthentication> authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Authentication/"));
            ChannelFactory<INavigation> navigationFactory = new ChannelFactory<INavigation>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Navigation/"));
            ChannelFactory<IUpload> uploadFactory = new ChannelFactory<IUpload>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Upload/"));
            ChannelFactory<IReadFile> readFileFactory = new ChannelFactory<IReadFile>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/ReadFile/"));

            authenticator = authenticationFactory.CreateChannel();
            navigator = navigationFactory.CreateChannel();
            uploader = uploadFactory.CreateChannel();
            filereader = readFileFactory.CreateChannel();

            User = null;
            lw.Show();
        }

        /* Logs in to the main application by requesting initial directory data and loading main window */
        public bool MainApplication(string user)
        {
            DirectoryData directory = null;

            try
            {
                directory = navigator.Initialize(User);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
            }

            if (directory != null)
            {
                User = user;
                Directory = directory.CurrentDirectory;
                mw = new MainWindow(this, directory.CurrentDirectory, directory.Children);
                lw.Close();
                mw.Show();
                return true;
            }

            return false;
        }

        /* Logs out of the main application by loading the login window */
        public void LoginWindow()
        {
            ExitMainWindow();
            lw = new LoginWindow(this);
            mw.Close();
            lw.Show();
        }

        /* Performs maintenance of server and client state whenever main window is closed */
        public void ExitMainWindow()
        {
            try
            {
                navigator.RemoveNavigator();
                Directory = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
            }

            try
            {
                authenticator.Logout(User);
                User = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to authentication service: {0}", e.ToString());
            }
        }

        /* Sends login Authentication request to server */
        public bool RequestLogin(string username, string password)
        {
            bool response = false;

            try
            {
                if (response = authenticator.Login(new AuthenticationRequest(username, password)))
                    User = username;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to authentication service: {0}", e.ToString());
            }

            return response;
        }

        /* Sends new user Authentication request to server */
        public bool RequestNewUser(string username, string password, string confirmPassword)
        {
            try
            {
                return authenticator.NewUser(new AuthenticationRequest(username, password, confirmPassword));
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to authentication service: {0}", e.ToString());
                return false;
            }
        }

        /* Sends navigate into Navigation request to server */
        public DirectoryData RequestNavigateInto(string directoryIdentifier)
        {
            DirectoryData data = null;

            try
            {
                data = navigator.NavigateInto(directoryIdentifier);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
            }

            if (data != null) Directory = data.CurrentDirectory;

            return data;
        }

        /* Sends navigate back Navigation request to server */
        public DirectoryData RequestNavigateBack()
        {
            DirectoryData data = null;

            try
            {
                data = navigator.NavigateBack();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
            }

            if (data != null) Directory = data.CurrentDirectory;

            return data;
        }

        /* Sends new project Upload request to server */
        public XElement RequestNewProject(string projectName)
        {
            try
            {
                return uploader.NewProject(User, projectName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                return null;
            }
        }

        /* Sends new upload Upload request and then upload block Upload requests to server repeatedly, until all files have been read */
        public bool RequestUpload(string projectName, List<string> files)
        {
            int blockNumber;
            bool newUpload;

            try
            {
                newUpload = uploader.NewUpload(User, projectName); // Request new upload
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                return false;
            }

            if (newUpload)
            {
                foreach (string filepath in files) // Iterate through all files in list to be uploaded
                {
                    blockNumber = 0;
                    using (FileStream s = new FileStream(filepath, FileMode.Open, FileAccess.Read)) // Read each file
                    {
                        while (s.Length > s.Position)
                        {
                            FileBlock block = new FileBlock(Path.GetFileName(filepath), blockNumber); // Create the next block

                            if (s.Length - s.Position < block.Buffer.Length) block.Length = (int)(s.Length - s.Position);
                            else block.Length = block.Buffer.Length;

                            s.Read(block.Buffer, 0, block.Length); // Read the next 16000 bytes into the buffer

                            try
                            {
                                uploader.UploadBlock(block); // Request upload block
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                                return false;
                            }
                            blockNumber++;
                        }
                    }
                }
                return true;
            }
            
            return false;
        }

        /* Sends complete upload Upload request to server */
        public XElement RequestCompleteUpload()
        {
            try
            {
                return uploader.CompleteUpload();
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                return null;
            }
        }

        /* Sends requests to read a file to the server, then sends file metadata ReadFile request to server */
        public bool RequestAnalysisFile(string filename, out string fileText, out XElement metadata)
        {
            string user = Directory.Attribute("author").Value;
            string project = Directory.Attribute("name").Value;
            string version = Directory.Attribute("number").Value;
            metadata = null;    // Metadata for analysis element - list of lines that need severity highlighting

            if (!RequestReadFile(user, project, version, filename, out fileText)) return false; // File reading failed

            // Request metadata
            try
            {
                metadata = filereader.FileMetadata(user, project, version, filename.Substring(0, filename.IndexOf('_')));
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to readfile service: {0}", e.ToString());
            }
            
            return true;
        }

        /* Sends requests to read a file to the server */
        public bool RequestCodeFile(string filename, out string fileText)
        {
            string user = Directory.Attribute("author").Value;
            string project = Directory.Attribute("name").Value;
            string version = Directory.Attribute("number").Value;

            return RequestReadFile(user, project, version, filename, out fileText);
        }

        /* Sends read block ReadFile requests to server repeatedly until entire file is read */
        private bool RequestReadFile(string user, string project, string version, string filename, out string fileText)
        {
            FileBlock block;    // The next file block
            fileText = "";      // Output file text

            using (MemoryStream s = new MemoryStream())
            {
                do
                {
                    try
                    {
                        block = filereader.ReadBlock(user, project, version, filename); // Request the next block
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to connect to readfile service: {0}", e.ToString());
                        return false;
                    }

                    if (block == null) return false;
                    s.Write(block.Buffer, 0, block.Length); // Read the next 16000 bytes from the buffer
                }
                while (!block.LastBlock); // Continue until end of file

                s.Position = 0;
                using (StreamReader r = new StreamReader(s)) // Convert MemoryStream bytes into a string
                    fileText = r.ReadToEnd(); // Saved file text
            }

            return true;
        }
    }
}
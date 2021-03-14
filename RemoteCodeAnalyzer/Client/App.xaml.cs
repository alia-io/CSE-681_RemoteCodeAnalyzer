﻿using System;
using System.IO;
using System.ServiceModel;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using System.Windows;
using RCALibrary;

namespace Client
{
    public partial class App : Application
    {
        public string User { get; private set; }
        public XElement Directory { get; private set; }
        private IAuthentication authenticator;
        private INavigation navigator;
        private IUpload uploader;
        private LoginWindow lw;
        private MainWindow mw;

        /* Start the client by connecting to authentication service and loading login window */
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Thread.Sleep(5000); // Can remove when done testing

            lw = new LoginWindow(this);
            ChannelFactory<IAuthentication> authenticationFactory = new ChannelFactory<IAuthentication>(new WSHttpBinding(SecurityMode.None), new EndpointAddress("http://localhost:8000/Authentication/"));
            ChannelFactory<INavigation> navigationFactory = new ChannelFactory<INavigation>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Navigation/"));
            ChannelFactory<IUpload> uploadFactory = new ChannelFactory<IUpload>(new WSHttpBinding(), new EndpointAddress("http://localhost:8000/Upload/"));

            authenticator = authenticationFactory.CreateChannel();
            navigator = navigationFactory.CreateChannel();
            uploader = uploadFactory.CreateChannel();

            User = null;
            lw.Show();
        }

        /* Login to the main application loading main window */
        public void Main_Application(string user)
        {
            User = user;
            DirectoryData directory;

            try
            {
                directory = navigator.Initialize(User);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
                return;
            }

            if (directory != null)
            {
                Directory = directory.CurrentDirectory;
                mw = new MainWindow(this, directory.CurrentDirectory, directory.Children);
                lw.Close();
                mw.Show();
            }
        }

        /* Logout of the main application by loading the login window */
        public void Login_Window()
        {
            RemoveNavigator();
            User = null;
            lw = new LoginWindow(this);
            mw.Close();
            lw.Show();
        }

        public void RemoveNavigator()
        {
            try
            {
                navigator.Remove();
                Directory = null;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to navigation service: {0}", e.ToString());
            }
        }

        public bool RequestLogin(string username, string password)
        {
            bool response = false;

            try
            {
                if (response = authenticator.Login(new AuthenticationRequest { Username = username, Password = password, ConfirmPassword = "" }))
                    User = username;
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to authentication service: {0}", e.ToString());
            }

            return response;
        }

        public bool RequestNewUser(string username, string password, string confirmPassword)
        {
            try
            {
                return authenticator.NewUser(new AuthenticationRequest { Username = username, Password = password, ConfirmPassword = confirmPassword });
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to authentication service: {0}", e.ToString());
                return false;
            }
        }

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

        public XElement RequestUpload(string projectName, List<string> files)
        {
            //Thread.Sleep(30000);
            int blockNumber;
            bool newUpload = false;

            try
            {
                newUpload = uploader.NewUpload(User, projectName);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                return null;
            }

            if (newUpload)
            {
                foreach (string filepath in files)
                {
                    using (Stream s = new FileStream(filepath, FileMode.Open))
                    {
                        blockNumber = 0;
                        while (s.Length > s.Position)
                        {
                            FileBlock block = new FileBlock(Path.GetFileName(filepath), blockNumber);

                            if (s.Length - s.Position < block.Buffer.Length)
                                block.Length = (int)(s.Length - s.Position);
                            else
                                block.Length = block.Buffer.Length;

                            s.Read(block.Buffer, 0, block.Length);

                            try
                            {
                                uploader.UploadBlock(block);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Unable to connect to upload service: {0}", e.ToString());
                                return null;
                            }
                            blockNumber++;
                        }
                    }
                }

                // TODO: Change "Uploading" animation to "Analyzing" animation

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
            
            // TODO: Error message = could not upload file(s)
            return null;
        }
    }
}
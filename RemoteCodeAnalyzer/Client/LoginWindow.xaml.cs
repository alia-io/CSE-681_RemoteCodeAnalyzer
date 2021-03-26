///////////////////////////////////////////////////////////////////////////////////
///                                                                             ///
///  LoginWindow.xaml.cs - Startup and event handlers for the login window      ///
///                                                                             ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                 ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10      ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                ///
///                 Software Modeling and Analysis, 2021                        ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu            ///
///                                                                             ///
///////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module renders the login window and handles login window events.
 * 
 *   Public Interface
 *   ----------------
 *   LoginWindow loginWindow = new LoginWindow((App) app);
 *   ***All public methods from inherited Window class***
 */

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Documents;

namespace Client
{
    /* Window for creating new users and for logging into the main application */
    public partial class LoginWindow : Window
    {
        private readonly App app; // Saved reference to App

        public LoginWindow(App app)
        {
            this.app = app;
            InitializeComponent();
        }

        /* If Return or Enter key was pressed, triggers either login button or confirm button (depending on whether in login mode or new user mode) */
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
            {
                if (Title.Equals("Remote Code Analyzer: Login"))
                    LoginButton_Click(LoginButton, e);
                else if (Title.Equals("Remote Code Analyzer: New User"))
                    ConfirmButton_Click(ConfirmButton, e);
            }
        }

        /* Attempts to login to the main application window */
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string user = Username.Text;
            Mouse.OverrideCursor = Cursors.Wait;

            if (app.RequestLogin(user, Password.Password)) // Login was successful
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                Message.Text = "Enter your username and password to login.";
                Username.Text = "";
                Password.Password = "";
                app.MainApplication(user);
            }
            else // Login failed
            {
                Message.Text = "";
                Message.Inlines.Add(new Run("Invalid username or password.") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                Username.Text = "";
                Password.Password = "";
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        /* Attempts to add a new user */
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            if (app.RequestNewUser(Username.Text, Password.Password, ConfirmPassword.Password)) // New user request was successful
            {
                Username.Text = "";
                Password.Password = "";
                ConfirmPassword.Password = "";
                Title = "Remote Code Analyzer: Login";
                BackButton.Visibility = Visibility.Collapsed;
                NewUserButton.Visibility = Visibility.Visible;
                ConfirmButton.Visibility = Visibility.Collapsed;
                LoginButton.Visibility = Visibility.Visible;
                ConfirmPasswordBlock.Visibility = Visibility.Collapsed;
                ConfirmPassword.Visibility = Visibility.Collapsed;
                Message.Text = "";
                Message.Inlines.Add(new Run("New account created!") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                Mouse.OverrideCursor = Cursors.Arrow;
            }
            else // New user request failed
            {
                Message.Text = "";
                Message.Inlines.Add(new Run("Failed to create new account.") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                Username.Text = "";
                Password.Password = "";
                ConfirmPassword.Password = "";
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        /* Switches from Login mode to New User mode */
        private void NewUserButton_Click(object sender, RoutedEventArgs e)
        {
            Title = "Remote Code Analyzer: New User";
            Message.Text = "Enter a new username and password to create an account.";
            NewUserButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;
            LoginButton.Visibility = Visibility.Collapsed;
            ConfirmButton.Visibility = Visibility.Visible;
            ConfirmPasswordBlock.Visibility = Visibility.Visible;
            ConfirmPassword.Visibility = Visibility.Visible;
        }

        /* Switches from New User mode to Login mode */
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Title = "Remote Code Analyzer: Login";
            Message.Text = "Enter your username and password to login.";
            BackButton.Visibility = Visibility.Collapsed;
            NewUserButton.Visibility = Visibility.Visible;
            ConfirmButton.Visibility = Visibility.Collapsed;
            LoginButton.Visibility = Visibility.Visible;
            ConfirmPasswordBlock.Visibility = Visibility.Collapsed;
            ConfirmPassword.Visibility = Visibility.Collapsed;
        }

        /* Exits the application */
        private void QuitButton_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Threading;

namespace Client
{
    public partial class LoginWindow : Window
    {
        private readonly App app; // Saved reference to App
        public LoginWindow(App app)
        {
            this.app = app;
            InitializeComponent();
        }

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

        public void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string user = Username.Text;

            Mouse.OverrideCursor = Cursors.Wait;
            if (app.RequestLogin(user, Password.Password))
            {
                Mouse.OverrideCursor = Cursors.Arrow;
                Message.Text = "Enter your username and password to login.";
                Username.Text = "";
                Password.Password = "";
                app.Main_Application(user);
            }
            else
            {
                Message.Text = "";
                Message.Inlines.Add(new Run("Invalid username or password.") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                Username.Text = "";
                Password.Password = "";
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        public void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            if (app.RequestNewUser(Username.Text, Password.Password, ConfirmPassword.Password))
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
            else
            {
                Message.Text = "";
                Message.Inlines.Add(new Run("Failed to create new account.") { FontWeight = FontWeights.Bold, Foreground = Brushes.Red });
                Username.Text = "";
                Password.Password = "";
                ConfirmPassword.Password = "";
                Mouse.OverrideCursor = Cursors.Arrow;
            }
        }

        public void NewUserButton_Click(object sender, RoutedEventArgs e)
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

        public void BackButton_Click(object sender, RoutedEventArgs e)
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

        public void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

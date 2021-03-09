﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Globalization;
using RCALibrary;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly App app; // Saved reference to App
        private readonly Button LastClickedButton;
        private readonly List<string> newFiles = new List<string>();
        public MainWindow(App app, XElement current, List<XElement> children)
        {
            this.app = app;
            InitializeComponent();
            SetExplorer(current, children);
            SetProjects(children);
        }

        ~MainWindow() => app.RemoveNavigator();

        private void SetExplorer(XElement current, List<XElement> children)
        {
            string type = current.Name.ToString();
            int i = 0;
            DateTime date;

            ExplorerHeader.Children.RemoveRange(0, ExplorerHeader.Children.Count - 1);
            DirectoryName.Text = "";

            if (type.Equals("root"))
            {
                DirectoryName.Text = "Users";
            }
            else
            {
                StackPanel leftPanel = new StackPanel { Orientation = Orientation.Horizontal };
                string left = "";

                DirectoryName.Text = current.Attribute("name").Value;

                DockPanel.SetDock(leftPanel, Dock.Left);
                ExplorerHeader.Children.Insert(0, leftPanel);

                if (type.Equals("user") || type.Equals("project"))
                    left = current.Name.ToString().Substring(0, 1).ToUpper() + current.Name.ToString().Substring(1);

                else if (type.Equals("version"))
                {
                    StackPanel rightPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    left = current.Attribute("number").Value;

                    DockPanel.SetDock(rightPanel, Dock.Right);
                    ExplorerHeader.Children.Insert(1, rightPanel);
                    DateTime.TryParseExact(current.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                    
                    rightPanel.Children.Add(new Rectangle
                    {
                        Fill = FindResource("LineColor") as SolidColorBrush,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        Width = 3
                    });

                    rightPanel.Children.Add(new TextBlock
                    {
                        Text = date.ToString("d"),
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(10, 5, 10, 5),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Foreground = FindResource("TextColor") as SolidColorBrush
                    });
                }

                leftPanel.Children.Add(new TextBlock
                {
                    Text = left,
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(10, 5, 10, 5),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Foreground = FindResource("TextColor") as SolidColorBrush
                });

                leftPanel.Children.Add(new Rectangle
                {
                    Fill = FindResource("LineColor") as SolidColorBrush,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Width = 3
                });
            }

            foreach (XElement child in children)
            {
                StackPanel outerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                StackPanel innerPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
                Image image = new Image { VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
                TextBlock header = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = FindResource("TextColor") as SolidColorBrush };
                Button button;

                type = child.Name.ToString();

                outerPanel.Children.Add(image);
                outerPanel.Children.Add(innerPanel);
                innerPanel.Children.Add(header);

                if (type.Equals("user") || type.Equals("project") || type.Equals("version"))
                {
                    TextBlock line1 = new TextBlock { FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };
                    TextBlock line2 = new TextBlock { FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

                    button = new Button { Content = outerPanel, Height = 75, Margin = new Thickness(10, 5, 10, 5) };
                    button.Click += DirectoryButton_Click;
                    header.MaxWidth = 286;
                    header.Text = child.Attribute("name").Value;
                    innerPanel.Children.Add(line1);
                    innerPanel.Children.Add(line2);

                    if (type.Equals("user"))
                    {
                        DateTime.TryParseExact(child.Attribute("date").Value, "yyyyMMdd", null, DateTimeStyles.None, out date);
                        button.Name = "User" + i++;
                        image.Source = new BitmapImage(new Uri("/Icons/user-directory.png", UriKind.Relative));
                        line1.Text = "Joined: " + date.ToString("d");
                        line2.Text = child.Attribute("projects").Value + " Projects";
                    }
                    else
                    {
                        TextBlock line3 = new TextBlock { FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

                        line1.Text = "Author: " + child.Attribute("author").Value;
                        innerPanel.Children.Add(line3);

                        if (type.Equals("project"))
                        {
                            button.Name = "Project" + i++;
                            image.Source = new BitmapImage(new Uri("/Icons/project-directory.png", UriKind.Relative));
                            DateTime.TryParseExact(child.Attribute("created").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            line2.Text = "Created: " + date.ToString("g");
                            DateTime.TryParseExact(child.Attribute("edited").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            line3.Text = "Last Upload: " + date.ToString("g");
                        }
                        else if (type.Equals("version"))
                        {
                            DateTime.TryParseExact(child.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            button.Name = "Version" + i++;
                            image.Source = new BitmapImage(new Uri("/Icons/version-directory.png", UriKind.Relative));
                            line2.Text = "Uploaded: " + date.ToString("g");
                            line3.Text = "Version: " + child.Attribute("number").Value;
                        }
                    }
                }
                else if (type.Equals("code") || type.Equals("analysis"))
                {
                    button = new Button { Height = 100, Margin = new Thickness(15, 5, 15, 5) };
                    button.Content = outerPanel;
                    header.MaxWidth = 93;

                    if (type.Equals("code"))
                    {
                        TextBlock line = new TextBlock { FontSize = 10, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

                        button.Name = "Code" + i++;
                        button.Click += CodeButton_Click;
                        image.Source = new BitmapImage(new Uri("/Icons/file.png", UriKind.Relative));
                        header.Text = child.Attribute("name").Value;
                        innerPanel.Children.Add(line);

                        if (child.Attribute("type").Value.Equals("txt")) line.Text = "Text";
                        else if (child.Attribute("type").Value.Equals("cs")) line.Text = "C#";
                        else if (child.Attribute("type").Value.Equals("java")) line.Text = "Java";
                    }
                    else if (type.Equals("analysis"))
                    {
                        button.Name = "Analysis" + i++;
                        button.Click += AnalysisButton_Click;
                        image.Source = new BitmapImage(new Uri("/Icons/xml-file.png", UriKind.Relative));
                        header.Text = char.ToUpper(child.Attribute("type").Value[0]) + child.Attribute("type").Value.Substring(1)
                            + Environment.NewLine + "Analysis";
                    }
                }
                else continue;

                Explorer.Children.Add(button);
            }
        }

        private void SetProjects(List<XElement> projects)
        {
            // ComboBoxItem Content="project.Attribute("name").Value" FontSize="12" HorizontalAlignment="Stretch" HorizontalContentAlignment="Center"


            foreach (XElement project in projects)
                Projects.Items.Add(new ComboBoxItem { Content = project.Attribute("name").Value, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center });
        }

        private void TabSelectionChanged(object sender, SelectionChangedEventArgs args)
        {
            TabItem tabItem;

            if (!IsLoaded) return;

            tabItem = (TabItem)(sender as TabControl).SelectedItem;

            BackButton.Visibility = Visibility.Collapsed;
            NewProjectButton.Visibility = Visibility.Collapsed;
            LogOutButton.Visibility = Visibility.Collapsed;
            QuitButton.Visibility = Visibility.Collapsed;
            AddFileButton.Visibility = Visibility.Collapsed;
            UploadProjectButton.Visibility = Visibility.Collapsed;
            ResetButton.Visibility = Visibility.Collapsed;

            if (tabItem == ExplorerTab)
            {
                BackButton.Visibility = Visibility.Visible;
                LogOutButton.Visibility = Visibility.Visible;
                QuitButton.Visibility = Visibility.Visible;
            }
            else if (tabItem == UploadTab)
            {
                NewProjectButton.Visibility = Visibility.Visible;
                AddFileButton.Visibility = Visibility.Visible;
                UploadProjectButton.Visibility = Visibility.Visible;
                ResetButton.Visibility = Visibility.Visible;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryData data = app.RequestNavigateBack();

            if (data != null) SetExplorer(data.CurrentDirectory, data.Children);
        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            app.Login_Window();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            NewProjectPanel.Visibility = Visibility.Visible;
        }

        private void NewProjectName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                ConfirmButton_Click(NewProjectButton, e);
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            XElement newProject = app.RequestNewProject(NewProjectName.Text);
            bool isUserDirectory = false;
            bool isThisUser = false;

            if (newProject != null)
            {
                if (ExplorerHeader.Children.Count == 2)
                {
                    foreach (UIElement element in ExplorerHeader.Children)
                        if (element.GetType() == typeof(TextBlock) && ((TextBlock)element).Name.Equals("DirectoryName") && ((TextBlock)element).Text.Equals(app.User))
                            isThisUser = true;
                        else if (element.GetType() == typeof(StackPanel) && ((StackPanel)element).Children.Count == 2)
                            foreach (UIElement child in ((StackPanel)element).Children)
                                if (child.GetType() == typeof(TextBlock) && ((TextBlock)child).Text.Equals("User"))
                                    isUserDirectory = true;
                }

                if (isUserDirectory && isThisUser) // Add new project to Explorer view
                {
                    StackPanel outerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    StackPanel innerPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
                    Button button = new Button { Name = "Project" + Explorer.Children.Count, Content = outerPanel, Height = 75, Margin = new Thickness(10, 5, 10, 5) };
                    DateTime date;

                    button.Click += DirectoryButton_Click;
                    DateTime.TryParseExact(newProject.Attribute("created").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);

                    outerPanel.Children.Add(new Image { Source = new BitmapImage(new Uri("/Icons/project-directory.png", UriKind.Relative)), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right });
                    outerPanel.Children.Add(innerPanel);

                    innerPanel.Children.Add(new TextBlock { Text = newProject.Attribute("name").Value, MaxWidth = 286, FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Author: " + newProject.Attribute("author").Value, FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Created: " + date.ToString("g"), FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Last Upload: " + date.ToString("g"), FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });

                    Explorer.Children.Insert(0, button);
                }

                // Add new project to Projects dropdown and select it
                Projects.Items.Insert(0, new ComboBoxItem { Content = newProject.Attribute("name").Value, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center });
                Projects.SelectedIndex = 0;

                NewProjectName.Text = "";
                NewProjectPanel.Visibility = Visibility.Collapsed;
                MessageBox.Show("New project added!");
            }
            else
            {
                NewProjectName.Text = "";
                MessageBox.Show("Unable to create new project." + Environment.NewLine + "The project may already exist.");
            }
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            IEnumerable<string> filenames = from string filepath in newFiles select System.IO.Path.GetFileName(filepath);
            int currentCount = FileList.Items.Count - 1;
            
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|C# Files (*.cs)|*.cs|Java Files (*.java)|*.java";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    string filepath = openFileDialog.FileNames[i];
                    string filename = System.IO.Path.GetFileName(filepath);
                    if (!filenames.Contains(filename))
                    {
                        filenames.Append(filename);
                        newFiles.Add(filepath);
                        AddToFileList(filename, i + currentCount);
                    }
                }
            }
        }

        private void AddToFileList(string filename, int index)
        {
            DockPanel panel = new DockPanel { LastChildFill = true };
            Button button = new Button
            {
                Content = new Image { Style = FindResource("RemoveIcon") as Style },
                ToolTip = "Remove File",
                Width = 20,
                Height = 12
            };

            DockPanel.SetDock(button, Dock.Right);
            button.Click += RemoveButton_Click;

            panel.Children.Add(button);
            panel.Children.Add(new TextBlock
            {
                Text = filename,
                FontSize = 14,
                FontWeight = FontWeights.Light,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = FindResource("TextColor") as SolidColorBrush
            });

            FileList.Items.Insert(FileList.Items.Count - 1, new ListBoxItem { Name = "I" + index.ToString(), Content = panel });
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            ListBoxItem listBoxItem = (ListBoxItem)((DockPanel)(sender as Button).Parent).Parent;
            int index = int.Parse(listBoxItem.Name[1].ToString());

            if (FileList.Items.Count <= 2)
            {
                FileList.Items.Clear();
                newFiles.Clear();
                FileList.Items.Add(new ListBoxItem());
                return;
            }

            newFiles.RemoveAt(index);
            FileList.Items.Remove(listBoxItem);

            for (int i = index; i < FileList.Items.Count - 1; i++)
                ((ListBoxItem)FileList.Items[i]).Name = "I" + i.ToString();
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Projects.SelectedItem = null;
            FileList.Items.Clear();
            FileList.Items.Add(new ListBoxItem());
            newFiles.Clear();
        }

        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CodeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ResetLastClickedButton()
        {

        }
    }
}

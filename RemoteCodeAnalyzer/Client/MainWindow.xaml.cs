using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RCALibrary;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly App app; // Saved reference to App
        private readonly List<string> newFiles = new List<string>();
        public MainWindow(App app, IFileSystemItem current, List<IFileSystemItem> children)
        {
            this.app = app;
            InitializeComponent();
            // TODO: set GUI with current & children data
        }

        ~MainWindow()
        {

            // TODO: remove from server lists
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

        }

        private void LogOutButton_Click(object sender, RoutedEventArgs e)
        {
            app.Login_Window();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void NewProject_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                NewProjectButton_Click(NewProjectButton, e);
        }

        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            IEnumerable<string> filenames = from string filepath in newFiles select Path.GetFileName(filepath);
            int currentCount = FileList.Items.Count - 1;
            
            openFileDialog.Filter = "Text Files (*.txt)|*.txt|C# Files (*.cs)|*.cs|Java Files (*.java)|*.java";
            openFileDialog.FilterIndex = 2;
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                for (int i = 0; i < openFileDialog.FileNames.Length; i++)
                {
                    string filepath = openFileDialog.FileNames[i];
                    string filename = Path.GetFileName(filepath);
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
                Content = new Image { Style = (Style) FindResource("RemoveIcon") },
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
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F4F1DE"))
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
            VersionName.SelectedItem = null;
            FileList.Items.Clear();
            FileList.Items.Add(new ListBoxItem());
            newFiles.Clear();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ProjectButton_Click(object sender, RoutedEventArgs e)
        {
            NewProjectPanel.Visibility = Visibility.Visible;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {



            NewProjectName.Text = "";
            NewProjectPanel.Visibility = Visibility.Visible;
        }

        private void VersionButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CodeButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

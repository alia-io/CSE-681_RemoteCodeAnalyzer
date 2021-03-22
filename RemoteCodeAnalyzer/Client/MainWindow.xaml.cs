using Microsoft.Win32;
using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using System.Windows.Threading;
using System.Windows.Documents;
using System.Text.RegularExpressions;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly App app; // Saved reference to App
        private readonly List<string> newFiles = new List<string>();
        private (Button button, long timestamp) LastClickedButton = (null, 0);

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
            Explorer.Children.Clear();
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
                        button.Name = "U" + i++;
                        image.Source = new BitmapImage(new Uri("/Assets/Icons/user-directory.png", UriKind.Relative));
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
                            button.Name = "P" + i++;
                            image.Source = new BitmapImage(new Uri("/Assets/Icons/project-directory.png", UriKind.Relative));
                            DateTime.TryParseExact(child.Attribute("created").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            line2.Text = "Created: " + date.ToString("g");
                            DateTime.TryParseExact(child.Attribute("edited").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            line3.Text = "Last Upload: " + date.ToString("g");
                        }
                        else if (type.Equals("version"))
                        {
                            DateTime.TryParseExact(child.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
                            button.Name = "V" + child.Attribute("number").Value;
                            image.Source = new BitmapImage(new Uri("/Assets/Icons/version-directory.png", UriKind.Relative));
                            line2.Text = "Uploaded: " + date.ToString("g");
                            line3.Text = "Version: " + child.Attribute("number").Value;
                        }
                    }
                }
                else if (type.Equals("code") || type.Equals("analysis"))
                {
                    button = new Button { Height = 100, Margin = new Thickness(15, 5, 15, 5) };
                    button.Content = outerPanel;
                    header.MaxWidth = 186;

                    if (type.Equals("code"))
                    {
                        TextBlock line = new TextBlock { FontSize = 10, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

                        button.Name = "C" + i++;
                        button.Click += CodeButton_Click;
                        image.Source = new BitmapImage(new Uri("/Assets/Icons/file.png", UriKind.Relative));
                        header.Text = child.Attribute("name").Value;
                        innerPanel.Children.Add(line);

                        if (child.Attribute("type").Value.Equals("txt")) line.Text = "Language: Text";
                        else if (child.Attribute("type").Value.Equals("cs")) line.Text = "Language: C#";
                        else if (child.Attribute("type").Value.Equals("java")) line.Text = "Language: Java";
                    }
                    else if (type.Equals("analysis"))
                    {
                        button.Name = "A" + i++;
                        button.Click += AnalysisButton_Click;
                        image.Source = new BitmapImage(new Uri("/Assets/Icons/xml-file.png", UriKind.Relative));
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
            foreach (XElement project in projects)
                Projects.Items.Add(new ComboBoxItem 
                { 
                    Content = project.Attribute("name").Value, 
                    FontSize = 12, 
                    HorizontalAlignment = HorizontalAlignment.Stretch, 
                    HorizontalContentAlignment = HorizontalAlignment.Center 
                });
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

            ResetLastClickedButton();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            DirectoryData data = app.RequestNavigateBack();

            if (data != null)
            {
                SetExplorer(data.CurrentDirectory, data.Children);
                ClearLastClickedButton();
            }
            else ResetLastClickedButton();
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
            FileListGrid.RowDefinitions[1].Height = new GridLength(50);
            NewProjectPanel.Visibility = Visibility.Visible;
        }

        private void NewProjectName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                ConfirmButton_Click(NewProjectButton, e);
        }

        // TODO: change MessageBoxes to Red(?) message text above the TextBox
        // TODO: on success, collapse the TextBox but still display the success message on a timer. At the end of the timer, collapse the message.
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            XElement newProject;
            bool isUserDirectory = false;
            bool isThisUser = false;

            // TODO: add some additional requirements for project name
            if (NewProjectName.Text.Length < 1)
            {
                NewProjectName.Text = "";
                MessageBox.Show("Unable to create a new project without a name.");
                return;
            }

            newProject = app.RequestNewProject(NewProjectName.Text);

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
                    Button button = new Button { Name = "P" + Explorer.Children.Count, Content = outerPanel, Height = 75, Margin = new Thickness(10, 5, 10, 5) };

                    button.Click += DirectoryButton_Click;
                    DateTime.TryParseExact(newProject.Attribute("created").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);

                    outerPanel.Children.Add(new Image { Source = new BitmapImage(new Uri("/Assets/Icons/project-directory.png", UriKind.Relative)), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right });
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
                FileListGrid.RowDefinitions[1].Height = new GridLength(0);
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
            int currentCount = FileList.Children.Count;
            
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
            DockPanel panel = new DockPanel { Name = "I" + index.ToString(), LastChildFill = true, Margin = new Thickness(0, 0, 0, 5) };

            if (FileList.Children.Count == 0)
            {
                Rectangle top = new Rectangle
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    Height = 1,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#888888"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                DockPanel.SetDock(top, Dock.Top);
                panel.Children.Add(top);
            }

            Rectangle bottom = new Rectangle
            {
                Margin = new Thickness(0, 5, 0, 0),
                Height = 1,
                Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#888888"),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            DockPanel.SetDock(bottom, Dock.Bottom);
            panel.Children.Add(bottom);

            Button button = new Button
            {
                Content = new Image { Style = FindResource("RemoveIcon") as Style },
                ToolTip = "Remove File",
                Margin = new Thickness(0, 0, 5, 0),
                Width = 20,
                Height = 12
            };
            DockPanel.SetDock(button, Dock.Right);
            button.Click += RemoveButton_Click;
            panel.Children.Add(button);

            TextBlock text = new TextBlock
            {
                Text = filename,
                FontSize = 14,
                FontWeight = FontWeights.Light,
                Margin = new Thickness(5, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Foreground = FindResource("TextColor") as SolidColorBrush
            };
            DockPanel.SetDock(text, Dock.Left);
            panel.Children.Add(text);

            FileList.Children.Add(panel);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            DockPanel panel = (DockPanel)(sender as Button).Parent;
            int index = int.Parse(panel.Name.Substring(1));

            if (index == 0 && FileList.Children.Count > 1)
            {
                Rectangle top = new Rectangle
                {
                    Margin = new Thickness(0, 0, 0, 5),
                    Height = 1,
                    Fill = (SolidColorBrush)new BrushConverter().ConvertFrom("#888888"),
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };
                DockPanel.SetDock(top, Dock.Top);
                ((DockPanel)FileList.Children[1]).Children.Insert(0, top);
            }

            newFiles.RemoveAt(index);
            FileList.Children.RemoveAt(index);

            for (int i = index; i < FileList.Children.Count; i++)
                ((DockPanel)FileList.Children[i]).Name = "I" + i.ToString();
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            XElement newVersion = null;
            string projectName;
            DispatcherTimer animate = new DispatcherTimer(DispatcherPriority.Normal);

            // TODO: Message: Select a project to upload to, or create a new one.
            if (Projects.SelectedItem == null) return;
            if (newFiles.Count < 1) return; // TODO: Message: add files to upload

            projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();

            NewProjectName.Text = "";
            UploadTab.MouseEnter += MouseWait;
            UploadTab.MouseLeave += MouseArrow;
            NewProjectButton.IsEnabled = false;
            AddFileButton.IsEnabled = false;
            UploadProjectButton.IsEnabled = false;
            ResetButton.IsEnabled = false;
            Projects.IsEnabled = false;
            FileList.Children.Clear();
            NewProjectPanel.Visibility = Visibility.Collapsed;
            FileListGrid.Visibility = Visibility.Collapsed;
            RightAnimation.Visibility = Visibility.Visible;

            animate.Interval = TimeSpan.FromMilliseconds(50);
            animate.Tick += UploadAnimation;
            animate.Start();

            if (await Task.Run(() => app.RequestUpload(projectName, newFiles)))
            {
                animate.Stop();
                animate.Tick -= UploadAnimation;
                animate.Tick += AnalyzeAnimation;
                animate.Start();

                newVersion = await Task.Run(() => app.RequestCompleteUpload());
            }

            RightAnimation.Visibility = Visibility.Collapsed;
            animate.Stop();
            FileListGrid.Visibility = Visibility.Visible;
            RightAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Uploading/uploading-0.png", UriKind.Relative));
            UploadTab.MouseEnter -= MouseWait;
            UploadTab.MouseLeave -= MouseArrow;
            NewProjectButton.IsEnabled = true;
            AddFileButton.IsEnabled = true;
            UploadProjectButton.IsEnabled = true;
            ResetButton.IsEnabled = true;
            Projects.IsEnabled = true;
            Projects.SelectedItem = null;
            newFiles.Clear();

            if (newVersion == null)
            {
                // TODO: display error message ("Could not upload files?")
            }
            else
            {
                if (app.Directory.Name.ToString().Equals("project") && app.Directory.Attribute("author").Value.Equals(app.User) && app.Directory.Attribute("name").Value.Equals(newVersion.Attribute("name").Value)) // Add new version to Explorer view
                {
                    StackPanel outerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                    StackPanel innerPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Left };
                    Button button = new Button { Name = "V" + newVersion.Attribute("number").Value, Content = outerPanel, Height = 75, Margin = new Thickness(10, 5, 10, 5) };

                    button.Click += DirectoryButton_Click;
                    DateTime.TryParseExact(newVersion.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);

                    outerPanel.Children.Add(new Image { Source = new BitmapImage(new Uri("/Assets/Icons/version-directory.png", UriKind.Relative)), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right });
                    outerPanel.Children.Add(innerPanel);

                    innerPanel.Children.Add(new TextBlock { Text = newVersion.Attribute("name").Value, MaxWidth = 286, FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Author: " + newVersion.Attribute("author").Value, FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Uploaded: " + date.ToString("g"), FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });
                    innerPanel.Children.Add(new TextBlock { Text = "Version: " + newVersion.Attribute("number").Value, FontSize = 10, MaxWidth = 289, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush });

                    Explorer.Children.Insert(0, button);
                }

                // TODO: change this.
                // TODO: Display: Files uploaded! for a few seconds, then disappear --> put this message on the top of BOTH tabs
                MessageBox.Show("New version added!");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Projects.SelectedItem = null;
            FileList.Children.Clear();
            newFiles.Clear();
            NewProjectName.Text = "";
            FileListGrid.RowDefinitions[1].Height = new GridLength(0);
            NewProjectPanel.Visibility = Visibility.Collapsed;
            UploadProjectMessage.Text = "";
            FileListGrid.RowDefinitions[0].Height = new GridLength(0); // GridLength is 25 when open
            UploadProjectMessage.Visibility = Visibility.Collapsed;
        }

        private void DirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (LastClickedButton.button != null
                && (sender as Button).Name.Equals(LastClickedButton.button.Name)
                && LastClickedButton.timestamp - LastClickedButton.timestamp < 5000000) // Double-click
            {
                char type = (sender as Button).Name[0];
                DirectoryData data = null;

                if (type == 'U' || type == 'P')
                    data = app.RequestNavigateInto(((TextBlock)((StackPanel)((StackPanel)(sender as Button).Content).Children[1]).Children[0]).Text);
                else if (type == 'V')
                    data = app.RequestNavigateInto((sender as Button).Name.Substring(1));

                if (data != null)
                {
                    SetExplorer(data.CurrentDirectory, data.Children);
                    ClearLastClickedButton();
                    return;
                }
            }
            
            SetLastClickedButton(sender as Button);
        }

        private async void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: disable clicking code or analysis file buttons or back button --> ONLY DISABLE TEMPORARILY TO GET INFO?

            DispatcherTimer animate = new DispatcherTimer(DispatcherPriority.Normal);
            TextBlock header = (TextBlock)((StackPanel)((StackPanel)(sender as Button).Content).Children[1]).Children[0];
            string analysisType = header.Text.Substring(0, header.Text.IndexOf(Environment.NewLine));
            string filename = char.ToLower(analysisType[0]) + analysisType.Substring(1) + "_analysis.xml";
            bool getFileText;

            // Variables used for writing file text
            string fileText = "";
            XElement metadata = null;
            string[] fileTextLines;
            int currentLine = 1;
            IEnumerator<int> lowSeverity;
            IEnumerator<int> mediumSeverity;
            IEnumerator<int> highSeverity;
            int nextLowLine = -1;
            int nextMediumLine = -1;
            int nextHighLine = -1;
            SolidColorBrush lowColor = FindResource("LowSeverity") as SolidColorBrush;
            SolidColorBrush mediumColor = FindResource("MediumSeverity") as SolidColorBrush;
            SolidColorBrush highColor = FindResource("HighSeverity") as SolidColorBrush;

            SetLastClickedButton(sender as Button);

            Version.Text = "";
            Date.Text = "";
            ProjectName.Text = "";
            LFileTypeBox.Background = Brushes.Transparent;
            RFileTypeBox.Background = Brushes.Transparent;
            LFileType.Text = "";
            RFileType.Text = "";
            FileText.Text = "";

            Mouse.OverrideCursor = Cursors.Wait;
            Explorer.IsEnabled = false;
            BackButton.IsEnabled = false;
            LeftPanel.MouseEnter += MouseWait;
            LeftPanel.MouseLeave += MouseArrow;
            ExplorerTab.MouseEnter += MouseWait;
            ExplorerTab.MouseLeave += MouseArrow;
            FileTextHeaders.Visibility = Visibility.Collapsed;
            FileTextView.Visibility = Visibility.Collapsed;
            FileTextHeadersRow.Height = new GridLength(0);
            LeftAnimation.Visibility = Visibility.Visible;

            animate.Interval = TimeSpan.FromMilliseconds(50);
            animate.Tick += LoadAnimation;
            animate.Start();

            getFileText = await Task.Run(() => app.RequestAnalysisFile(filename, out fileText, out metadata));
            
            LeftAnimation.Visibility = Visibility.Collapsed;
            animate.Stop();

            if (getFileText)
            {
                DateTime.TryParseExact(app.Directory.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);
                Version.Text = app.Directory.Attribute("number").Value;
                Date.Text = date.ToString("d");
                ProjectName.Text = app.Directory.Attribute("name").Value;
                LFileTypeBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2A9D8F");
                RFileTypeBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#2A9D8F");
                LFileType.Text = "XML";
                RFileType.Text = "XML";
                FileName.Text = analysisType + " Analysis";

                if (metadata == null) FileText.Text = fileText;
                else
                {
                    fileTextLines = Regex.Split(fileText, "\r\n|\r|\n");

                    lowSeverity = (from XElement severity in metadata.Elements("severity")
                                  where severity.Attribute("level").Value.Equals("low")
                                  from XElement element in severity.Elements("line")
                                  select int.Parse(element.Value)).GetEnumerator();

                    mediumSeverity = (from XElement severity in metadata.Elements("severity")
                                      where severity.Attribute("level").Value.Equals("medium")
                                      from XElement element in severity.Elements("line")
                                      select int.Parse(element.Value)).GetEnumerator();

                    highSeverity = (from XElement severity in metadata.Elements("severity")
                                    where severity.Attribute("level").Value.Equals("high")
                                    from XElement element in severity.Elements("line")
                                    select int.Parse(element.Value)).GetEnumerator();

                    if (lowSeverity.MoveNext()) nextLowLine = lowSeverity.Current;
                    if (mediumSeverity.MoveNext()) nextMediumLine = mediumSeverity.Current;
                    if (highSeverity.MoveNext()) nextHighLine = highSeverity.Current;

                    foreach (string line in fileTextLines)
                    {
                        if (nextLowLine == currentLine)
                        {
                            FileText.Inlines.Add(new Run(line + "\r\n") { FontWeight = FontWeights.Bold, Foreground = lowColor });
                            if (lowSeverity.MoveNext()) nextLowLine = lowSeverity.Current;
                            else nextLowLine = -1;
                        }
                        else if (nextMediumLine == currentLine)
                        {
                            FileText.Inlines.Add(new Run(line + "\r\n") { FontWeight = FontWeights.Bold, Foreground = mediumColor });
                            if (mediumSeverity.MoveNext()) nextMediumLine = mediumSeverity.Current;
                            else nextMediumLine = -1;
                        }
                        else if (nextHighLine == currentLine)
                        {
                            FileText.Inlines.Add(new Run(line + "\r\n") { FontWeight = FontWeights.Bold, Foreground = highColor });
                            if (highSeverity.MoveNext()) nextHighLine = highSeverity.Current;
                            else nextHighLine = -1;
                        }
                        else FileText.Inlines.Add(line + "\r\n");
                        currentLine++;
                    }
                }

                FileTextHeadersRow.Height = new GridLength(105);
                FileTextHeaders.Visibility = Visibility.Visible;
                FileTextView.Visibility = Visibility.Visible;
                // TODO: success message (temp)
            }
            else
            {
                // TODO: error message (temp)
                // FileTextView.Visibility = Visibility.Visible; //???
            }

            Explorer.IsEnabled = true;
            BackButton.IsEnabled = true;
            LeftAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Loading/loading-0.png", UriKind.Relative));
            LeftPanel.MouseEnter -= MouseWait;
            LeftPanel.MouseLeave -= MouseArrow;
            ExplorerTab.MouseEnter -= MouseWait;
            ExplorerTab.MouseLeave -= MouseArrow;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private async void CodeButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: disable clicking code or analysis file buttons or back button --> ONLY DISABLE TEMPORARILY TO GET INFO?

            DispatcherTimer animate = new DispatcherTimer(DispatcherPriority.Normal);
            StackPanel innerPanel = (StackPanel)((StackPanel)(sender as Button).Content).Children[1];
            string filename = ((TextBlock)innerPanel.Children[0]).Text;
            string language = ((TextBlock)innerPanel.Children[1]).Text;
            string type = language.Substring(language.LastIndexOf(' ') + 1);
            string fileType = "";
            string fileText = "";
            bool getFileText;

            if (type.Equals("Text")) fileType = "txt";
            else if (type.Equals("C#")) fileType = "cs";
            else if (type.Equals("Java")) fileType = "java";

            SetLastClickedButton(sender as Button);

            Version.Text = "";
            Date.Text = "";
            ProjectName.Text = "";
            LFileTypeBox.Background = Brushes.Transparent;
            RFileTypeBox.Background = Brushes.Transparent;
            LFileType.Text = "";
            RFileType.Text = "";
            FileText.Text = "";

            Mouse.OverrideCursor = Cursors.Wait;
            Explorer.IsEnabled = false;
            BackButton.IsEnabled = false;
            LeftPanel.MouseEnter += MouseWait;
            LeftPanel.MouseLeave += MouseArrow;
            ExplorerTab.MouseEnter += MouseWait;
            ExplorerTab.MouseLeave += MouseArrow;
            FileTextHeaders.Visibility = Visibility.Collapsed;
            FileTextView.Visibility = Visibility.Collapsed;
            FileTextHeadersRow.Height = new GridLength(0);
            LeftAnimation.Visibility = Visibility.Visible;

            animate.Interval = TimeSpan.FromMilliseconds(50);
            animate.Tick += LoadAnimation;
            animate.Start();

            getFileText = await Task.Run(() => app.RequestCodeFile(filename + "." + fileType, out fileText));

            LeftAnimation.Visibility = Visibility.Collapsed;
            animate.Stop();

            if (getFileText)
            {
                DateTime.TryParseExact(app.Directory.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);
                Version.Text = app.Directory.Attribute("number").Value;
                Date.Text = date.ToString("d");
                ProjectName.Text = app.Directory.Attribute("name").Value;
                LFileTypeBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#40768C");
                RFileTypeBox.Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#40768C");
                LFileType.Text = type;
                RFileType.Text = type;
                FileName.Text = filename;
                FileText.Text = fileText;

                FileTextHeadersRow.Height = new GridLength(105);
                FileTextHeaders.Visibility = Visibility.Visible;
                FileTextView.Visibility = Visibility.Visible;
                // TODO: success message (temp)
            }
            else
            {
                // TODO: error message (temp)
                // FileTextView.Visibility = Visibility.Visible; //???
            }

            Explorer.IsEnabled = true;
            BackButton.IsEnabled = true;
            LeftAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Loading/loading-0.png", UriKind.Relative));
            LeftPanel.MouseEnter -= MouseWait;
            LeftPanel.MouseLeave -= MouseArrow;
            ExplorerTab.MouseEnter -= MouseWait;
            ExplorerTab.MouseLeave -= MouseArrow;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void UploadAnimation(object sender, EventArgs e)
        {
            string source = RightAnimation.Source.ToString();
            int index;

            if (source != null)
            {
                index = source.LastIndexOf('-') + 1;
                if (int.TryParse(source.Substring(index, source.Length - index - 4), out int number))
                {
                    if (++number > 15) number = 0;
                    RightAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Uploading/uploading-" + number + ".png", UriKind.Relative));
                }
            }
        }

        private void AnalyzeAnimation(object sender, EventArgs e)
        {
            string source = RightAnimation.Source.ToString();
            int index;

            if (source != null)
            {
                index = source.LastIndexOf('-') + 1;
                if (int.TryParse(source.Substring(index, source.Length - index - 4), out int number))
                {
                    if (++number > 15) number = 0;
                    RightAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Analyzing/analyzing-" + number + ".png", UriKind.Relative));
                }
            }
        }

        private void LoadAnimation(object sender, EventArgs e)
        {
            string source = LeftAnimation.Source.ToString();
            int index;

            if (source != null)
            {
                index = source.LastIndexOf('-') + 1;
                if (int.TryParse(source.Substring(index, source.Length - index - 4), out int number))
                {
                    if (++number > 15) number = 0;
                    RightAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Loading/loading-" + number + ".png", UriKind.Relative));
                }
            }
        }

        private void ClearLastClickedButton()
        {
            LastClickedButton.button = null;
            LastClickedButton.timestamp = 0;
        }
        
        private void SetLastClickedButton(Button button)
        {
            char type = button.Name[0];

            foreach (UIElement element in ((StackPanel)button.Content).Children)
            {
                if (element.GetType() == typeof(Image))
                {
                    if (type == 'U') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/user-directory-click.png", UriKind.Relative));
                    else if (type == 'P') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/project-directory-click.png", UriKind.Relative));
                    else if (type == 'V') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/version-directory-click.png", UriKind.Relative));
                    else if (type == 'C') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/file-click.png", UriKind.Relative));
                    else if (type == 'A') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/xml-file-click.png", UriKind.Relative));
                }
                else if (element.GetType() == typeof(StackPanel))
                    foreach (UIElement child in ((StackPanel)element).Children)
                    {
                        if (type == 'U') ((TextBlock)child).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FECB4A");
                        else if (type == 'P') ((TextBlock)child).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFA343");
                        else if (type == 'V') ((TextBlock)child).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#FF8561");
                        else if (type == 'C') ((TextBlock)child).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#5E9EB9");
                        else if (type == 'A') ((TextBlock)child).Foreground = (SolidColorBrush)new BrushConverter().ConvertFrom("#40C5B5");
                    }
            }

            ResetLastClickedButton();
            LastClickedButton.button = button;
            LastClickedButton.timestamp = DateTime.Now.Ticks;
        }

        private void ResetLastClickedButton()
        {
            if (LastClickedButton.button == null) return;

            char type = LastClickedButton.button.Name[0];

            foreach (UIElement element in ((StackPanel)LastClickedButton.button.Content).Children)
            {
                if (element.GetType() == typeof(Image))
                {
                    if (type == 'U') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/user-directory.png", UriKind.Relative));
                    else if (type == 'P') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/project-directory.png", UriKind.Relative));
                    else if (type == 'V') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/version-directory.png", UriKind.Relative));
                    else if (type == 'C') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/file.png", UriKind.Relative));
                    else if (type == 'A') ((Image)element).Source = new BitmapImage(new Uri("/Assets/Icons/xml-file.png", UriKind.Relative));
                }
                else if (element.GetType() == typeof(StackPanel))
                    foreach (UIElement child in ((StackPanel)element).Children)
                        ((TextBlock)child).Foreground = FindResource("TextColor") as SolidColorBrush;
            }
        }

        private void MouseWait(object _sender, MouseEventArgs _e) => Mouse.OverrideCursor = Cursors.Wait;
        private void MouseArrow(object _sender, MouseEventArgs _e) => Mouse.OverrideCursor = Cursors.Arrow;
    }
}

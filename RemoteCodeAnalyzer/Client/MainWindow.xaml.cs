///////////////////////////////////////////////////////////////////////////////////
///                                                                             ///
///  MainWindow.xaml.cs - Startup and event handlers for the main window        ///
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
 *   This module renders the main window and handles main window events.
 * 
 *   Public Interface
 *   ----------------
 *   MainWindow mainWindow = new MainWindow((App) app, (XElement) current, (List<XElement>) children);
 *   ***All public methods from inherited Window class***
 */

using System;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RCALibrary;

namespace Client
{
    public partial class MainWindow : Window
    {
        private readonly App app;                                               // Saved reference to App
        private readonly List<string> newFiles = new List<string>();            // List of filepaths from files added to upload list
        private (Button button, long timestamp) LastClickedButton = (null, 0);  // Saved button that was last clicked

        public MainWindow(App app, XElement current, List<XElement> children)
        {
            this.app = app;
            InitializeComponent();
            Title = "Remote Code Analyzer - " + app.User;
            SetExplorer(current, children);
            SetProjects(children);
        }

        ~MainWindow() => app.ExitMainWindow();

        /* Sets mouse cursor to wait icon/animation */
        private void MouseWait(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Wait;

        /* Sets mouse cursor to normal arrow */
        private void MouseArrow(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Arrow;

        /* Logs out of the main application and opens the login window */
        private void LogOutButton_Click(object sender, RoutedEventArgs e) => app.LoginWindow();

        /* Exits the application */
        private void QuitButton_Click(object sender, RoutedEventArgs e) => Environment.Exit(0);

        /* Changes active buttons depending on which tab is currently active */
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

        /* Attempts to navigate back to the parent directory and rerenders the file explorer */
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

        /* Displays the grid area used to enter a new project name */
        private void NewProjectButton_Click(object sender, RoutedEventArgs e)
        {
            FileListGrid.RowDefinitions[1].Height = new GridLength(50);
            NewProjectPanel.Visibility = Visibility.Visible;
        }

        /* If Return or Enter key was pressed, triggers confirm button */
        private void NewProjectName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return || e.Key == Key.Enter)
                ConfirmButton_Click(NewProjectButton, e);
        }

        /* Attempts to add a new project with user-provided name */
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            XElement newProject;

            // TODO: probably sanitize this input
            if (NewProjectName.Text.Length < 1)
            {
                NewProjectName.Text = "";
                StartUploadTabMessage("Unable to create new project without a name.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));
                return;
            }

            newProject = app.RequestNewProject(NewProjectName.Text);

            if (newProject != null)
            {
                if (app.Directory.Name.ToString().Equals("user") && app.Directory.Attribute("name").Value.Equals(app.User))
                    AddExplorerChild(newProject, 0, Explorer.Children.Count); // Add new project to Explorer view

                // Add new project to Projects dropdown and select it
                Projects.Items.Insert(0, new ComboBoxItem { Content = newProject.Attribute("name").Value, FontSize = 12, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Center });
                Projects.SelectedIndex = 0;

                FileListGrid.RowDefinitions[1].Height = new GridLength(0);
                NewProjectPanel.Visibility = Visibility.Collapsed;
                StartUploadTabMessage("New project added!", (SolidColorBrush)new BrushConverter().ConvertFrom("#40C5B5"));
            }
            else StartUploadTabMessage("Unable to create new project.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));

            NewProjectName.Text = "";
        }

        /* Creates OpenFileDialog to explore and add local files to list */
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

        /* Renders the new file name to the list of files to upload */
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

        /* Removes a file from the list of files to upload */
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

        /* Attempts to upload a new project version */
        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            DispatcherTimer animate = new DispatcherTimer(DispatcherPriority.Normal);
            XElement newVersion = null;
            string projectName;
            
            if (Projects.SelectedItem == null)
            {
                StartUploadTabMessage("Select a project to upload to, or create a new one.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));
                return;
            }

            if (newFiles.Count < 1)
            {
                StartUploadTabMessage("Add at least one file to upload.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));
                return;
            }

            projectName = ((ComboBoxItem)Projects.SelectedItem).Content.ToString();
            DisableUpload();
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

            animate.Stop();
            EnableUpload();

            if (newVersion == null)
            {
                StartUploadTabMessage("An error occurred while attempting to upload files.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));
                StartExplorerTabMessage("An error occurred while attempting to upload files.", (SolidColorBrush)new BrushConverter().ConvertFrom("#BC4749"));
            }
            else
            {
                if (app.Directory.Name.ToString().Equals("project") && app.Directory.Attribute("author").Value.Equals(app.User)
                    && app.Directory.Attribute("name").Value.Equals(newVersion.Attribute("name").Value))
                        AddExplorerChild(newVersion, 0, Explorer.Children.Count); // Add new version to Explorer view

                StartUploadTabMessage("New files uploaded!", (SolidColorBrush)new BrushConverter().ConvertFrom("#40C5B5"));
                StartExplorerTabMessage("New files uploaded!", (SolidColorBrush)new BrushConverter().ConvertFrom("#40C5B5"));
            }
        }

        /* Clears the current file list and closes the new project grid area */
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Projects.SelectedItem = null;
            FileList.Children.Clear();
            newFiles.Clear();
            NewProjectName.Text = "";
            UploadProjectMessage.Text = "";
            FileListGrid.RowDefinitions[1].Height = new GridLength(0);
            UploadProjectMessageRow.Height = new GridLength(0);
            NewProjectPanel.Visibility = Visibility.Collapsed;
            UploadProjectMessage.Visibility = Visibility.Collapsed;
        }

        /* Attempts to navigate into the selected directory */
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

        /* Attempts to display the text of the selected analysis file */
        private async void AnalysisButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock header = (TextBlock)((StackPanel)((StackPanel)(sender as Button).Content).Children[1]).Children[0];
            string analysisType = header.Text.Substring(0, header.Text.IndexOf(Environment.NewLine));
            string filename = char.ToLower(analysisType[0]) + analysisType.Substring(1) + "_analysis.xml";
            bool getFileText;

            // Variables used for writing file text
            string fileText = "";
            XElement metadata = null;

            DispatcherTimer animate = DisableNavigation(sender as Button);

            getFileText = await Task.Run(() => app.RequestAnalysisFile(filename, out fileText, out metadata));
            
            LeftAnimation.Visibility = Visibility.Collapsed;
            animate.Stop();

            if (getFileText)
            {
                SetFileTextHeader(analysisType + " Analysis", "XML", (SolidColorBrush)new BrushConverter().ConvertFrom("#2A9D8F"));

                if (metadata == null) FileText.Text = fileText;
                else
                {
                    IEnumerator<int> lowSeverity = (from XElement severity in metadata.Elements("severity")
                                                    where severity.Attribute("level").Value.Equals("low")
                                                    from XElement element in severity.Elements("line")
                                                    select int.Parse(element.Value)).GetEnumerator();

                    IEnumerator<int> mediumSeverity = (from XElement severity in metadata.Elements("severity")
                                                       where severity.Attribute("level").Value.Equals("medium")
                                                       from XElement element in severity.Elements("line")
                                                       select int.Parse(element.Value)).GetEnumerator();

                    IEnumerator<int> highSeverity = (from XElement severity in metadata.Elements("severity")
                                                     where severity.Attribute("level").Value.Equals("high")
                                                     from XElement element in severity.Elements("line")
                                                     select int.Parse(element.Value)).GetEnumerator();

                    WriteAnalysisFileText(fileText, lowSeverity, mediumSeverity, highSeverity);
                }
            }
            else StartFilePanelMessage("An error occurred while attempting to retrieve the file.");

            EnableNavigation();
        }

        /* Attempts to display the text of the selected code file */
        private async void CodeButton_Click(object sender, RoutedEventArgs e)
        {
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

            DispatcherTimer animate = DisableNavigation(sender as Button);

            getFileText = await Task.Run(() => app.RequestCodeFile(filename + "." + fileType, out fileText));

            LeftAnimation.Visibility = Visibility.Collapsed;
            animate.Stop();

            if (getFileText)
            {
                SetFileTextHeader(filename, fileType, (SolidColorBrush)new BrushConverter().ConvertFrom("#40768C"));
                FileText.Text = fileText;
            }
            else StartFilePanelMessage("An error occurred while attempting to retrieve the file.");

            EnableNavigation();
        }

        /* Writes the text for an analysis file to the left FileText area */
        private void WriteAnalysisFileText(string fileText, IEnumerator<int> lowSeverity, IEnumerator<int> mediumSeverity, IEnumerator<int> highSeverity)
        {
            SolidColorBrush lowColor = FindResource("LowSeverity") as SolidColorBrush;
            SolidColorBrush mediumColor = FindResource("MediumSeverity") as SolidColorBrush;
            SolidColorBrush highColor = FindResource("HighSeverity") as SolidColorBrush;
            string[] fileTextLines = Regex.Split(fileText, "\r\n|\r|\n");
            int currentLine = 1;
            int nextLowLine = -1;
            int nextMediumLine = -1;
            int nextHighLine = -1;

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

        /* Disables navigation tab while retrieving file text */
        private DispatcherTimer DisableNavigation(Button button)
        {
            DispatcherTimer animate = new DispatcherTimer(DispatcherPriority.Normal);

            SetLastClickedButton(button);

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
            FileTextMessageRow.Height = new GridLength(0);
            FileTextMessage.Text = "";
            FileTextMessage.Visibility = Visibility.Collapsed;
            FileTextHeaders.Visibility = Visibility.Collapsed;
            FileTextView.Visibility = Visibility.Collapsed;
            FileTextHeadersRow.Height = new GridLength(0);
            LeftAnimation.Visibility = Visibility.Visible;

            animate.Interval = TimeSpan.FromMilliseconds(10);
            animate.Tick += LoadAnimation;
            animate.Start();

            return animate;
        }

        /* Enables navigation tab after file text is written */
        private void EnableNavigation()
        {
            // TODO: reset scrollviewer position
            Explorer.IsEnabled = true;
            BackButton.IsEnabled = true;
            LeftAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Loading/loading-0.png", UriKind.Relative));
            LeftPanel.MouseEnter -= MouseWait;
            LeftPanel.MouseLeave -= MouseArrow;
            ExplorerTab.MouseEnter -= MouseWait;
            ExplorerTab.MouseLeave -= MouseArrow;
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /* Disables upload tab while uploading files */
        private void DisableUpload()
        {
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
        }

        /* Enables upload tab after uploading files completed */
        private void EnableUpload()
        {
            RightAnimation.Visibility = Visibility.Collapsed;
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
        }

        /* Renders the header for the FileText panel */
        private void SetFileTextHeader(string filename, string filetype, SolidColorBrush fileTypeBackground)
        {
            DateTime.TryParseExact(app.Directory.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);
            Version.Text = app.Directory.Attribute("number").Value;
            Date.Text = date.ToString("d");
            ProjectName.Text = app.Directory.Attribute("name").Value;
            LFileTypeBox.Background = fileTypeBackground;
            RFileTypeBox.Background = fileTypeBackground;
            LFileType.Text = filetype;
            RFileType.Text = filetype;
            FileName.Text = filename;
            FileTextHeadersRow.Height = new GridLength(105);
            FileTextHeaders.Visibility = Visibility.Visible;
            FileTextView.Visibility = Visibility.Visible;
        }

        /* Increment to the next uploading animation */
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

        /* Increment to the next analyzing animation */
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

        /* Increment to the next loading animation */
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
                    LeftAnimation.Source = new BitmapImage(new Uri("/Assets/Animations/Loading/loading-" + number + ".png", UriKind.Relative));
                }
            }
        }

        /* Adds timed message to file reader panel */
        private void StartFilePanelMessage(string message)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);

            FileTextMessage.Text = "";
            FileTextMessage.Text = message;
            FileTextMessage.Visibility = Visibility.Visible;
            FileTextMessageRow.Height = new GridLength(30);

            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += EndFilePanelMessage;
            timer.Start();
        }

        /* Adds timed message to the top of the file explorer tab */
        private void StartExplorerTabMessage(string message, SolidColorBrush color)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);

            ProjectExplorerMessage.Text = "";
            ProjectExplorerMessage.Inlines.Add(new Run(message) { Foreground = color });
            ProjectExplorerMessage.Visibility = Visibility.Visible;
            ProjectExplorerMessageRow.Height = new GridLength(30);

            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += EndExplorerTabMessage;
            timer.Start();
        }

        /* Adds timed message to the top of the upload tab */
        private void StartUploadTabMessage(string message, SolidColorBrush color)
        {
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Normal);

            UploadProjectMessage.Text = "";
            UploadProjectMessage.Inlines.Add(new Run(message) { Foreground = color });
            UploadProjectMessage.Visibility = Visibility.Visible;
            UploadProjectMessageRow.Height = new GridLength(30);

            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += EndUploadTabMessage;
            timer.Start();
        }

        /* Ends a timed message on file reader panel */
        private void EndFilePanelMessage(object sender, EventArgs e)
        {
            FileTextMessageRow.Height = new GridLength(0);
            FileTextMessage.Text = "";
            FileTextMessage.Visibility = Visibility.Collapsed;
            (sender as DispatcherTimer).Stop();
        }

        /* Ends a timed message on top of file explorer tab */
        private void EndExplorerTabMessage(object sender, EventArgs e)
        {
            ProjectExplorerMessageRow.Height = new GridLength(0);
            ProjectExplorerMessage.Text = "";
            ProjectExplorerMessage.Visibility = Visibility.Collapsed;
            (sender as DispatcherTimer).Stop();
        }

        /* Ends a timed message on top of upload tab */
        private void EndUploadTabMessage(object sender, EventArgs e)
        {
            UploadProjectMessageRow.Height = new GridLength(0);
            UploadProjectMessage.Text = "";
            UploadProjectMessage.Visibility = Visibility.Collapsed;
            (sender as DispatcherTimer).Stop();
        }

        /* Populates the dropdown menu with the list of projects owned by this user */
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

        /* Renders the header and the directory or file buttons within the file explorer tab */
        private void SetExplorer(XElement current, List<XElement> children)
        {
            int index = 0;

            SetExplorerHeader(current, current.Name.ToString());

            foreach (XElement child in children)
            {
                AddExplorerChild(child, Explorer.Children.Count, index);
                index++;
            }
        }

        /* Renders the header for the explorer tab */
        private void SetExplorerHeader(XElement current, string type)
        {
            ExplorerHeader.Children.RemoveRange(0, ExplorerHeader.Children.Count - 1);
            Explorer.Children.Clear();
            DirectoryName.Text = "";

            if (type.Equals("root")) DirectoryName.Text = "Users";
            else SetSubdirectoryHeader(current, type);
        }

        /* Renders the header for the explorer tab for a user, project, or version directory */
        private void SetSubdirectoryHeader(XElement current, string type)
        {
            StackPanel leftPanel = new StackPanel { Orientation = Orientation.Horizontal };
            string left = "";

            DirectoryName.Text = current.Attribute("name").Value;

            DockPanel.SetDock(leftPanel, Dock.Left);
            ExplorerHeader.Children.Insert(0, leftPanel);

            if (type.Equals("user") || type.Equals("project"))
                left = current.Name.ToString().Substring(0, 1).ToUpper() + current.Name.ToString().Substring(1);
            else if (type.Equals("version")) SetVersionHeader(current);

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

        /* Partially renders the header for the explorer tab for a version directory */
        private string SetVersionHeader(XElement current)
        {
            StackPanel rightPanel = new StackPanel { Orientation = Orientation.Horizontal };
            string left = current.Attribute("number").Value;

            DockPanel.SetDock(rightPanel, Dock.Right);
            ExplorerHeader.Children.Insert(1, rightPanel);
            DateTime.TryParseExact(current.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);

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

            return left;
        }

        /* Renders a child element (directory or file) in the explorer tab */
        private void AddExplorerChild(XElement child, int insertIndex, int nameIndex)
        {
            StackPanel outerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            StackPanel innerPanel = new StackPanel { Orientation = Orientation.Vertical, VerticalAlignment = VerticalAlignment.Center };
            Image image = new Image();
            TextBlock header = new TextBlock { FontSize = 14, TextWrapping = TextWrapping.Wrap, Foreground = FindResource("TextColor") as SolidColorBrush };
            Button button = new Button { Content = outerPanel, HorizontalAlignment = HorizontalAlignment.Left };
            Border border = new Border { Child = button, Margin = new Thickness(10) };

            string type = child.Name.ToString();

            outerPanel.Children.Add(image);
            outerPanel.Children.Add(innerPanel);
            innerPanel.Children.Add(header);

            if (type.Equals("user") || type.Equals("project") || type.Equals("version"))
                SetExplorerChildDirectory(child, type, border, innerPanel, button, header, image, nameIndex);
            else if (type.Equals("code") || type.Equals("analysis"))
                SetExplorerChildFile(child, type, border, innerPanel, button, image, header, nameIndex);
            else return;

            Explorer.Children.Insert(insertIndex, border);
        }

        /* Partially renders a child directory element in the explorer tab */
        private void SetExplorerChildDirectory(XElement child, string type, Border border, StackPanel innerPanel, Button button, TextBlock header, Image image, int index)
        {
            TextBlock line1 = new TextBlock { FontSize = 10, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };
            TextBlock line2 = new TextBlock { FontSize = 10, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

            border.Height = 75;
            border.Width = 260;
            innerPanel.MaxWidth = 160;
            button.Click += DirectoryButton_Click;
            header.Text = child.Attribute("name").Value;
            innerPanel.Children.Add(line1);
            innerPanel.Children.Add(line2);

            if (type.Equals("user")) SetExplorerChildUser(child, button, image, line1, line2, index);
            else
            {
                TextBlock line3 = new TextBlock { FontSize = 10, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

                line1.Text = "Author: " + child.Attribute("author").Value;
                innerPanel.Children.Add(line3);

                if (type.Equals("project")) SetExplorerChildProject(child, button, image, line2, line3, index);
                else if (type.Equals("version")) SetExplorerChildVersion(child, button, image, line2, line3);
            }
        }

        /* Partially renders a child user directory element in the explorer tab */
        private void SetExplorerChildUser(XElement child, Button button, Image image, TextBlock line1, TextBlock line2, int index)
        {
            DateTime.TryParseExact(child.Attribute("date").Value, "yyyyMMdd", null, DateTimeStyles.None, out DateTime date);
            button.Name = "U" + index;
            image.Source = new BitmapImage(new Uri("/Assets/Icons/user-directory.png", UriKind.Relative));
            line1.Text = "Joined: " + date.ToString("d");
            line2.Text = child.Attribute("projects").Value + " Projects";
        }

        /* Partially renders a child project directory element in the explorer tab */
        private void SetExplorerChildProject(XElement child, Button button, Image image, TextBlock line2, TextBlock line3, int index)
        {
            button.Name = "P" + index;
            image.Source = new BitmapImage(new Uri("/Assets/Icons/project-directory.png", UriKind.Relative));
            DateTime.TryParseExact(child.Attribute("created").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);
            line2.Text = "Created: " + date.ToString("g");
            DateTime.TryParseExact(child.Attribute("edited").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out date);
            line3.Text = "Last Upload: " + date.ToString("g");
        }

        /* Partially renders a child version directory element in the explorer tab */
        private void SetExplorerChildVersion(XElement child, Button button, Image image, TextBlock line2, TextBlock line3)
        {
            DateTime.TryParseExact(child.Attribute("date").Value, "yyyyMMddHHmm", null, DateTimeStyles.None, out DateTime date);
            button.Name = "V" + child.Attribute("number").Value;
            image.Source = new BitmapImage(new Uri("/Assets/Icons/version-directory.png", UriKind.Relative));
            line2.Text = "Uploaded: " + date.ToString("g");
            line3.Text = "Version: " + child.Attribute("number").Value;
        }

        /* Partially renders a child file element in the explorer tab */
        private void SetExplorerChildFile(XElement child, string type, Border border, StackPanel innerPanel, Button button, Image image, TextBlock header, int index)
        {
            border.Height = 100;
            border.Width = 210;
            innerPanel.MaxWidth = 130;

            if (type.Equals("code")) SetExplorerChildCode(child, innerPanel, button, image, header, index);
            else if (type.Equals("analysis")) SetExplorerChildAnalysis(child, button, image, header, index);
        }

        /* Partially renders a child code file element in the explorer tab */
        private void SetExplorerChildCode(XElement child, StackPanel innerPanel, Button button, Image image, TextBlock header, int index)
        {
            TextBlock line = new TextBlock { FontSize = 10, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Light, Foreground = FindResource("TextColor") as SolidColorBrush };

            button.Name = "C" + index;
            button.Click += CodeButton_Click;
            image.Source = new BitmapImage(new Uri("/Assets/Icons/file.png", UriKind.Relative));
            header.Text = child.Attribute("name").Value;
            innerPanel.Children.Add(line);

            if (child.Attribute("type").Value.Equals("txt")) line.Text = "Language: Text";
            else if (child.Attribute("type").Value.Equals("cs")) line.Text = "Language: C#";
            else if (child.Attribute("type").Value.Equals("java")) line.Text = "Language: Java";
        }

        /* Partially renders a child analysis file element in the explorer tab */
        private void SetExplorerChildAnalysis(XElement child, Button button, Image image, TextBlock header, int index)
        {
            button.Name = "A" + index;
            button.Click += AnalysisButton_Click;
            image.Source = new BitmapImage(new Uri("/Assets/Icons/xml-file.png", UriKind.Relative));
            header.Text = char.ToUpper(child.Attribute("type").Value[0]) + child.Attribute("type").Value.Substring(1)
                + Environment.NewLine + "Analysis";
        }

        /* Clears data associated with last clicked button */
        private void ClearLastClickedButton()
        {
            LastClickedButton.button = null;
            LastClickedButton.timestamp = 0;
        }
        
        /* Sets clicked button and renders its clicked image and text color */
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

        /* Resets the last clicked button to render its default image and text color */
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
    }
}

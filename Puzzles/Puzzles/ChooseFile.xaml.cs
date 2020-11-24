using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace Puzzles
{
    /// <summary>
    /// Interaction logic for ChooseFile.xaml
    /// </summary>
    public partial class ChooseFile : Window
    {
        private Window _window;
        private bool _openFile = false;

        public ChooseFile(Window window = null, bool openFile = false)
        {
            this._window = window;

            this._openFile = openFile;

            InitializeComponent();

            this.Loaded += ChooseFileWindow_Load;

            FillDriveNodes();
        }

        private TreeViewItem GetVisualizedComponent(string name, bool isFile)
        {
            StackPanel stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;


            Image icon = new Image();
            TextBlock label = new TextBlock();

            if (!isFile)    //for folders
            {
                icon.Source = new BitmapImage(new Uri("pack://application:,,/Resources/folder_res.png"));
                label.Text = new DirectoryInfo(name).Name;
            }
            else       //for files
            {
                icon.Source = new BitmapImage(new Uri("pack://application:,,/Resources/file_res.png"));
                label.Text = name.Remove(0, name.LastIndexOf('\\') + 1);
            }

            stackPanel.Children.Add(icon);
            stackPanel.Children.Add(label);

            return new TreeViewItem { Header = stackPanel };
        }

        private void FillDriveNodes()
        {
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                TreeViewItem driveNode = GetVisualizedComponent(drive.Name, false);       //Повертає імена всіх лог дисків на пристрої

                FillTreeNode(driveNode, drive.Name);
                driveNode.Expanded += TreeView_Expanded;

                treeView.Items.Add(driveNode);
            }
        }

        private void FillTreeNode(TreeViewItem driveNode, string path)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);     //Повертає імена підкаталогів у даному каталозі
                foreach (string dir in dirs)
                {
                    TreeViewItem dirNode = new TreeViewItem();
                    dirNode.Header = dir.Remove(0, dir.LastIndexOf("\\") + 1);

                    driveNode.Items.Add(dirNode);
                }
            }
            catch(System.UnauthorizedAccessException ex){ }
        }

        private string GetHeaderText(TreeViewItem node)
        {
            return ((TextBlock)((StackPanel)node.Header).Children[1]).Text;
        }

        public string GetFullPath(TreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException();

            var result = GetHeaderText(node);

            for (var i = GetParentItem(node); i != null; i = GetParentItem(i))
                result = GetHeaderText(i) + "\\" + result;

            return result;
        }

        // To get the parent of the item
        static TreeViewItem GetParentItem(TreeViewItem item)
        {
            for (var i = VisualTreeHelper.GetParent(item); i != null; i = VisualTreeHelper.GetParent(i))
                if (i is TreeViewItem)
                    return (TreeViewItem)i;

            return null;
        }

        private void TreeView_Expanded(object sender, RoutedEventArgs e)
        {
            var args = e.OriginalSource as TreeViewItem;
            args.Items.Clear();

            string[] dirs;

            if (Directory.Exists(GetFullPath(args)))
            {
                dirs = Directory.GetDirectories(GetFullPath(args));
                if (dirs.Length != 0)
                {
                    foreach (var dir in dirs)
                    {
                        TreeViewItem dirItem = GetVisualizedComponent(dir, false);

                        FillTreeNode(dirItem, dir);
                        dirItem.Expanded += TreeView_Expanded;
                        args.Items.Add(dirItem);
                    }
                }

                if (_openFile == true)
                {
                    string[] files = Directory.GetFiles(GetFullPath(args));
                    foreach (var file in files)
                    {
                        TreeViewItem fileItem = GetVisualizedComponent(file, true);

                        args.Items.Add(fileItem);
                    }
                }
            }
        }

        private void OkButon_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_window is ConfigureSize)
                {
                    if (_openFile == true)
                    {
                        ((ConfigureSize)_window).ShowFile(GetFullPath((treeView.SelectedItem as TreeViewItem)));
                    }
                    else
                    {
                        ((ConfigureSize)_window).ShowCatalog(GetFullPath((treeView.SelectedItem as TreeViewItem)));
                    }
                }
                else if (_window is MainWindow)
                {
                    ((MainWindow)_window).FormatCatalog(GetFullPath((treeView.SelectedItem as TreeViewItem)));
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Невірний тип файлу");
            }
        }

        private void ChooseFileWindow_Load(object sender, RoutedEventArgs e)
        {
            if (!_openFile)
            {
                this.Title = "ChooseFolder";
            }
        }
    }
}
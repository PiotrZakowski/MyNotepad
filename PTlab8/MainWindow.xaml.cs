using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace PTlab8
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public DirectoryInfo selectedRootDirectory;

        public MainWindow()
        {
            InitializeComponent();
        }

        //Akcja przycisku File>Exit 
        private void menuItem_FileExit(object sender, RoutedEventArgs e)
        {
            this.Close();
            return;
        }

        //Akcja przycisku File>Open
        private void menuItem_FileOpen(object sender, RoutedEventArgs e)
        {
            treeView.Items.Clear();

            var dlg = new System.Windows.Forms.FolderBrowserDialog() { Description = "Select directory to open" };
            dlg.ShowDialog();

            if (dlg.SelectedPath.Equals(""))
                return;

            selectedRootDirectory = new DirectoryInfo(dlg.SelectedPath);

            treeView.Items.Add(mapDirectoryElements(selectedRootDirectory));
            return;
        }

        //rekurencyjne tworzenie drzewa TreeView dla zadanego korzenia
        private TreeViewItem mapDirectoryElements(DirectoryInfo mainDirectoryInfo)
        {
            var root = new TreeViewItem
            {
                Header = mainDirectoryInfo.Name,
                Tag = mainDirectoryInfo.FullName,
                ContextMenu = addDirectoryContextMenu()
            };

            foreach (DirectoryInfo directoryInfo in mainDirectoryInfo.GetDirectories())
            {
                var itemDir = mapDirectoryElements(directoryInfo);
                itemDir.Selected += treeViewItem_DirectoryFocus;
                root.Items.Add(itemDir);
                
            }

            foreach (FileInfo fileInfo in mainDirectoryInfo.GetFiles())
            {
                var itemFile = new TreeViewItem
                {
                    Header = fileInfo.Name,
                    Tag = fileInfo.FullName,
                    ContextMenu = addFileContextMenu()
                };
                //itemFile.Selected += treeViewItem_FileFocus; 
                root.Items.Add(itemFile);
            }

            return root;
        }

        //dodanie menu kontekstowego dla pliku
        public ContextMenu addFileContextMenu()
        {
            var menuOpenButton = new System.Windows.Controls.MenuItem();
            menuOpenButton.Header = "Open";
            menuOpenButton.Click += contextMenu_FileOpen;
            
            var menuDeleteButton = new System.Windows.Controls.MenuItem();
            menuDeleteButton.Header = "Delete";
            menuDeleteButton.Click += contextMenu_FileDelete;

            var rightClickMenu = new System.Windows.Controls.ContextMenu();
            rightClickMenu.Items.Add(menuOpenButton);
            rightClickMenu.Items.Add(menuDeleteButton);

            return rightClickMenu;
        }

        //dodanie menu kontekstowego dla folderu
        public ContextMenu addDirectoryContextMenu()
        {
            var menuCreateButton = new System.Windows.Controls.MenuItem();
            menuCreateButton.Header = "Create";
            menuCreateButton.Click += contextMenu_DirectoryCreate;
            
            var menuDeleteButton = new System.Windows.Controls.MenuItem();
            menuDeleteButton.Header = "Delete";
            menuDeleteButton.Click += contextMenu_DirectoryDelete; 

            var rightClickMenu = new System.Windows.Controls.ContextMenu();
            rightClickMenu.Items.Add(menuCreateButton);
            rightClickMenu.Items.Add(menuDeleteButton);

            return rightClickMenu;
        }

        //akcja dla *PPM*>open dla pliku
        private void contextMenu_FileOpen(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            FileInfo selectedFile = new FileInfo(selectedTreeViewItem.Tag.ToString());

            StreamReader sr = selectedFile.OpenText();
            textBlock_FileContent.Text = "";
            while (!sr.EndOfStream)
                textBlock_FileContent.Text += sr.ReadLine() + "\n";

            return;
        }

        //akcja dla *PPM*>delete dla pliku
        private void contextMenu_FileDelete(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            FileInfo selectedFile = new FileInfo(selectedTreeViewItem.Tag.ToString());

            fileDelete(selectedFile);

            TreeViewItem parentTreeViewItem = selectedTreeViewItem.Parent as TreeViewItem;
            parentTreeViewItem.Items.Remove(selectedTreeViewItem);

            return;
        }

        //logika usuwania pliku
        private void fileDelete(FileInfo fileInfo)
        {
            FileAttributes attributes = File.GetAttributes(fileInfo.FullName);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(fileInfo.FullName, attributes & ~FileAttributes.ReadOnly);
            }

            /*
             * Stack:
             *  If your application is the one locking the file, and you are sure you've released the lock, 
             *  then it might still be in GC. 
            
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
             */

            File.Delete(fileInfo.FullName);

            /*
            while (true)
            {
                try
                {
                    File.Delete(fileInfo.FullName);
                    break;
                }
                catch(System.IO.IOException e)
                {
                    System.Console.WriteLine("Nie mozna teraz usunac pliku: {0}", e.ToString());
                }
            }
            */

            return;
        }

        //akcja dla *PPM*>create dla folderu
        private void contextMenu_DirectoryCreate(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            DirectoryInfo selectedDirectory = new DirectoryInfo(selectedTreeViewItem.Tag.ToString());

            FormWindow window = new FormWindow(selectedDirectory, selectedTreeViewItem, this);
            window.Show();

            return;
        }

        //akcja dla *PPM*>delete dla folderu
        private void contextMenu_DirectoryDelete(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            DirectoryInfo selectedDirectory = new DirectoryInfo(selectedTreeViewItem.Tag.ToString());
            Boolean deletingRoot;
            if (selectedDirectory.FullName.Equals(selectedRootDirectory.FullName))
                deletingRoot = true;
            else
                deletingRoot = false;

            directoryDelete(selectedDirectory);

            if (deletingRoot==true)
            {
                this.treeView.Items.Clear();
            }
            if (deletingRoot == false)
            {
                TreeViewItem parentTreeViewItem = selectedTreeViewItem.Parent as TreeViewItem;
                parentTreeViewItem.Items.Remove(selectedTreeViewItem);
            }

            return;
        }

        //logika usuwania rekurencyjnie folderu
        private void directoryDelete(DirectoryInfo mainDirectoryInfo)
        {
            foreach (DirectoryInfo directoryInfo in mainDirectoryInfo.GetDirectories())
            {
                directoryDelete(directoryInfo);
            }

            foreach (FileInfo fileInfo in mainDirectoryInfo.GetFiles())
            {
                fileDelete(fileInfo);
            }

            FileAttributes attributes = File.GetAttributes(mainDirectoryInfo.FullName);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(mainDirectoryInfo.FullName, attributes & ~FileAttributes.ReadOnly);
            }

            Directory.Delete(mainDirectoryInfo.FullName);

            return;
        }

        //akcja dla *LPM* dla pliku
        private void treeViewItem_FileFocus(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            FileInfo selectedFile = new FileInfo(selectedTreeViewItem.Tag.ToString());

            System.Console.WriteLine("Jej, wybrano mnie =3 ~Plik {0}",selectedFile.Name);

            FileAttributes attributes = selectedFile.Attributes;
            textBlock_DOSAttributes.Text = checkDOSAttributes(attributes);
        }

        private void treeViewItem_DirectoryFocus(object sender, RoutedEventArgs e)
        {
            TreeViewItem selectedTreeViewItem = treeView.SelectedItem as TreeViewItem;
            DirectoryInfo selectedDirectory = new DirectoryInfo(selectedTreeViewItem.Tag.ToString());

            System.Console.WriteLine("Jej, wybrano mnie =3 ~Katalog {0}", selectedDirectory.Name);

            FileAttributes attributes = selectedDirectory.Attributes;
            textBlock_DOSAttributes.Text = checkDOSAttributes(attributes);
        }

        private string checkDOSAttributes(FileAttributes attributes)
        {
            // StringBuilder do stworzenia lancucha.
            System.Text.StringBuilder attributesLine = new System.Text.StringBuilder();

            //czy to plik do odczytu
            if ((attributes & FileAttributes.ReadOnly) > 0)
                attributesLine.Append("r");
            else
                attributesLine.Append("-");

            //czy to plik-archiwum
            if ((attributes & FileAttributes.Archive) > 0)
                attributesLine.Append("a");
            else
                attributesLine.Append("-");

            //czy to plik ukryty
            if ((attributes & FileAttributes.Hidden) > 0)
                attributesLine.Append("h");
            else
                attributesLine.Append("-");

            //czy to plik systemowy
            if ((attributes & FileAttributes.System) > 0)
                attributesLine.Append("s");
            else
                attributesLine.Append("-");

            return (attributesLine.ToString());
        }
    }
}

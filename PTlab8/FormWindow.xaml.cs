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
using System.Windows.Shapes;
using System.IO;
using System.Text.RegularExpressions;

namespace PTlab8
{
    /// <summary>
    /// Interaction logic for FormWindow.xaml
    /// </summary>
    public partial class FormWindow : Window
    {
        public DirectoryInfo parentDirectory;
        public TreeViewItem parentTreeViewItem;
        public MainWindow mainWindow;

        public FormWindow(DirectoryInfo directoryInfo, TreeViewItem treeViewItem, MainWindow callingWindow)
        {
            InitializeComponent();
            this.parentDirectory = directoryInfo;
            this.parentTreeViewItem = treeViewItem;
            this.mainWindow = callingWindow;
        }

        private void button_Cancel(object sender, RoutedEventArgs e)
        {
            this.Close();
            return;
        }

        private void button_Ok(object sender, RoutedEventArgs e)
        {
            String elementName = textBox_FileName.Text;
            Boolean? elementIsFile = radioButton_FileType.IsChecked;
            Boolean? elementIsDirectory = radioButton_DirectoryType.IsChecked;
            Boolean? elementIsReadOnly = checkBox_ReadOnly.IsChecked;
            Boolean? elementIsArchive = checkBox_Archive.IsChecked;
            Boolean? elementIsHidden = checkBox_Hidden.IsChecked;
            Boolean? elementIsSystem = checkBox_System.IsChecked;

            if (elementIsFile != true && elementIsDirectory != true)
                return;

            //[a-zA-Z0-9_~-]\.(txt|php|htm)
            if(elementIsFile==true && !Regex.IsMatch(elementName, "[a-zA-Z0-9_~-]{1,8}.(txt|php|htm)"))
            {
                //MessageBox.Show("Invalid new element's name");
                textBlock_ErrorConsole.Text = "Invalid new element's name";
                return;
            }

            String elementPath = parentDirectory.FullName + "\\" + elementName;

            FileStream fileStream=null;
            if (elementIsFile == true)
                fileStream=File.Create(elementPath);
            else if (elementIsDirectory == true)
                Directory.CreateDirectory(elementPath);

            if(elementIsReadOnly==true)
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) | FileAttributes.ReadOnly);
            else
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) & ~FileAttributes.ReadOnly);

            if (elementIsArchive == true)
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) | FileAttributes.Archive);
            else
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) & ~FileAttributes.Archive);

            if (elementIsHidden == true)
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) | FileAttributes.Hidden);
            else
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) & ~FileAttributes.Hidden);

            if (elementIsSystem == true)
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) | FileAttributes.System);
            else
                File.SetAttributes(elementPath, File.GetAttributes(elementPath) & ~FileAttributes.System);

            if(elementIsFile==true)
            {
                var item = new TreeViewItem
                {
                    Header = elementName,
                    Tag = elementPath,
                    ContextMenu = mainWindow.addFileContextMenu()
                };
                parentTreeViewItem.Items.Add(item);
            } 
            else if(elementIsDirectory==true)
            {
                var item = new TreeViewItem
                {
                    Header = elementName,
                    Tag = elementPath,
                    ContextMenu = mainWindow.addDirectoryContextMenu()
                };
                parentTreeViewItem.Items.Add(item);
            }

            if (elementIsFile == true)
                fileStream.Close();

            this.Close();
            return;
        }
    }
}

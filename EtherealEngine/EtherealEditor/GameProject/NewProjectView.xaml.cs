using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace EtherealEditor.GameProject
{
    public partial class NewProjectView : System.Windows.Controls.UserControl
    {
        public NewProjectView()
        {
            InitializeComponent();

            templateListBox.ItemContainerGenerator.StatusChanged += ListBoxItemStatusChanged;
        }

        private void ListBoxItemStatusChanged(object sender, EventArgs e)
        {
            if (templateListBox.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                var item = templateListBox.ItemContainerGenerator.ContainerFromIndex(templateListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            }
        }

        private void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as NewProject;
            var projectPath = vm.CreateProject(templateListBox.SelectedItem as ProjectTemplate);
            bool dialogResult = false;
            var win = Window.GetWindow(this);

            if (!String.IsNullOrEmpty(projectPath))
            {
                dialogResult = true;
                var project = OpenProject.Open(new ProjectData { ProjectName = vm.ProjectName, ProjectPath = projectPath });
                win.DataContext = project;
            }

            win.DialogResult = dialogResult;
            win.Close();
        }

        private void OnBrowse_Button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    PathTextBox.Text = dialog.SelectedPath;
                }
            }
        }
    }
}
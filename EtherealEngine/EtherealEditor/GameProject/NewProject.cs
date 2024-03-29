using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using EtherealEditor.Utilities;

namespace EtherealEditor.GameProject
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string ProjectFile { get; set; }
        [DataMember]
        public List<String> Folder { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }

        public string IconFilePath { get; set; }
        public string ScreenshotFilePath { get; set; }
        public string ProjectFilePath { get; set; }
    }

    class NewProject : ViewModelBase
    {
        private readonly string _templatePath = @"..\..\EtherealEditor\ProjectTemplates";

        private string _projectname = "NewProject";
        
        public string ProjectName
        {
            get => _projectname;

            set
            {
                if(_projectname != value)
                {
                    _projectname = value;
                    validateProjectDetail();
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectpath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\EtherealProjects\";

        public string ProjectPath
        {
            get => _projectpath;

            set
            {
                if (_projectpath != value)
                {
                    _projectpath = value;
                    validateProjectDetail();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private bool _isValid;

        public bool IsValid
        {
            get => _isValid;

            set
            {
                if(_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string _errorMsg;

        public string ErrorMsg
        {
            get => _errorMsg;

            set
            {
                if(_errorMsg != value)
                {
                    _errorMsg = value;
                    OnPropertyChanged(nameof(ErrorMsg));
                }
            }
        }

        private ObservableCollection<ProjectTemplate> _projecttemplates = new ObservableCollection<ProjectTemplate>();
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates
        { get; }

        private bool validateProjectDetail()
        {
            if (ProjectPath.Length <= 0)
            {
                ErrorMsg = "Path cannot be empty";
                IsValid = false;
                return IsValid;
            }
            if (ProjectName.Length <= 0)
            {
                ErrorMsg = "Name cannot be empty";
                IsValid = false;
                return IsValid;
            }

            var path = ProjectPath;
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path += Path.DirectorySeparatorChar;
            }
            path += $@"{ProjectName}\";

            IsValid = false;
            if(string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMsg = "Type in a project name";
            }
            else if(ProjectName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
            {
                ErrorMsg = "Invalid Character(s) used in project name";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMsg = "Select a valid project folder";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMsg = "Invalid Character(s) used in project path";
            }
            else if(Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMsg = "Selected project folder already exist and is not empty";
            }
            else
            {
                IsValid = true;
                ErrorMsg = String.Empty;
            }

            return IsValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            validateProjectDetail();
            if (!IsValid)
                return String.Empty;

            if (!ProjectPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                ProjectPath += Path.DirectorySeparatorChar;
            }
            var path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach(var folder in template.Folder)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }
                var dirInfo = new DirectoryInfo(path + @".ethereal");
                dirInfo.Attributes |= FileAttributes.Hidden;

                File.Copy(template.IconFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, @"Icon.png")));
                File.Copy(template.ScreenshotFilePath, Path.GetFullPath(Path.Combine(dirInfo.FullName, @"Screenshot.png")));

                var projectxml = File.ReadAllText(template.ProjectFilePath);
                projectxml = String.Format(projectxml, ProjectName, ProjectPath);
                var projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectPath, projectxml);

                return path;
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
                return String.Empty;
            }

        }

        public NewProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projecttemplates);
            try
            {
                var templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Any());

                foreach(var file in templateFiles)
                {
                    var template = Serializer.FromFile<ProjectTemplate>(file);
                    template.IconFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconFilePath);
                    template.ScreenshotFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), "Screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotFilePath);
                    template.ProjectFilePath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(file), template.ProjectFile));
                    
                    _projecttemplates.Add(template);
                }
                validateProjectDetail();
            }
            catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}

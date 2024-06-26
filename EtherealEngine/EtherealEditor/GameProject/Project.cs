﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EtherealEditor.Utilities;

namespace EtherealEditor.GameProject
{
    [DataContract(Name = "Game")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".ethereal";

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if(_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }
        public string FullPath => $"{Path}{Name}{Extension}";

        [DataMember(Name = "Scene")]
        private ObservableCollection<Scene> _scenes = new ObservableCollection<Scene>();
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        public void AddScene(string SceneName)
        {
            Debug.Assert(!String.IsNullOrEmpty(SceneName.Trim()));
            _scenes.Add(new Scene(this, SceneName));
        }

        public void RemoveScene(Scene scene)
        {
            _scenes.Remove(scene);
        }

        public static Project Current => Application.Current.MainWindow.DataContext as Project;

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            _scenes.Add(new Scene(this, "Default Scene"));
        }

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if(_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene = _scenes.FirstOrDefault(x => x.IsActive);
        }

        public void Unload()
        {

        }
    }
}
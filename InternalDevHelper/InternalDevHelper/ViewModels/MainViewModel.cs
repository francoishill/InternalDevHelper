using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using InternalDevHelper.Notifications;
using InternalDevHelper.ViewModels.Projects;
using System.Threading;
using System.Threading.Tasks;

namespace InternalDevHelper.ViewModels
{
    public class MainViewModel : ViewModelBase, IMainViewModel
    {
        private readonly TimeSpan LongLoopDelay = TimeSpan.FromSeconds(2);
        private readonly TimeSpan ShortLoopDelay = TimeSpan.FromMilliseconds(500);

        private readonly string m_ConfigYamlFilePath;
        private Config.Config m_Config;

        private ICollection<IDevProject> m_VSCodeDirectories;
        private bool m_HasSelectedVSCodeDirectory;
        private IDevProject m_SelectedVSCodeDirectory;
        private int m_BusyIncrement;
        private bool m_IsBusy;

        public MainViewModel()
        {
            m_ConfigYamlFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "InternalDevHelper",
                "config.yml");

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                InitializeConfig();
            }
            catch (Exception exception)
            {
                PopupNotificationBuilder.New()
                    .WithMessage("Cannot load config, error: {0}", exception.Message)
                    .Topmost()
                    .Show();
                ExitApplication();
            }

            OpenConfig = new RelayCommand(() =>
            {
                OpenConfigFile();
            });

            ExitApp = new RelayCommand(ExitApplication);

            OpenAllDirectoriesInVSCodeCommand = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    var exe = @"C:\Program Files (x86)\Microsoft VS Code\Code.exe";
                    foreach (var projectDirectory in GetFlattenedDirectoriesOfProject(SelectedVSCodeDirectory))
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory);
                        var args = dirToOpen;
                        Process.Start(exe, args);
                        await Task.Delay(ShortLoopDelay);
                    }
                }
                finally
                {
                    DecrementBusy();
                }
            });

            OpenAllDirectoriesInGitkrakenCommand = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    var exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"gitkraken\update.exe");
                    foreach (var projectDirectory in GetFlattenedDirectoriesOfProject(SelectedVSCodeDirectory))
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory);
                        var args = $"--processStart=gitkraken.exe --process-start-args=\"-p {dirToOpen}\"";
                        Process.Start(exe, args);
                        await Task.Delay(LongLoopDelay);
                    }
                }
                finally
                {
                    DecrementBusy();
                }
            });

            OpenInExplorerCommand = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    foreach (var projectDirectory in GetFlattenedDirectoriesOfProject(SelectedVSCodeDirectory))
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory);
                        var args = $"\"{dirToOpen}\"";
                        Process.Start("explorer", args);
                        await Task.Delay(ShortLoopDelay);
                    }
                }
                finally
                {
                    DecrementBusy();
                }
            });
        }

        private void IncrementBusy()
        {
            m_BusyIncrement++;
            IsBusy = m_BusyIncrement > 0;
        }

        private void DecrementBusy()
        {
            m_BusyIncrement--;
            IsBusy = m_BusyIncrement > 0;
        }

        private void OpenConfigFile()
        {
            Process.Start(m_ConfigYamlFilePath);
        }

        private void ExitApplication()
        {
            Environment.Exit(0);
        }

        private void InitializeConfig()
        {
            if (!File.Exists(m_ConfigYamlFilePath))
            {
                Config.Config.Default().Save(m_ConfigYamlFilePath);
            }

            m_Config = Config.Config.Load(m_ConfigYamlFilePath);
            m_VSCodeDirectories = m_Config.ToProjectList().ToList();

            foreach (var project in GetFlattenProjects())
            {
                //TODO: "unsafe" cast
                var tmpProj = (DevProject)project;
                tmpProj.PropertyChanged += (sender, eventArgs) =>
                {
                    if (eventArgs.PropertyName == "IsSelected")
                    {
                        OnSelectedProjectChanged();
                    }
                };
            }
        }

        private void OnSelectedProjectChanged()
        {
            SelectedVSCodeDirectory = GetFlattenProjects().FirstOrDefault(p => p.IsSelected);
        }

        private List<IDevProject> GetFlattenProjects()
        {
            var flattenedList = new List<IDevProject>();
            foreach (var proj in m_VSCodeDirectories)
            {
                AddFlattenProjects(flattenedList, proj);
            }
            return flattenedList;
        }

        private void AddFlattenProjects(List<IDevProject> flattenedList, IDevProject parentProject)
        {
            flattenedList.Add(parentProject);
            foreach (var childProject in parentProject.ChildProjects)
            {
                AddFlattenProjects(flattenedList, childProject);
            }
        }

        private List<string> GetFlattenedDirectoriesOfProject(IDevProject project)
        {
            var flattenedList = new List<IDevProject>();
            AddFlattenProjects(flattenedList, project);
            return flattenedList.SelectMany(p => p.Directories).ToList();
        }

        public ICollection<IDevProject> VSCodeDirectories
        {
            get
            {
                return m_VSCodeDirectories;
            }
        }

        public bool HasSelectedVSCodeDirectory
        {
            get
            {
                return m_HasSelectedVSCodeDirectory;
            }
            set
            {
                if (m_HasSelectedVSCodeDirectory == value) return;
                m_HasSelectedVSCodeDirectory = value;
                RaisePropertyChanged(() => HasSelectedVSCodeDirectory);
            }
        }

        public IDevProject SelectedVSCodeDirectory
        {
            get
            {
                return m_SelectedVSCodeDirectory;
            }
            set
            {
                if (m_SelectedVSCodeDirectory == value) return;
                m_SelectedVSCodeDirectory = value;
                HasSelectedVSCodeDirectory = m_SelectedVSCodeDirectory != null;
                RaisePropertyChanged(() => SelectedVSCodeDirectory);
            }
        }

        public RelayCommand OpenConfig
        {
            get;
            private set;
        }
        public RelayCommand ExitApp
        {
            get;
            private set;
        }

        public RelayCommand OpenAllDirectoriesInVSCodeCommand
        {
            get;
            private set;
        }

        public RelayCommand OpenAllDirectoriesInGitkrakenCommand
        {
            get;
            private set;
        }

        public RelayCommand OpenInExplorerCommand
        {
            get;
            private set;
        }

        public bool IsBusy
        {
            get { return m_IsBusy; }
            private set
            {
                if (m_IsBusy == value) return;
                m_IsBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }
    }
}

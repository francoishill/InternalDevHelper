using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using InternalDevHelper.Notifications;
using InternalDevHelper.ViewModels.Projects;

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
        private string m_Base64Input;
        private string m_Base64Output;
        private int m_RandomStringLength;
        private string m_SelectedRandomStringLetterChoice;
        private string m_RandomStringOutput;

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

            OpenAllDirectoriesInTortoiseGitCommand = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    var exe = @"C:\Program Files\TortoiseGit\bin\TortoiseGitProc.exe";
                    foreach (var projectDirectory in GetFlattenedDirectoriesOfProject(SelectedVSCodeDirectory))
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory);
                        var args = $"/command:sync /path:\"{dirToOpen}\"";
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

            OpenInConEmu = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    var exe = @"C:\Program Files\ConEmu\ConEmu64.exe";
                    foreach (var projectDirectory in GetFlattenedDirectoriesOfProject(SelectedVSCodeDirectory))
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory);
                        var args = $"-here -dir \"{dirToOpen}\" -run {{cmd}} -cur_console:n";
                        Process.Start(exe, args);
                        await Task.Delay(ShortLoopDelay);
                    }
                }
                finally
                {
                    DecrementBusy();
                }
            });

            RandomStringLetterChoices = new ReadOnlyCollection<string>(new List<string>
            {
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456890",
            });
            SelectedRandomStringLetterChoice = RandomStringLetterChoices.First();
            RandomStringLength = 10;
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
            m_VSCodeDirectories = GetProjectListFromConfig(m_Config);

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

        private static List<IDevProject> GetProjectListFromConfig(Config.Config config)
        {
            var tmp = config.ToProjectList();
            if (config.SortProjects == Config.ProjectSorting.Alphabetically)
            {
                tmp = tmp.OrderBy(p => p.DisplayName);
            }
            return tmp.ToList();
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

        public RelayCommand OpenAllDirectoriesInTortoiseGitCommand
        {
            get;
            private set;
        }

        public RelayCommand OpenInExplorerCommand
        {
            get;
            private set;
        }

        public RelayCommand OpenInConEmu
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

        public string Base64Input
        {
            get
            {
                return m_Base64Input;
            }
            set
            {
                if (m_Base64Input == value) return;
                m_Base64Input = value;
                RaisePropertyChanged(() => Base64Input);
                GenerateBase64Output();
            }
        }

        public string Base64Output
        {
            get
            {
                return m_Base64Output;
            }
            set
            {
                if (m_Base64Output == value) return;
                m_Base64Output = value;
                RaisePropertyChanged(() => Base64Output);
            }
        }

        private void GenerateBase64Output()
        {
            Base64Output = Base64Encode(Base64Input);
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public ICollection<string> RandomStringLetterChoices
        {
            get;
            private set;
        }

        public string SelectedRandomStringLetterChoice
        {
            get
            {
                return m_SelectedRandomStringLetterChoice;
            }
            set
            {
                if (m_SelectedRandomStringLetterChoice == value) return;
                m_SelectedRandomStringLetterChoice = value;
                RaisePropertyChanged(() => SelectedRandomStringLetterChoice);
                GenerateRandomString();
            }
        }

        public int RandomStringLength
        {
            get
            {
                return m_RandomStringLength;
            }
            set
            {
                if (m_RandomStringLength == value) return;
                m_RandomStringLength = value;
                RaisePropertyChanged(() => RandomStringLength);
                GenerateRandomString();
            }
        }

        public string RandomStringOutput
        {
            get
            {
                return m_RandomStringOutput;
            }
            set
            {
                if (m_RandomStringOutput == value) return;
                m_RandomStringOutput = value;
                RaisePropertyChanged(() => RandomStringOutput);
            }
        }

        private void GenerateRandomString()
        {
            RandomStringOutput = RandomString();
        }

        private static Random random = new Random();
        private string RandomString()
        {
            return new string(Enumerable.Repeat(SelectedRandomStringLetterChoice, RandomStringLength)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}

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
        private readonly TimeSpan LoopDelay = TimeSpan.FromSeconds(2);

        private readonly string m_ConfigYamlFilePath;
        private Config.Config m_Config;

        private ICollection<IDevProject> m_VSCodeDirectories;
        private bool m_HasSelectedVSCodeDirectory;
        private DevProject m_SelectedVSCodeDirectory;
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
                    foreach (var projectDirectory in SelectedVSCodeDirectory.Directories)
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory.Directory);
                        var args = dirToOpen;
                        Process.Start(exe, args);
                        await Task.Delay(LoopDelay);
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
                    foreach (var projectDirectory in SelectedVSCodeDirectory.Directories)
                    {
                        var dirToOpen = Environment.ExpandEnvironmentVariables(projectDirectory.Directory);
                        var args = $"--processStart=gitkraken.exe --process-start-args=\"-p {dirToOpen}\"";
                        Process.Start(exe, args);
                        await Task.Delay(LoopDelay);
                    }
                }
                finally
                {
                    DecrementBusy();
                }
            });

            SignDroneCIYamlForAllDirectoriesCommand = new RelayCommand(async delegate
            {
                IncrementBusy();
                try
                {
                    var exe = "drone";

                    var tasks = SelectedVSCodeDirectory.Directories.Select(async projectDirectory =>
                    {
                        var dir = Environment.ExpandEnvironmentVariables(projectDirectory.Directory).Replace("/", "\\").TrimEnd('\\');
                        var prefixToRemove = Path.Combine(Environment.ExpandEnvironmentVariables("%GOPATH%"), @"src\gogs.firepuma.com");
                        if (!dir.StartsWith(prefixToRemove))
                        {
                            PopupNotificationBuilder.New()
                                .WithMessage("Directory '{0}' does not start with expected prefix '{1}'", dir, prefixToRemove)
                                .Topmost()
                                .Show();
                            return;
                        }

                        var droneProjectRelativeURL = dir
                            .Substring(prefixToRemove.Length)
                            .TrimStart('\\')
                            .Replace("\\", "/");

                        var startInfo = new ProcessStartInfo(exe, $"sign {droneProjectRelativeURL}")
                        {
                            WorkingDirectory = dir,
                        };
                        var runner = new Utils.ProcessUtils.Runner(startInfo);
                        var result = await Task.Run(() => runner.RunAndWait());
                        if (!result.Success(true))
                        {
                            PopupNotificationBuilder.New()
                                .WithMessage("Unable to sign drone yaml for dir '{0}', error:\n\n{1}", dir, result.GetDisplayError())
                                .Topmost()
                                .Show();
                            return;
                        }
                    });
                    await Task.WhenAll(tasks);
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

        public DevProject SelectedVSCodeDirectory
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

        public RelayCommand SignDroneCIYamlForAllDirectoriesCommand
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

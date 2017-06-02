using System.Collections.Generic;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using InternalDevHelper.ViewModels.Projects;

namespace InternalDevHelper.ViewModels
{
    public interface IMainViewModel
    {
        void HandleKeyPressed(Key key, ModifierKeys modifiers);

        ICollection<IDevProject> VSCodeDirectories
        {
            get;
        }

        bool HasSelectedVSCodeDirectory
        {
            get;
        }

        IDevProject SelectedVSCodeDirectory
        {
            get;
            set;
        }

        RelayCommand OpenConfig
        {
            get;
        }

        RelayCommand ExitApp
        {
            get;
        }

        RelayCommand OpenAllDirectoriesInVSCodeCommand
        {
            get;
        }

        RelayCommand OpenAllDirectoriesInGitkrakenCommand
        {
            get;
        }

        RelayCommand OpenAllDirectoriesInTortoiseGitCommand
        {
            get;
        }

        RelayCommand OpenInExplorerCommand
        {
            get;
        }

        RelayCommand OpenInConEmu
        {
            get;
        }

        string Base64Input
        {
            get;
            set;
        }

        string Base64Output
        {
            get;
        }

        ICollection<string> RandomStringLetterChoices
        {
            get;
        }

        string SelectedRandomStringLetterChoice
        {
            get;
            set;
        }

        int RandomStringLength
        {
            get;
            set;
        }

        string RandomStringOutput
        {
            get;
        }
    }
}
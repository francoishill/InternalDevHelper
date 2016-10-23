using System.Collections.Generic;
using GalaSoft.MvvmLight.Command;
using InternalDevHelper.ViewModels.Projects;

namespace InternalDevHelper.ViewModels
{
    public interface IMainViewModel
    {
        ICollection<IDevProject> VSCodeDirectories
        {
            get;
        }

        bool HasSelectedVSCodeDirectory
        {
            get;
        }

        DevProject SelectedVSCodeDirectory
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
    }
}
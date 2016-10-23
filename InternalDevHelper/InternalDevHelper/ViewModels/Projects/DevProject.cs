using System.Collections.Generic;
using GalaSoft.MvvmLight;
using InternalDevHelper.ViewModels.Projects.DevProjects;

namespace InternalDevHelper.ViewModels.Projects
{
    public class DevProject : ViewModelBase, IDevProject
    {
        public DevProject(string displayName, ICollection<IProjectDirectory> directories)
        {
            DisplayName = displayName;
            Directories = directories;
        }

        public string DisplayName
        {
            get;
        }

        public ICollection<IProjectDirectory> Directories
        {
            get;
        }
    }
}

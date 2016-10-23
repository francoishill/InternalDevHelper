using GalaSoft.MvvmLight;

namespace InternalDevHelper.ViewModels.Projects.DevProjects
{
    public class ProjectDirectory : ViewModelBase, IProjectDirectory
    {
        public ProjectDirectory(string displayName, string directory)
        {
            DisplayName = displayName;
            Directory = directory;
        }

        public string DisplayName
        {
            get;
        }

        public string Directory
        {
            get;
        }
    }
}

using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace InternalDevHelper.ViewModels.Projects
{
    public class DevProject : ViewModelBase, IDevProject
    {
        private bool m_IsSelected;

        public DevProject(string displayName, ICollection<string> directories)
        {
            DisplayName = displayName;
            Directories = directories;
            ChildProjects = new List<IDevProject>();
        }

        public void AddChildProject(IDevProject childProject)
        {
            ChildProjects.Add(childProject);
        }

        public bool IsSelected
        {
            get
            {
                return m_IsSelected;
            }
            set
            {
                if (m_IsSelected == value) return;
                m_IsSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        public string DisplayName
        {
            get;
        }

        public ICollection<string> Directories
        {
            get;
        }

        public ICollection<IDevProject> ChildProjects
        {
            get;
        }
    }
}

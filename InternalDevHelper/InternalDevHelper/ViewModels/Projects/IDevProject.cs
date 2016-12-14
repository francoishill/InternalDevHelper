using System.Collections.Generic;

namespace InternalDevHelper.ViewModels.Projects
{
    public interface IDevProject
    {
        bool IsSelected
        {
            get;
            set;
        }

        string DisplayName
        {
            get;
        }

        ICollection<string> Directories
        {
            get;
        }

        ICollection<IDevProject> ChildProjects
        {
            get;
        }

        void AddChildProject(IDevProject childProject);
    }
}
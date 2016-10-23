using System.Collections.Generic;
using InternalDevHelper.ViewModels.Projects.DevProjects;

namespace InternalDevHelper.ViewModels.Projects
{
    public interface IDevProject
    {
        string DisplayName
        {
            get;
        }

        ICollection<IProjectDirectory> Directories
        {
            get;
        }
    }
}
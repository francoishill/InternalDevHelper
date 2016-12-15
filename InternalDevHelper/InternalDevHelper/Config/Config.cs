using System.Collections.Generic;
using System.IO;
using System.Linq;
using InternalDevHelper.ViewModels.Projects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace InternalDevHelper.Config
{
    public class Config
    {
        public ProjectSorting SortProjects
        {
            get;
            set;
        }

        public ConfigDevProject[] DevProjects
        {
            get;
            set;
        }

        public static Config Default()
        {
            return new Config {DevProjects = new ConfigDevProject[0]};
        }

        private static INamingConvention YamlNamingConvention()
        {
            return new UnderscoredNamingConvention();
        }

        public static Config Load(string yamlFilePath)
        {
            var fileContent = File.ReadAllText(yamlFilePath);

            var deserializer = new DeserializerBuilder().WithNamingConvention(YamlNamingConvention()).Build();
            return deserializer.Deserialize<Config>(fileContent);
        }

        public void Save(string yamlFilePath)
        {
            var serializer = new SerializerBuilder().WithNamingConvention(YamlNamingConvention()).Build();
            using (var sw = new StreamWriter(yamlFilePath))
            {
                serializer.Serialize(sw, this);
            }
        }

        public IEnumerable<IDevProject> ToProjectList()
        {
            return DevProjects.Select(ConvertProject).ToArray();
        }

        private static IDevProject ConvertProject(ConfigDevProject project)
        {
            var currentDevProject = NewDevProjectFromConfig(project);
            if (project.HasChildProjects())
            {
                AppendDevProjectChildren(currentDevProject, project.ChildProjects);
            }
            return currentDevProject;
        }

        private static void AppendDevProjectChildren(IDevProject parentProject, ConfigDevProject[] childProjects)
        {
            foreach (var childProject in childProjects)
            {
                var childDevProject = NewDevProjectFromConfig(childProject);
                parentProject.AddChildProject(childDevProject);
                if (childProject.HasChildProjects())
                {
                    AppendDevProjectChildren(childDevProject, childProject.ChildProjects);
                }
            }
        }

        private static DevProject NewDevProjectFromConfig(ConfigDevProject project)
        {
            return new DevProject(project.Name, project.Directories != null ? project.Directories.ToList() : new List<string>());
        }

        public class ConfigDevProject
        {
            public string Name
            {
                get;
                set;
            }

            public string[] Directories
            {
                get;
                set;
            }

            public ConfigDevProject[] ChildProjects
            {
                get;
                set;
            }

            public bool HasChildProjects()
            {
                return ChildProjects != null && ChildProjects.Length > 0;
            }
        }
    }
}
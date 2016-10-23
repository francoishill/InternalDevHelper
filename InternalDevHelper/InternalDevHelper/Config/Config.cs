using System.Collections.Generic;
using System.IO;
using System.Linq;
using InternalDevHelper.ViewModels.Projects;
using InternalDevHelper.ViewModels.Projects.DevProjects;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace InternalDevHelper.Config
{
    public class Config
    {
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
            return new DevProject(project.Name, project.Directories.Select(ConvertDirectory).ToList());
        }

        private static IProjectDirectory ConvertDirectory(ConfigDevProject.ProjectDirectory directory)
        {
            return new ProjectDirectory(directory.Name, directory.Path);
        }

        public class ConfigDevProject
        {
            public string Name
            {
                get;
                set;
            }

            public ProjectDirectory[] Directories
            {
                get;
                set;
            }

            public class ProjectDirectory
            {
                public string Name
                {
                    get;
                    set;
                }

                public string Path
                {
                    get;
                    set;
                }
            }
        }
    }
}
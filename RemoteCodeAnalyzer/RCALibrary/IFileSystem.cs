using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCALibrary
{
    public interface IFileSystemItem { }

    [Serializable]
    public class UserDirectory : IFileSystemItem
    {
        public string Name { get; private set; } // user name
        public string Date { get; private set; } // joined date
        public int Projects { get; private set; } // number of projects
        public UserDirectory(string name, string date, int projects)
        {
            Name = name;
            Date = date;
            Projects = projects;
        }
    }

    [Serializable]
    public class ProjectDirectory : IFileSystemItem
    {
        public string Name { get; private set; } // project name
        public string Author { get; private set; } // username
        public string Created { get; private set; } // date created
        public string Edited { get; private set; } // last upload date
        public ProjectDirectory(string name, string author, string created, string edited)
        {
            Name = name;
            Author = author;
            Created = created;
            Edited = edited;
        }
    }

    [Serializable]
    public class VersionDirectory : IFileSystemItem
    {
        public string Name { get; private set; } // project name
        public int Number { get; private set; } // version number
        public string Author { get; private set; } // user name
        public string Date { get; private set; } // upload date
        public VersionDirectory(string name, int number, string author, string date)
        {
            Name = name;
            Number = number;
            Author = author;
            Date = date;
        }
    }

    [Serializable]
    public class CodeFile : IFileSystemItem
    {
        public string Name { get; private set; } // file name
        public string FType { get; private set; } // programming language
        public string Project { get; private set; } // project name
        public int Version { get; private set; } // version number
        public string Author { get; private set; } // user name
        public DateTime Date { get; private set; } // upload date
        public CodeFile(string name, string ftype, string project, int version, string author, DateTime date)
        {
            Name = name;
            FType = ftype;
            Project = project;
            Version = version;
            Author = author;
            Date = date;
        }
    }

    [Serializable]
    public class AnalysisFile : IFileSystemItem
    {
        public string FType { get; private set; } // function or relationship
        public string Project { get; private set; } // project name
        public int Version { get; private set; } // version number
        public string Author { get; private set; } // user name
        public DateTime Date { get; private set; } // upload date
        public AnalysisFile(string ftype, string project, int version, string author, DateTime date)
        {
            FType = ftype;
            Project = project;
            Version = version;
            Author = author;
            Date = date;
        }
    }
}

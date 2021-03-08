using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCALibrary
{
    public interface IFileSystemItem { }

    public abstract class SystemDirectory : IFileSystemItem
    {
        public string Name { get; private set; }
        public DateTime Date { get; private set; }
        public SystemDirectory(string name, DateTime date)
        {
            Name = name;
            Date = date;
        }
    }

    public abstract class SubDirectory : SystemDirectory
    {
        public string Author { get; private set; }
        public SubDirectory(string name, DateTime date, string author)
            : base(name, date) => Author = author;
    }

    public abstract class SystemFile : IFileSystemItem
    {
        public string FType { get; private set; }
        public string Project { get; private set; }
        public int Version { get; private set; }
        public string Author { get; private set; }
        public DateTime Date { get; private set; }
        public SystemFile(string fType, string project, int version, string author, DateTime date)
        {
            FType = fType;
            Project = project;
            Version = version;
            Author = author;
            Date = date;
        }
    }

    [Serializable]
    public class RootDirectory : IFileSystemItem { }

    [Serializable]
    public class UserDirectory : SystemDirectory
    {
        // Name = user name, Date = joined date
        public int Projects { get; private set; } // number of projects
        public UserDirectory(string name, DateTime date, int projects)
            : base(name, date) => Projects = projects;
    }

    [Serializable]
    public class ProjectDirectory : SubDirectory
    {
        // Name = project name, Date = date created, Author = username
        public DateTime LastEdit { get; private set; } // last upload date
        public ProjectDirectory(string name, string author, DateTime created, DateTime lastEdit)
            : base(name, created, author) => LastEdit = lastEdit;
    }

    [Serializable]
    public class VersionDirectory : SubDirectory
    {
        // Name = project name, Date = upload date, Author = username
        public int Number { get; private set; } // version number
        public VersionDirectory(string name, int number, string author, DateTime date)
            : base(name, date, author) => Number = number;
    }

    [Serializable]
    public class CodeFile : SystemFile
    {
        // FType = programming language, Project = project name, Version = version number, Author = username, Date = upload date
        public string Name { get; private set; } // file name
        public CodeFile(string name, string fType, string project, int version, string author, DateTime date)
            : base(fType, project, version, author, date) => Name = name;
    }

    [Serializable]
    public class AnalysisFile : SystemFile
    {
        // FType = function or relationship, Project = project name, Version = version number, Author = username, Date = upload date
        public AnalysisFile(string fType, string project, int version, string author, DateTime date)
            : base(fType, project, version, author, date) { }
    }
}

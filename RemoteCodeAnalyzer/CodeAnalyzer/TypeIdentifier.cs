/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  TypeIdentifier.cs - Defines C# types, stores data within types                   ///
///                                                                                   ///
///  Language:      C# .Net Framework                                                 ///
///  Platform:      Dell G5 5090, Intel Core i7, 16GB RAM, Windows 10                 ///
///  Application:   RemoteCodeAnalyzer - Project #4 for                               ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeAnalyzer
{
    /* Defines core information for all relevant program types: files, namespaces, classes, interfaces, and functions */
    public abstract class ProgramType
    {
        public virtual string Name { get; set; }
        public int Line { get; protected set; }
        public string DirectoryPath { get; protected set; }
        public ProgramType Parent { get; protected set; }
        public List<ProgramType> ChildList { get; }

        public ProgramType() => ChildList = new List<ProgramType>();

        public ProgramType(ProgramType parent, string name, int line)
        {
            Name = name;
            Line = line;
            ChildList = new List<ProgramType>();

            if (parent != null)
            {
                Parent = parent;
                parent.ChildList.Add(this);

                string directoryName = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidPathChars())))).Replace(parent.Name, "");
                if (directoryName.Contains(".")) directoryName = directoryName.Substring(0, directoryName.LastIndexOf('.'));

                DirectoryPath = parent.DirectoryPath + "\\" + directoryName;
            }
        }
    }

    /* Defines core information for types that hold relevant analysis data: classes, interfaces, and functions */
    public abstract class ProgramDataType : ProgramType
    {
        public List<string> Modifiers { get; }
        public List<string> Generics { get; }
        public ProgramDataType(ProgramType parent, string name, int line, List<string> modifiers, List<string> generics)
            : base(parent, name, line)
        {
            Modifiers = modifiers;
            Generics = generics;
        }
    }

    /* Defines core information for types that "behave" or "look" like classes: classes and interfaces */
    public abstract class ProgramClassType : ProgramDataType
    {
        public ProgramClassTypeCollection ProgramClassTypes { get; internal set; }
        public List<ProgramClassType> SubClasses { get; }       // Inheritance (child data): ProgramType(s) that this type is inherited by
        public List<ProgramClassType> SuperClasses { get; }     // Inheritance (parent data): ProgramType(s) that this type inherits from
        public override string Name // Maintain the ProgramClassTypeCollection if a name changes
        {
            get { return base.Name; }

            set
            {
                if (ProgramClassTypes != null) ProgramClassTypes.NotifyNameChange(this, value);
                base.Name = value;
            }
        }

        public ProgramClassType(ProgramClassTypeCollection programClassTypes, ProgramType parent, string name, int line, List<string> modifiers, List<string> generics) 
            : base(parent, name, line, modifiers, generics)
        {
            ProgramClassTypes = programClassTypes;
            programClassTypes.Add(this);
            SubClasses = new List<ProgramClassType>();
            SuperClasses = new List<ProgramClassType>();
        }

        public override bool Equals(object obj) // Defines equality based on name and type
        {
            return base.Name.Equals(((ProgramClassType)obj).Name) && GetType() == obj.GetType();
        }

        public override int GetHashCode() => // HashCode based on name and type
            Name.GetHashCode() + GetType().GetHashCode();
    }

    /* Defines unique data contained in an object representing a file */
    public class ProgramFile : ProgramType
    {
        public string FileType { get; }
        public ProgramFile(string fileName, string directoryPath) : base()
        {
            Name = fileName;
            DirectoryPath = directoryPath + "\\temp";

            if (fileName.Contains("."))
            {
                Name = fileName.Substring(0, fileName.LastIndexOf('.'));
                FileType = fileName.Substring(fileName.LastIndexOf('.') + 1);
            }
        }
    }

    /* Defines unique data contained in an object representing a namespace */
    public class ProgramNamespace : ProgramType
    {
        public ProgramNamespace(ProgramType parent, string name, int line)
            : base(parent, name, line) { }
    }

    /* Defines unique data contained in an object representing a class */
    public class ProgramClass : ProgramClassType
    {
        public List<ProgramClassType> OwnedClasses { get; }     // Composition/Aggregation (child data): ProgramClass(es) that are owned by ("part of") this class
        public List<ProgramClassType> OwnedByClasses { get; }   // Composition/Aggregation (parent data): ProgramClass(es) that this class is owned by ("part of")
        public List<ProgramClassType> UsedClasses { get; }      // Using (child data): ProgramClass(es) that this class uses
        public List<ProgramClassType> UsedByClasses { get; }    // Using (parent data): ProgramClass(es) that this class is used by
        public ProgramClass(ProgramClassTypeCollection programClassTypes, ProgramType parent, string name, int line, List<string> modifiers, List<string> generics)
            : base(programClassTypes, parent, name, line, modifiers, generics)
        {
            OwnedClasses = new List<ProgramClassType>();
            OwnedByClasses = new List<ProgramClassType>();
            UsedClasses = new List<ProgramClassType>();
            UsedByClasses = new List<ProgramClassType>();
        }
    }

    /* Defines unique data contained in an object representing an interface */
    public class ProgramInterface : ProgramClassType
    {
        public ProgramInterface(ProgramClassTypeCollection programClassTypes, ProgramType parent, string name, int line, List<string> modifiers, List<string> generics)
            : base(programClassTypes, parent, name, line, modifiers, generics) { }
    }

    /* Defines unique data contained in an object representing a function */
    public class ProgramFunction : ProgramDataType
    {
        public List<string> ReturnTypes { get; }
        public List<string> Parameters { get; }
        public List<string> BaseParameters { get; }
        public int Size { get; set; }
        public int Complexity { get; set; }
        public ProgramFunction(ProgramType parent, string name, int line, List<string> modifiers, List<string> returnTypes, List<string> generics, List<string> parameters, List<string> baseParameters)
            : base(parent, name, line, modifiers, generics)
        {
            ReturnTypes = returnTypes;
            Parameters = parameters;
            BaseParameters = baseParameters;
            Size = 0;
            Complexity = 0;
        }
    }
}

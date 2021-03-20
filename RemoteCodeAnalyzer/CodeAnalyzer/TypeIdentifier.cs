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

namespace CodeAnalyzer
{
    /* Defines core information for all relevant program types: files, namespaces, classes, interfaces, and functions */
    public abstract class ProgramType
    {
        public virtual string Name { get; protected set; }
        public virtual string DirectoryPath { get; protected set; }
        public virtual ProgramType Parent { get; protected set; }
        public List<ProgramType> ChildList { get; }
        public ProgramType(ProgramType parent, string name)
        {
            Name = name;
            ChildList = new List<ProgramType>();

            if (parent != null)
            {
                Parent = parent;
                DirectoryPath = parent.DirectoryPath + "\\" + parent.Name;
            }
        }
    }

    /* Defines core information for types that hold relevant analysis data: classes, interfaces, and functions */
    public abstract class ProgramDataType : ProgramType
    {
        public List<string> Modifiers { get; }
        public List<string> Generics { get; }
        public ProgramDataType(ProgramType parent, string name, List<string> modifiers, List<string> generics) : base(parent, name)
        {
            Modifiers = modifiers;
            Generics = generics;
        }
    }

    /* Defines core information for types that "behave" or "look" like classes: classes and interfaces */
    public abstract class ProgramClassType : ProgramDataType
    {
        public ProgramClassTypeCollection ProgramClassCollection { get; internal set; }
        public List<ProgramClassType> SubClasses { get; }       // Inheritance (child data): ProgramType(s) that this type is inherited by
        public List<ProgramClassType> SuperClasses { get; }     // Inheritance (parent data): ProgramType(s) that this type inherits from
        public override string Name // Maintain the ProgramClassTypeCollection if a name changes
        {
            get { return base.Name; }
            protected set
            {
                if (ProgramClassCollection != null) ProgramClassCollection.NotifyNameChange(this, value);
                base.Name = value;
            }
        }

        public ProgramClassType(ProgramType parent, string name, List<string> modifiers, List<string> generics) 
            : base(parent, name, modifiers, generics)
        {
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
        public ProgramFile(string fileName, string directoryPath) : base(null, fileName)
        {
            FileType = fileName.Substring(fileName.LastIndexOf('.') + 1);
            DirectoryPath = directoryPath;
        }
    }

    /* Defines unique data contained in an object representing a namespace */
    public class ProgramNamespace : ProgramType
    {
        public ProgramNamespace(ProgramType parent, string name)
            : base(parent, name) { }
    }

    /* Defines unique data contained in an object representing a class */
    public class ProgramClass : ProgramClassType
    {
        public List<ProgramClassType> OwnedClasses { get; }     // Composition/Aggregation (child data): ProgramClass(es) that are owned by ("part of") this class
        public List<ProgramClassType> OwnedByClasses { get; }   // Composition/Aggregation (parent data): ProgramClass(es) that this class is owned by ("part of")
        public List<ProgramClassType> UsedClasses { get; }      // Using (child data): ProgramClass(es) that this class uses
        public List<ProgramClassType> UsedByClasses { get; }    // Using (parent data): ProgramClass(es) that this class is used by
        public ProgramClass(ProgramType parent, string name, List<string> modifiers, List<string> generics)
            : base(parent, name, modifiers, generics)
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
        public ProgramInterface(ProgramType parent, string name, List<string> modifiers, List<string> generics)
            : base(parent, name, modifiers, generics) { }
    }

    /* Defines unique data contained in an object representing a function */
    public class ProgramFunction : ProgramDataType
    {
        public List<string> ReturnTypes { get; }
        public List<string> Parameters { get; }
        public List<string> BaseParameters { get; }
        public int Size { get; set; }
        public int Complexity { get; set; }
        public ProgramFunction(ProgramType parent, string name, List<string> modifiers, List<string> returnTypes, List<string> generics, List<string> parameters, List<string> baseParameters)
            : base(parent, name, modifiers, generics)
        {
            ReturnTypes = returnTypes;
            Parameters = parameters;
            BaseParameters = baseParameters;
            Size = 0;
            Complexity = 0;
        }
    }
}

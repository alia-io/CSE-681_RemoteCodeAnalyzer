///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  ProgramTypes.cs - Defines programming types, stores data associated with types     ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module contains definitions for all relevant types typically associated with an
 *   application: file, namespace (C#), class, interface, and function. The module establishes
 *   an inheritance hierarchy for all defined types, and controls the data that should be
 *   stored within each type. Types are designed to exist in a tree-like structure, which
 *   mimics an actual program's structure within each file.
 * 
 *   Public Interface
 *   ----------------
 *   
 *   ProgramType
 *   -----------
 *   ProgramType programType = new ProgramFile((string) fileName, (string) directoryPath);
 *   ProgramType programType = new ProgramNamespace((ProgramType) parent, (string) name, (int) line);
 *   ProgramType programType = new ProgramClass((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   ProgramType programType = new ProgramInterface((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   ProgramType programType = new ProgramFunction((ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) returnTypes, (List<string>) generics, (List<string>) parameters, (List<string>) baseParameters);
 *   programType.Name = (string) name;
 *   string name = programType.Name;
 *   int line = programType.Line;
 *   string directoryPath = programType.DirectoryPath;
 *   ProgramType parent = programType.Parent;
 *   List<ProgramType> childList = programType.ChildList;
 *   
 *   ProgramDataType
 *   ---------------
 *   ProgramDataType programDataType = new ProgramClass((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   ProgramDataType programDataType = new ProgramInterface((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   ProgramDataType programDataType = new ProgramFunction((ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) returnTypes, (List<string>) generics, (List<string>) parameters, (List<string>) baseParameters);
 *   programDataType.Name = (string) name;
 *   string name = programDataType.Name;
 *   int line = programDataType.Line;
 *   string directoryPath = programDataType.DirectoryPath;
 *   ProgramType parent = programDataType.Parent;
 *   List<ProgramType> childList = programDataType.ChildList;
 *   List<string> modifiers = programDataType.Modifiers;
 *   List<string> generics = programDataType.Generics;
 *   
 *   ProgramClassType
 *   ----------------
 *   ProgramClassType programClassType = new ProgramClass((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   ProgramClassType programClassType = new ProgramInterface((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   programClassType.Name = (string) name;
 *   string name = programClassType.Name;
 *   int line = programClassType.Line;
 *   string directoryPath = programClassType.DirectoryPath;
 *   ProgramType parent = programClassType.Parent;
 *   List<ProgramType> childList = programClassType.ChildList;
 *   List<string> modifiers = programClassType.Modifiers;
 *   List<string> generics = programClassType.Generics;
 *   ProgramClassTypeCollection programClassTypes = programClassType.ProgramClassTypes;
 *   List<ProgramClassType> subClasses = programClassType.SubClasses;
 *   List<ProgramClassType> superClasses = programClassType.SuperClasses;
 *   
 *   ProgramFile
 *   -----------
 *   ProgramFile programFile = new ProgramFile((string) fileName, (string) directoryPath);
 *   programFile.Name = (string) name;
 *   string name = programFile.Name;
 *   int line = programFile.Line;
 *   string directoryPath = programFile.DirectoryPath;
 *   ProgramType parent = programFile.Parent;
 *   List<ProgramType> childList = programFile.ChildList;
 *   string fileType = programFile.FileType;
 *   
 *   ProgramNamespace
 *   ----------------
 *   ProgramType programNamespace = new ProgramNamespace((ProgramType) parent, (string) name, (int) line);
 *   programNamespace.Name = (string) name;
 *   string name = programNamespace.Name;
 *   int line = programNamespace.Line;
 *   string directoryPath = programNamespace.DirectoryPath;
 *   ProgramType parent = programNamespace.Parent;
 *   List<ProgramType> childList = programNamespace.ChildList;
 *   
 *   ProgramClass
 *   ------------
 *   ProgramClass programClass = new ProgramClass((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   programClass.Name = (string) name;
 *   string name = programClass.Name;
 *   int line = programClass.Line;
 *   string directoryPath = programClass.DirectoryPath;
 *   ProgramType parent = programClass.Parent;
 *   List<ProgramType> childList = programClass.ChildList;
 *   List<string> modifiers = programClass.Modifiers;
 *   List<string> generics = programClass.Generics;
 *   ProgramClassTypeCollection programClassTypes = programClass.ProgramClassTypes;
 *   List<ProgramClassType> subClasses = programClass.SubClasses;
 *   List<ProgramClassType> superClasses = programClass.SuperClasses;
 *   List<ProgramClassType> ownedClasses = programClass.OwnedClasses;
 *   List<ProgramClassType> ownedByClasses = programClass.OwnedByClasses;
 *   List<ProgramClassType> usedClasses = programClass.UsedClasses;
 *   List<ProgramClassType> usedByClasses = programClass.UsedByClasses;
 *   
 *   ProgramInterface
 *   ----------------
 *   ProgramInterface programInterface = new ProgramInterface((ProgramClassTypeCollection) programClassTypes, (ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) generics);
 *   programInterface.Name = (string) name;
 *   string name = programInterface.Name;
 *   int line = programInterface.Line;
 *   string directoryPath = programInterface.DirectoryPath;
 *   ProgramType parent = programInterface.Parent;
 *   List<ProgramType> childList = programInterface.ChildList;
 *   List<string> modifiers = programInterface.Modifiers;
 *   List<string> generics = programInterface.Generics;
 *   ProgramClassTypeCollection programClassTypes = programInterface.ProgramClassTypes;
 *   List<ProgramClassType> subClasses = programInterface.SubClasses;
 *   List<ProgramClassType> superClasses = programInterface.SuperClasses;
 *   
 *   ProgramFunction
 *   ---------------
 *   ProgramFunction programFunction = new ProgramFunction((ProgramType) parent, (string) name, (int) line, (List<string>) modifiers, (List<string>) returnTypes, (List<string>) generics, (List<string>) parameters, (List<string>) baseParameters);
 *   programFunction.Name = (string) name;
 *   programFunction.Size = (int) size;
 *   programFunction.Complexity = (int) complexity;
 *   string name = programFunction.Name;
 *   int line = programFunction.Line;
 *   string directoryPath = programFunction.DirectoryPath;
 *   ProgramType parent = programFunction.Parent;
 *   List<ProgramType> childList = programFunction.ChildList;
 *   List<string> modifiers = programFunction.Modifiers;
 *   List<string> generics = programFunction.Generics;
 *   List<string> returnTypes = programFunction.ReturnTypes;
 *   List<string> parameters = programFunction.Parameters;
 *   List<string> baseParameters = programFunction.BaseParameters;
 *   int size = programFunction.Size;
 *   int complexity = programFunction.Complexity;
 */

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
        public List<ProgramType> ChildList { get; private set; }

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
        public List<string> Modifiers { get; private set; }
        public List<string> Generics { get; private set; }
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
        public string FileType { get; private set; }
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
        public List<ProgramClassType> OwnedClasses { get; private set; }     // Composition/Aggregation (child data): ProgramClass(es) that are owned by ("part of") this class
        public List<ProgramClassType> OwnedByClasses { get; private set; }   // Composition/Aggregation (parent data): ProgramClass(es) that this class is owned by ("part of")
        public List<ProgramClassType> UsedClasses { get; private set; }      // Using (child data): ProgramClass(es) that this class uses
        public List<ProgramClassType> UsedByClasses { get; private set; }    // Using (parent data): ProgramClass(es) that this class is used by
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
        public List<string> ReturnTypes { get; private set; }
        public List<string> Parameters { get; private set; }
        public List<string> BaseParameters { get; private set; }
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

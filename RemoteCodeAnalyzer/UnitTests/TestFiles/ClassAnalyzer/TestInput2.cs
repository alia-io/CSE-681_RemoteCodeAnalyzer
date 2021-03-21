/////////////////////////////////////////////////////////////////////////////////////////
///                                                                                   ///
///  TypeIdentifier.cs - Defines C# types, stores data within types                   ///
///                                                                                   ///
///  Language:      C#                                                                ///
///  Platform:      Dell G5 5090, Windows 10                                          ///
///  Application:   CodeAnalyzer - Project #2 for                                     ///
///                 CSE 681: Software Modeling and Analysis                           ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                  ///
///                                                                                   ///
/////////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CodeAnalyzer
{
    /* Defines core information for all relevant program types: files, namespaces, classes, interfaces, and functions */
    public abstract class ProgramType
    {
        public virtual string Name { get; set; }
        public List<ProgramType> ChildList { get; set; }
        public ProgramType(string name)
        {
            this.ChildList = new List<ProgramType>();
            this.Name = name;
        }
    }

    /* Defines core information for types that hold relevant analysis data: classes, interfaces, and functions */
    public abstract class ProgramDataType : ProgramType
    {
        public List<string> TextData { get; set; }
        public List<string> Modifiers { get; }
        public List<string> Generics { get; }
        public ProgramDataType(string name, List<string> modifiers, List<string> generics) : base(name)
        {
            this.Modifiers = modifiers;
            this.Generics = generics;
            this.TextData = new List<string>();
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
            set
            {
                if (ProgramClassCollection != null) ProgramClassCollection.NotifyNameChange(this, value);
                base.Name = value;
            }
        }

        public ProgramClassType(string name, List<string> modifiers, List<string> generics) : base(name, modifiers, generics) 
        {
            this.SubClasses = new List<ProgramClassType>();
            this.SuperClasses = new List<ProgramClassType>();
        }

        public override bool Equals(object obj) // Defines equality based on name and type
        {
            return base.Name.Equals(((ProgramClassType)obj).Name) && this.GetType() == obj.GetType();
        }

        public override int GetHashCode() => // HashCode based on name and type
            this.Name.GetHashCode() + this.GetType().GetHashCode();
    }

    /* Defines unique data contained in an object representing a file */
    public class ProgramFile : ProgramType
    {
        public string FilePath { get; }
        public string FileText { get; }
        public List<string> FileTextData { get; }
        public ProgramFile(string filePath, string fileName, string fileText) : base(fileName)
        {
            this.FilePath = filePath;
            this.FileText = fileText;
            this.FileTextData = new List<string>();
        }
    }

    /* Defines unique data contained in an object representing a namespace */
    public class ProgramNamespace : ProgramType { public ProgramNamespace(string name) : base(name) { } }

    /* Defines unique data contained in an object representing a class */
    public class ProgramClass : ProgramClassType 
    {
        public List<ProgramClassType> OwnedClasses { get; }     // Composition/Aggregation (child data): ProgramClass(es) that are owned by ("part of") this class
        public List<ProgramClassType> OwnedByClasses { get; }   // Composition/Aggregation (parent data): ProgramClass(es) that this class is owned by ("part of")
        public List<ProgramClassType> UsedClasses { get; }      // Using (child data): ProgramClass(es) that this class uses
        public List<ProgramClassType> UsedByClasses { get; }    // Using (parent data): ProgramClass(es) that this class is used by
        public ProgramClass(string name, List<string> modifiers, List<string> generics) 
            : base(name, modifiers, generics)
        {
            this.OwnedClasses = new List<ProgramClassType>();
            this.OwnedByClasses = new List<ProgramClassType>();
            this.UsedClasses = new List<ProgramClassType>();
            this.UsedByClasses = new List<ProgramClassType>();
        }
    }

    /* Defines unique data contained in an object representing an interface */
    public class ProgramInterface : ProgramClassType
    {
        public ProgramInterface(string name, List<string> modifiers, List<string> generics) 
            : base(name, modifiers, generics) { }
    }

    /* Defines unique data contained in an object representing a function */
    public class ProgramFunction : ProgramDataType
    {
        public List<string> ReturnTypes { get; }
        public List<string> Parameters { get; }
        public List<string> BaseParameters { get; }
        public int Size { get; set; }
        public int Complexity { get; set; }
        public ProgramFunction(string name, List<string> modifiers, List<string> returnTypes, List<string> generics, List<string> parameters, List<string> baseParameters) 
            : base(name, modifiers, generics)
        {
            this.ReturnTypes = returnTypes;
            this.Parameters = parameters;
            this.BaseParameters = baseParameters;
            this.Size = 0;
            this.Complexity = 0;
        }
    }

    /* KeyedCollection for ProgramClassType - allows for quick retrieval of ProgramClassTypes by both index and key (name) */
    public class ProgramClassTypeCollection : KeyedCollection<string, ProgramClassType>
    {
        internal void NotifyNameChange(ProgramClassType programClassType, string newName) =>
            this.ChangeItemKey(programClassType, newName);
        protected override string GetKeyForItem(ProgramClassType item) => item.Name;
        protected override void InsertItem(int index, ProgramClassType item)
        {
            base.InsertItem(index, item);
            item.ProgramClassCollection = this;
        }
        protected override void SetItem(int index, ProgramClassType item)
        {
            base.SetItem(index, item);
            item.ProgramClassCollection = this;
        }
        protected override void RemoveItem(int index)
        {
            this[index].ProgramClassCollection = null;
            base.RemoveItem(index);
        }
        protected override void ClearItems()
        {
            foreach (ProgramClassType programClassType in this) programClassType.ProgramClassCollection = null;
            base.ClearItems();
        }
    }
}

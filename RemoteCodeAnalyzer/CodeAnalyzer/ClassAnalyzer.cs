using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    public class ClassAnalyzer
    {
        private int savedIndex = 0;
        private readonly ProgramClassType programClassType;
        private readonly ProgramClassTypeCollection programClassTypeCollection;

        public ClassAnalyzer(ProgramClassType programClassType, ProgramClassTypeCollection programClassTypeCollection)
        {
            this.programClassType = programClassType;
            this.programClassTypeCollection = programClassTypeCollection;
        }

        /* Starts the relationship processor for a class or interface */
        public void ProcessRelationships()
        {
            SetInheritanceRelationships(); // Get the superclass/subclass data from the beginning of the class text

            if (programClassType.GetType() != typeof(ProgramClass)) return; // Interfaces only collect inheritance data

            /* (1) Get the aggregation data from the class text and text of all children
             * (2) Get the using data from the parameters fields of all child functions */
            SetAggregationAndUsingRelationships(programClassType);
        }

        /* Populates superclass and subclass lists related to this class/interface */
        private void SetInheritanceRelationships()
        {
            string entry;
            bool hasSuperclasses = false;
            int brackets = 0;

            if (File.Exists(programClassType.DirectoryPath + "\\" + programClassType.Name + ".txt"))
            {
                using (StreamReader reader = File.OpenText(programClassType.DirectoryPath + "\\" + programClassType.Name + ".txt"))
                {
                    while (!reader.EndOfStream)
                    {
                        entry = reader.ReadLine();
                        savedIndex++;

                        if (!hasSuperclasses && entry.Equals(":")) // Look for a colon (signifies that the class/interface has a superclass)
                        {
                            hasSuperclasses = true;
                            continue;
                        }

                        if (entry.Equals("{")) return; // End the search at the first opening bracket

                        if (entry.Equals("[") || entry.Equals("<")) brackets++;

                        if (brackets > 0) // Ignore text within brackets
                        {
                            if (entry.Equals("]") || entry.Equals(">"))
                                brackets--;
                        }

                        // Entry might be a superclass - search the class list
                        if (hasSuperclasses && !programClassType.Name.Equals(entry) && programClassTypeCollection.Contains(entry))
                        {
                            // Add to each other's lists
                            ProgramClassType super = programClassTypeCollection[entry];
                            super.SubClasses.Add(programClassType);
                            programClassType.SuperClasses.Add(super);
                        }
                    }
                }
            }
        }

        /* Populates all relationship lists related to this class, except inheritance */
        private void SetAggregationAndUsingRelationships(ProgramDataType programDataType)
        {
            // Find and set the aggregation data
            SetAggregationRelationships(programDataType);

            // Find and set the using data
            SetUsingRelationships(programDataType);

            // Repeat recursively for each child class and function
            foreach (ProgramDataType child in programDataType.ChildList)
            {
                if (child.GetType() == typeof(ProgramClass) || child.GetType() == typeof(ProgramFunction))
                {
                    SetAggregationAndUsingRelationships(child);
                }
            }
        }

        /* Populates the aggregation lists related to this class */
        private void SetAggregationRelationships(ProgramDataType programDataType)
        {
            string entry;

            if (File.Exists(programDataType.DirectoryPath + "\\" + programDataType.Name + ".txt"))
            {
                using (StreamReader reader = File.OpenText(programDataType.DirectoryPath + "\\" + programDataType.Name + ".txt"))
                {
                    // TODO: change this to just be called from the inheritance method?
                    if (programDataType.Equals(programClassType)) // Skip the inheritance information
                        for (int i = 0; i < savedIndex && !reader.EndOfStream; i++)
                            entry = reader.ReadLine();

                    while (!reader.EndOfStream)
                    {
                        entry = reader.ReadLine();

                        // Check that "entry" is a different class/interface
                        if (!programClassType.Name.Equals(entry) && programClassTypeCollection.Contains(entry))
                        {
                            ProgramClassType owned = programClassTypeCollection[entry];

                            // Check that "owned" is a class and is not already in this class's OwnedClasses list
                            if (!((ProgramClass)programClassType).OwnedClasses.Contains(owned) && owned.GetType() == typeof(ProgramClass))
                            {
                                // Add each to the other's list
                                ((ProgramClass)owned).OwnedByClasses.Add(programClassType);
                                ((ProgramClass)programClassType).OwnedClasses.Add(owned);
                            }
                        }
                    }
                }
            }
        }

        /* Populates the using lists related to this class */
        private void SetUsingRelationships(ProgramDataType programDataType)
        {
            // Check that "programDataType" is a function with parameters
            if (programDataType.GetType() == typeof(ProgramFunction) &&
                (((ProgramFunction)programDataType).Parameters.Count > 0 || ((ProgramFunction)programDataType).ReturnTypes.Count > 0))
            {
                foreach (string parameter in ((ProgramFunction)programDataType).Parameters) // Search the parameters
                {
                    // Check that "parameter" is a different class/interface
                    if (!programClassType.Name.Equals(parameter) && programClassTypeCollection.Contains(parameter))
                    {
                        ProgramClassType used = programClassTypeCollection[parameter];

                        // Check that "used" is a class and is not already in this class's UsedClasses list
                        if (used.GetType() == typeof(ProgramClass) && !((ProgramClass)programClassType).UsedClasses.Contains(used))
                        {
                            // Add each to the other's lists
                            ((ProgramClass)used).UsedByClasses.Add(programClassType);
                            ((ProgramClass)programClassType).UsedClasses.Add(used);
                        }
                    }
                }
                foreach (string returnType in ((ProgramFunction)programDataType).ReturnTypes) // Search the return types
                {
                    // Check that "returnType" is a different class/interface
                    if (!programClassType.Name.Equals(returnType) && programClassTypeCollection.Contains(returnType))
                    {
                        ProgramClassType used = programClassTypeCollection[returnType];

                        // Check that "used" is a class and is not already in this class's UsedClasses list
                        if (used.GetType() == typeof(ProgramClass) && !((ProgramClass)programClassType).UsedClasses.Contains(used))
                        {
                            // Add each to the other's lists
                            ((ProgramClass)used).UsedByClasses.Add(programClassType);
                            ((ProgramClass)programClassType).UsedClasses.Add(used);
                        }
                    }
                }
            }
        }
    }
}

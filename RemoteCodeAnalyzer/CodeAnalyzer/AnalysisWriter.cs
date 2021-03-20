using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeAnalyzer
{
    public static class FunctionAnalysisWriter
    {
        /* Creates the new XML document, adds all elements, and saves the document */
        public static void WriteOutput(ConcurrentOrderedList processedFileList, string directoryPath, string projectName, string versionNumber)
        {
            XDocument xml = new XDocument(new XElement("analysis",
                new XAttribute("type", "function"),
                new XAttribute("project", projectName),
                new XAttribute("version", versionNumber)));

            for (int i = 0; i < processedFileList.Count; i++)
                SetElements(xml.Root, processedFileList[i]);

            try
            {
                xml.Save(directoryPath + "\\function_analysis.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save new function analysis XML file: {0}", e.ToString());
            }
        }

        /* Adds all elements for a file's function analysis data */
        private static void SetElements(XElement parent, ProgramType programType)
        {
            XElement element = null;

            // Find the type and create the new element
            if (programType.GetType() == typeof(ProgramFile))
                element = new XElement("file", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramNamespace))
                element = new XElement("namespace", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramClass))
                element = new XElement("class", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramInterface))
                element = new XElement("interface", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramFunction)) // Also add analysis data for functions
                element = new XElement("function", new XAttribute("name", programType.Name),
                                                    new XElement("size", ((ProgramFunction)programType).Size),
                                                    new XElement("complexity", ((ProgramFunction)programType).Complexity));

            if (element != null)
            {
                parent.Add(element); // Add the element to its parent
                foreach (ProgramType child in programType.ChildList) // Repeat recursively for each child
                    SetElements(element, child);
            }
        }
    }

    public static class RelationshipAnalysisWriter
    {
        /* Creates the new XML document, adds all elements, and saves the document */
        public static void WriteOutput(ConcurrentOrderedList processedFileList, string directoryPath, string projectName, string versionNumber)
        {
            XDocument xml = new XDocument(new XElement("analysis",
                new XAttribute("type", "relationship"),
                new XAttribute("project", projectName),
                new XAttribute("version", versionNumber)));

            for (int i = 0; i < processedFileList.Count; i++)
                SetElements(xml.Root, processedFileList[i]);

            try
            {
                xml.Save(directoryPath + "\\relationship_analysis.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save new relationship analysis XML file: {0}", e.ToString());
            }
        }

        /* Adds all elements for a file's relationship analysis data */
        private static void SetElements(XElement parent, ProgramType programType)
        {
            XElement element = null;

            // Find the type and open the new element
            if (programType.GetType() == typeof(ProgramFile))
                element = new XElement("file", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramNamespace))
                element = new XElement("namespace", new XAttribute("name", programType.Name));

            else if (programType.GetType() == typeof(ProgramClass))
            {
                element = new XElement("class", new XAttribute("name", programType.Name));
                SetClassAnalysisData((ProgramClass)programType, element); // Also add analysis data for classes
            }
            else if (programType.GetType() == typeof(ProgramInterface))
            {
                element = new XElement("interface", new XAttribute("name", programType.Name));
                SetInterfaceAnalysisData((ProgramInterface)programType, element); // Also add analysis data for interfaces
            }
            //else if (programType.GetType() == typeof(ProgramFunction))
                //element = new XElement("function", new XAttribute("name", programType.Name));

            if (element != null)
            {
                parent.Add(element); // Add the element to its parent
                foreach (ProgramType child in programType.ChildList) // Repeat recursively for each child
                    SetElements(element, child);
            }
        }

        /* Adds XML elements for a class's relationship data */
        private static void SetClassAnalysisData(ProgramClass programClass, XElement element)
        {
            SetInheritanceData(programClass, element);

            if (programClass.OwnedByClasses.Count > 0) // Aggregation, parents
                foreach (ProgramClassType ownerclass in programClass.OwnedByClasses)
                    element.Add(new XElement("aggregation_parent", ownerclass.Name));

            if (programClass.OwnedClasses.Count > 0) // Aggregation, children
                foreach (ProgramClassType ownedclass in programClass.OwnedClasses)
                    element.Add(new XElement("aggregation_child", ownedclass.Name));

            if (programClass.UsedByClasses.Count > 0) // Using, parents
                foreach (ProgramClassType userclass in programClass.UsedByClasses)
                    element.Add(new XElement("using_parent", userclass.Name));

            if (programClass.UsedClasses.Count > 0) // Using, children
                foreach (ProgramClassType usedclass in programClass.UsedClasses)
                    element.Add(new XElement("using_child", usedclass.Name));
        }

        /* Adds XML elements for an interface's relationship data */
        private static void SetInterfaceAnalysisData(ProgramInterface programInterface, XElement element)
            => SetInheritanceData(programInterface, element);

        /* Adds inheritance XML elements for a class or interface */
        private static void SetInheritanceData(ProgramClassType programClassType, XElement element)
        {
            if (programClassType.SuperClasses.Count > 0) // Inheritance, parents
                foreach (ProgramClassType superclass in programClassType.SuperClasses)
                    element.Add(new XElement("inheritance_parent", superclass.Name));

            if (programClassType.SubClasses.Count > 0) // Inheritance, children
                foreach (ProgramClassType subclass in programClassType.SubClasses)
                    element.Add(new XElement("inheritance_child", subclass.Name));
        }
    }
}

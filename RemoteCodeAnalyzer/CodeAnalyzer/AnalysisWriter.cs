using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace CodeAnalyzer
{
    public static class FunctionAnalysisWriter
    {
        /* Creates the new XML document, adds all elements, and saves the document */
        public static void WriteOutput(ConcurrentOrderedList processedFileList, string directoryPath, string projectName, string versionNumber)
        {
            XDocument xml = new XDocument(new XElement("analysis",
                new XAttribute("version", versionNumber),
                new XAttribute("project", projectName),
                new XAttribute("type", "function")));

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
                element = new XElement("file", new XAttribute("name", programType.Name + "." + ((ProgramFile)programType).FileType));

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
                new XAttribute("version", versionNumber),
                new XAttribute("project", projectName),
                new XAttribute("type", "relationship")));

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
                element = new XElement("file", new XAttribute("name", programType.Name + "." + ((ProgramFile)programType).FileType));

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

    public static class AnalysisMetadataWriter
    {
        /* Writes the metadata file - contains "severity" information for elements in code analysis */
        public static void WriteMetadata(string directoryPath)
        {
            XDocument functions;
            XDocument relationships;

            XDocument metadata = new XDocument();
            XElement severity = new XElement("severity");
            XElement fAnalysis = new XElement("analysis", new XAttribute("type", "function"));
            XElement rAnalysis = new XElement("analysis", new XAttribute("type", "relationship"));

            severity.Add(fAnalysis, rAnalysis);
            metadata.Add(severity);

            try
            {
                functions = XDocument.Load(directoryPath + "\\function_analysis.xml", LoadOptions.SetLineInfo);
                WriteFunctionSeverity(fAnalysis, functions.Root);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open function analysis file: {0}", e.ToString());
            }

            try
            {
                relationships = XDocument.Load(directoryPath + "\\relationship_analysis.xml", LoadOptions.SetLineInfo);
                WriteRelationshipSeverity(rAnalysis, relationships.Root);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to open relationship analysis file: {0}", e.ToString());
            }

            try
            {
                metadata.Save(directoryPath + "\\metadata.xml");
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to save analysis metadata file: {0}", e.ToString());
            }
        }

        private static void WriteFunctionSeverity(XElement metadata, XElement analysis)
        {
            IEnumerable<XElement> functions = analysis.Descendants("function");
            XElement low = new XElement("severity", new XAttribute("level", "low"));
            XElement medium = new XElement("severity", new XAttribute("level", "medium"));
            XElement high = new XElement("severity", new XAttribute("level", "high"));
            XElement size;
            XElement complexity;
            int _size;
            int _complexity;

            metadata.Add(low, medium, high);

            foreach (XElement function in functions)
            {
                size = function.Element("size");
                complexity = function.Element("complexity");
                _size = int.Parse(size.Value);
                _complexity = int.Parse(complexity.Value);

                if (_size > 150 || _complexity > 30)
                {
                    high.Add(new XElement("line", (function as IXmlLineInfo).LineNumber));

                    if (_size > 150) high.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));
                    else if (_size > 100) medium.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));
                    else if (_size > 50) low.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));

                    if (_complexity > 30) high.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                    else if (_complexity > 20) medium.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                    else if (_complexity > 10) low.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                }
                else if (_size > 100 || _complexity > 20)
                {
                    medium.Add(new XElement("line", (function as IXmlLineInfo).LineNumber));

                    if (_size > 100) medium.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));
                    else if (_size > 50) low.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));

                    if (_complexity > 20) medium.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                    else if (_complexity > 10) low.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                }
                else if (_size > 50 || _complexity > 10)
                {
                    low.Add(new XElement("line", (function as IXmlLineInfo).LineNumber));
                    if (_size > 50) low.Add(new XElement("line", (size as IXmlLineInfo).LineNumber));
                    if (_complexity > 10) low.Add(new XElement("line", (complexity as IXmlLineInfo).LineNumber));
                }
            }
        }

        private static void WriteRelationshipSeverity(XElement metadata, XElement analysis)
        {
            IEnumerable<XElement> classesAndInterfaces = from XElement element in analysis.Descendants()
                                                         where element.Name.ToString().Equals("class")
                                                            || element.Name.ToString().Equals("interface")
                                                         select element;

            XElement low = new XElement("severity", new XAttribute("level", "low"));
            XElement medium = new XElement("severity", new XAttribute("level", "medium"));
            XElement high = new XElement("severity", new XAttribute("level", "high"));
            IEnumerable<XElement> relationships;
            int _relationships;

            metadata.Add(low, medium, high);

            foreach (XElement classOrInterface in classesAndInterfaces)
            {
                relationships = from XElement element in classOrInterface.Elements()
                                where element.Name.ToString().Equals("inheritance_parent") || element.Name.ToString().Equals("inheritance_child")
                                    || element.Name.ToString().Equals("aggregation_parent") || element.Name.ToString().Equals("aggregation_child")
                                    || element.Name.ToString().Equals("using_parent") || element.Name.ToString().Equals("using_child")
                                select element;

                _relationships = relationships.Count();

                if (_relationships > 15)
                {
                    high.Add(new XElement("line", (classOrInterface as IXmlLineInfo).LineNumber));

                    //foreach (XElement relationship in relationships)
                        //high.Add(new XElement("line", (relationship as IXmlLineInfo).LineNumber));
                }
                else if (_relationships > 10)
                {
                    medium.Add(new XElement("line", (classOrInterface as IXmlLineInfo).LineNumber));

                    //foreach (XElement relationship in relationships)
                        //medium.Add(new XElement("line", (relationship as IXmlLineInfo).LineNumber));
                }
                else if (_relationships > 5)
                {
                    low.Add(new XElement("line", (classOrInterface as IXmlLineInfo).LineNumber));

                    //foreach (XElement relationship in relationships)
                        //low.Add(new XElement("line", (relationship as IXmlLineInfo).LineNumber));
                }
            }
        }
    }
}

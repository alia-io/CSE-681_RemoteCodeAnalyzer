using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests.CodeAnalyzerTests
{
    [TestClass]
    public class FileAnalyzerTests
    {
        [TestMethod]
        public void FileAnalyzerTest()
        {
            ProgramNamespace expectedNamespace;
            ProgramClass expectedClass;
            List<string> empty = new List<string>();

            string directoryPath = "..\\..\\TestFiles";
            string fileName = "FileAnalyzer_Input.cs";
            string filePath = directoryPath + "\\" + fileName;
            string tempFilePath = directoryPath + "\\temp\\" + fileName + ".txt";

            ProgramFile expectedProgramFile = new ProgramFile(fileName, directoryPath + "\\temp");
            ProgramFile actualProgramFile = new ProgramFile(fileName, directoryPath + "\\temp");
            ProgramClassTypeCollection expectedProgramClassTypes = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypes = new ProgramClassTypeCollection();

            expectedNamespace = new ProgramNamespace(expectedProgramFile, "CodeAnalyzer");
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "FileProcessor", empty, empty);
            new ProgramFunction(expectedClass, "FileProcessor", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ProcessFile", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "AddEntryToFileTextData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DetectDoubleCharacter", empty, empty, empty, empty, empty);
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "CodeProcessor", empty, empty);
            new ProgramFunction(expectedClass, "CodeProcessor", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ProcessFileCode", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ProcessProgramClassTypeData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ProcessFunctionData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "NewNamespace", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "NewProgramClassType", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "NewClass", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "NewInterface", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "NewFunction", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "GetClassTypeData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckIfFunction", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckIfConstructor_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckIfConstructor_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckIfDeconstructor", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "TestFunctionRequirement", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "TestConstructorRequirement_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "TestConstructorRequirement_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "TestDeconstructorRequirement", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep0", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep1", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep2", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep3", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep4", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep5", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep6", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "FunctionStep7", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep0_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep1_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep2_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep3_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep4_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep5_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep6_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep7_cs", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep0_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep1_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep2_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ConstructorStep3_java", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DeconstructorStep0", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DeconstructorStep1", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DeconstructorStep2", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DeconstructorStep3", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "DeconstructorStep4", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckControlFlowScopes", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "EndBracketedScope", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "EndBracketlessScope", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckScopeClosersWithinFunction", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "CheckScopeOpenersWithinFunction", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "UpdateTextData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "IncrementFunctionSize", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "IgnoreEntry", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "RemoveFunctionSignatureFromTextData", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "UpdateStringBuilder", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ClearCurrentItems", empty, empty, empty, empty, empty);
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "RelationshipProcessor", empty, empty);
            new ProgramFunction(expectedClass, "RelationshipProcessor", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "ProcessRelationships", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "SetInheritanceRelationships", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "SetAggregationAndUsingRelationships", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "SetAggregationRelationships", empty, empty, empty, empty, empty);
            new ProgramFunction(expectedClass, "SetUsingRelationships", empty, empty, empty, empty, empty);

            new TextParser(filePath, tempFilePath).ProcessFile();
            new FileAnalyzer(actualProgramFile, actualProgramClassTypes).ProcessFileCode();

            CheckAllChildLists(expectedProgramFile, actualProgramFile);
            CollectionAssert.AreEqual(expectedProgramClassTypes, actualProgramClassTypes);
        }

        public static void CheckAllChildLists(ProgramType expected, ProgramType actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.GetType(), actual.GetType());
            Assert.AreEqual(expected.ChildList.Count, actual.ChildList.Count);

            for (int i = 0; i < expected.ChildList.Count; i++)
                CheckAllChildLists(expected.ChildList[i], actual.ChildList[i]);
        }
    }
}

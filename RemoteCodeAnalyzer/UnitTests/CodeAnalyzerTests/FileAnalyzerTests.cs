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
        public void Test1()
        {
            ProgramClass expectedClass;
            List<string> empty = new List<string>();

            ProgramFile expectedProgramFile = new ProgramFile("TestInput1.cs", "..\\..\\TestFiles\\FileAnalyzer");
            ProgramFile actualProgramFile = new ProgramFile("TestInput1.cs", "..\\..\\TestFiles\\FileAnalyzer");
            ProgramClassTypeCollection expectedProgramClassTypes = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypes = new ProgramClassTypeCollection();

            new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IAnimalActions", empty, empty);
            new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IHumanActions", empty, empty);
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Animal", empty, empty);
            new ProgramFunction(expectedClass, "Animal", empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "Move", empty, empty, empty, empty, empty) { Size = 9, Complexity = 4 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Pet", empty, empty);
            new ProgramFunction(expectedClass, "Pet", empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Dog", empty, empty);
            new ProgramFunction(expectedClass, "Dog", empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "Talk", empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Human", empty, empty);
            new ProgramFunction(expectedClass, "Human", empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            new ProgramFunction(expectedClass, "Talk", empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "Move", empty, empty, empty, empty, empty) { Size = 11, Complexity = 5 };
            new ProgramFunction(expectedClass, "GoToSchool", empty, empty, empty, empty, empty) { Size = 6, Complexity = 1 };
            new ProgramFunction(expectedClass, "GraduateSchool", empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "GoToWork", empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyPet", empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyDog", empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyCar", empty, empty, empty, empty, empty) { Size = 8, Complexity = 1 };
            new ProgramFunction(expectedClass, "SellCar", empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "FillCarFuelTank", empty, empty, empty, empty, empty) { Size = 8, Complexity = 2 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Car", empty, empty);
            new ProgramFunction(expectedClass, "Car", empty, empty, empty, empty, empty) { Size = 5, Complexity = 0 };
            new ProgramFunction(expectedClass, "FillTank", empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };

            new TextParser("..\\..\\TestFiles\\FileAnalyzer", "TestInput1.cs").ProcessFile();
            new FileAnalyzer(actualProgramFile, actualProgramClassTypes).ProcessFileCode();

            CheckAllChildLists(expectedProgramFile, actualProgramFile);
            CollectionAssert.AreEqual(expectedProgramClassTypes, actualProgramClassTypes);
        }

        [TestMethod]
        public void Test2()
        {
            ProgramNamespace expectedNamespace;
            ProgramClass expectedClass;
            List<string> empty = new List<string>();

            ProgramFile expectedProgramFile = new ProgramFile("TestInput2.cs", "..\\..\\TestFiles\\FileAnalyzer");
            ProgramFile actualProgramFile = new ProgramFile("TestInput2.cs", "..\\..\\TestFiles\\FileAnalyzer");
            ProgramClassTypeCollection expectedProgramClassTypes = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypes = new ProgramClassTypeCollection();

            expectedNamespace = new ProgramNamespace(expectedProgramFile, "CodeAnalyzer");
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "FileProcessor", empty, empty);
            new ProgramFunction(expectedClass, "FileProcessor", empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessFile", empty, empty, empty, empty, empty) { Size = 41, Complexity = 6 };
            new ProgramFunction(expectedClass, "AddEntryToFileTextData", empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "DetectDoubleCharacter", empty, empty, empty, empty, empty) { Size = 23, Complexity = 11 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "CodeProcessor", empty, empty);
            new ProgramFunction(expectedClass, "CodeProcessor", empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessFileCode", empty, empty, empty, empty, empty) { Size = 32, Complexity = 6 };
            new ProgramFunction(expectedClass, "ProcessProgramClassTypeData", empty, empty, empty, empty, empty) { Size = 48, Complexity = 10 };
            new ProgramFunction(expectedClass, "ProcessFunctionData", empty, empty, empty, empty, empty) { Size = 29, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewNamespace", empty, empty, empty, empty, empty) { Size = 25, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewProgramClassType", empty, empty, empty, empty, empty) { Size = 26, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewClass", empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "NewInterface", empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "NewFunction", empty, empty, empty, empty, empty) { Size = 13, Complexity = 2 };
            new ProgramFunction(expectedClass, "GetClassTypeData", empty, empty, empty, empty, empty) { Size = 38, Complexity = 10 };
            new ProgramFunction(expectedClass, "CheckIfFunction", empty, empty, empty, empty, empty) { Size = 44, Complexity = 12 };
            new ProgramFunction(expectedClass, "CheckIfConstructor_cs", empty, empty, empty, empty, empty) { Size = 37, Complexity = 9 };
            new ProgramFunction(expectedClass, "CheckIfConstructor_java", empty, empty, empty, empty, empty) { Size = 39, Complexity = 11 };
            new ProgramFunction(expectedClass, "CheckIfDeconstructor", empty, empty, empty, empty, empty) { Size = 21, Complexity = 3 };
            new ProgramFunction(expectedClass, "TestFunctionRequirement", empty, empty, empty, empty, empty) { Size = 28, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestConstructorRequirement_cs", empty, empty, empty, empty, empty) { Size = 28, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestConstructorRequirement_java", empty, empty, empty, empty, empty) { Size = 16, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestDeconstructorRequirement", empty, empty, empty, empty, empty) { Size = 19, Complexity = 1 };
            new ProgramFunction(expectedClass, "FunctionStep0", empty, empty, empty, empty, empty) { Size = 16, Complexity = 5 };
            new ProgramFunction(expectedClass, "FunctionStep1", empty, empty, empty, empty, empty) { Size = 27, Complexity = 7 };
            new ProgramFunction(expectedClass, "FunctionStep2", empty, empty, empty, empty, empty) { Size = 37, Complexity = 12 };
            new ProgramFunction(expectedClass, "FunctionStep3", empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "FunctionStep4", empty, empty, empty, empty, empty) { Size = 15, Complexity = 4 };
            new ProgramFunction(expectedClass, "FunctionStep5", empty, empty, empty, empty, empty) { Size = 11, Complexity = 4 };
            new ProgramFunction(expectedClass, "FunctionStep6", empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "FunctionStep7", empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "ConstructorStep0_cs", empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ConstructorStep1_cs", empty, empty, empty, empty, empty) { Size = 23, Complexity = 5 };
            new ProgramFunction(expectedClass, "ConstructorStep2_cs", empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep3_cs", empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep4_cs", empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep5_cs", empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep6_cs", empty, empty, empty, empty, empty) { Size = 10, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep7_cs", empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "ConstructorStep0_java", empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ConstructorStep1_java", empty, empty, empty, empty, empty) { Size = 23, Complexity = 5 };
            new ProgramFunction(expectedClass, "ConstructorStep2_java", empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep3_java", empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "DeconstructorStep0", empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep1", empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep2", empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep3", empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep4", empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "CheckControlFlowScopes", empty, empty, empty, empty, empty) { Size = 19, Complexity = 5 };
            new ProgramFunction(expectedClass, "EndBracketedScope", empty, empty, empty, empty, empty) { Size = 35, Complexity = 11 };
            new ProgramFunction(expectedClass, "EndBracketlessScope", empty, empty, empty, empty, empty) { Size = 22, Complexity = 5 };
            new ProgramFunction(expectedClass, "CheckScopeClosersWithinFunction", empty, empty, empty, empty, empty) { Size = 18, Complexity = 5 };
            new ProgramFunction(expectedClass, "CheckScopeOpenersWithinFunction", empty, empty, empty, empty, empty) { Size = 16, Complexity = 6 };
            new ProgramFunction(expectedClass, "UpdateTextData", empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "IncrementFunctionSize", empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "IgnoreEntry", empty, empty, empty, empty, empty) { Size = 39, Complexity = 10 };
            new ProgramFunction(expectedClass, "RemoveFunctionSignatureFromTextData", empty, empty, empty, empty, empty) { Size = 15, Complexity = 2 };
            new ProgramFunction(expectedClass, "UpdateStringBuilder", empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ClearCurrentItems", empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "RelationshipProcessor", empty, empty);
            new ProgramFunction(expectedClass, "RelationshipProcessor", empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessRelationships", empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };
            new ProgramFunction(expectedClass, "SetInheritanceRelationships", empty, empty, empty, empty, empty) { Size = 41, Complexity = 7 };
            new ProgramFunction(expectedClass, "SetAggregationAndUsingRelationships", empty, empty, empty, empty, empty) { Size = 14, Complexity = 2 };
            new ProgramFunction(expectedClass, "SetAggregationRelationships", empty, empty, empty, empty, empty) { Size = 16, Complexity = 3 };
            new ProgramFunction(expectedClass, "SetUsingRelationships", empty, empty, empty, empty, empty) { Size = 37, Complexity = 7 };

            new TextParser("..\\..\\TestFiles\\FileAnalyzer", "TestInput2.cs").ProcessFile();
            new FileAnalyzer(actualProgramFile, actualProgramClassTypes).ProcessFileCode();

            CheckAllChildLists(expectedProgramFile, actualProgramFile);
            CollectionAssert.AreEqual(expectedProgramClassTypes, actualProgramClassTypes);
        }

        public static void CheckAllChildLists(ProgramType expected, ProgramType actual)
        {
            Assert.AreEqual(expected.Name, actual.Name);
            Assert.AreEqual(expected.GetType(), actual.GetType());
            Assert.AreEqual(expected.ChildList.Count, actual.ChildList.Count);

            if (expected.GetType() == typeof(ProgramFunction) && actual.GetType() == typeof(ProgramFunction))
            {
                Assert.AreEqual(((ProgramFunction)expected).Size, ((ProgramFunction)actual).Size);
                Assert.AreEqual(((ProgramFunction)expected).Complexity, ((ProgramFunction)actual).Complexity);
            }

            for (int i = 0; i < expected.ChildList.Count; i++)
                CheckAllChildLists(expected.ChildList[i], actual.ChildList[i]);
        }
    }
}

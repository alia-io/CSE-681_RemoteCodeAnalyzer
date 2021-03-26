using System.Collections.Generic;
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

            new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IAnimalActions", 0, empty, empty);
            new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IHumanActions", 0, empty, empty);
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Animal", 0, empty, empty);
            new ProgramFunction(expectedClass, "Animal", 0, empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "Move", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 4 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Pet", 0, empty, empty);
            new ProgramFunction(expectedClass, "Pet", 0, empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Dog", 0, empty, empty);
            new ProgramFunction(expectedClass, "Dog", 0, empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "Talk", 0, empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Human", 0, empty, empty);
            new ProgramFunction(expectedClass, "Human", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            new ProgramFunction(expectedClass, "Talk", 0, empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "Move", 0, empty, empty, empty, empty, empty) { Size = 11, Complexity = 5 };
            new ProgramFunction(expectedClass, "GoToSchool", 0, empty, empty, empty, empty, empty) { Size = 6, Complexity = 1 };
            new ProgramFunction(expectedClass, "GraduateSchool", 0, empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "GoToWork", 0, empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyPet", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyDog", 0, empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "BuyCar", 0, empty, empty, empty, empty, empty) { Size = 8, Complexity = 1 };
            new ProgramFunction(expectedClass, "SellCar", 0, empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "FillCarFuelTank", 0, empty, empty, empty, empty, empty) { Size = 8, Complexity = 2 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Car", 0, empty, empty);
            new ProgramFunction(expectedClass, "Car", 0, empty, empty, empty, empty, empty) { Size = 5, Complexity = 0 };
            new ProgramFunction(expectedClass, "FillTank", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };

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

            expectedNamespace = new ProgramNamespace(expectedProgramFile, "CodeAnalyzer", 0);
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "FileProcessor", 0, empty, empty);
            new ProgramFunction(expectedClass, "FileProcessor", 0, empty, empty, empty, empty, empty) { Size = 1, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessFile", 0, empty, empty, empty, empty, empty) { Size = 41, Complexity = 6 };
            new ProgramFunction(expectedClass, "AddEntryToFileTextData", 0, empty, empty, empty, empty, empty) { Size = 5, Complexity = 1 };
            new ProgramFunction(expectedClass, "DetectDoubleCharacter", 0, empty, empty, empty, empty, empty) { Size = 23, Complexity = 11 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "CodeProcessor", 0, empty, empty);
            new ProgramFunction(expectedClass, "CodeProcessor", 0, empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessFileCode", 0, empty, empty, empty, empty, empty) { Size = 32, Complexity = 6 };
            new ProgramFunction(expectedClass, "ProcessProgramClassTypeData", 0, empty, empty, empty, empty, empty) { Size = 48, Complexity = 10 };
            new ProgramFunction(expectedClass, "ProcessFunctionData", 0, empty, empty, empty, empty, empty) { Size = 29, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewNamespace", 0, empty, empty, empty, empty, empty) { Size = 25, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewProgramClassType", 0, empty, empty, empty, empty, empty) { Size = 26, Complexity = 5 };
            new ProgramFunction(expectedClass, "NewClass", 0, empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "NewInterface", 0, empty, empty, empty, empty, empty) { Size = 3, Complexity = 0 };
            new ProgramFunction(expectedClass, "NewFunction", 0, empty, empty, empty, empty, empty) { Size = 13, Complexity = 2 };
            new ProgramFunction(expectedClass, "GetClassTypeData", 0, empty, empty, empty, empty, empty) { Size = 38, Complexity = 10 };
            new ProgramFunction(expectedClass, "CheckIfFunction", 0, empty, empty, empty, empty, empty) { Size = 44, Complexity = 12 };
            new ProgramFunction(expectedClass, "CheckIfConstructor_cs", 0, empty, empty, empty, empty, empty) { Size = 37, Complexity = 9 };
            new ProgramFunction(expectedClass, "CheckIfConstructor_java", 0, empty, empty, empty, empty, empty) { Size = 39, Complexity = 11 };
            new ProgramFunction(expectedClass, "CheckIfDeconstructor", 0, empty, empty, empty, empty, empty) { Size = 21, Complexity = 3 };
            new ProgramFunction(expectedClass, "TestFunctionRequirement", 0, empty, empty, empty, empty, empty) { Size = 28, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestConstructorRequirement_cs", 0, empty, empty, empty, empty, empty) { Size = 28, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestConstructorRequirement_java", 0, empty, empty, empty, empty, empty) { Size = 16, Complexity = 1 };
            new ProgramFunction(expectedClass, "TestDeconstructorRequirement", 0, empty, empty, empty, empty, empty) { Size = 19, Complexity = 1 };
            new ProgramFunction(expectedClass, "FunctionStep0", 0, empty, empty, empty, empty, empty) { Size = 16, Complexity = 5 };
            new ProgramFunction(expectedClass, "FunctionStep1", 0, empty, empty, empty, empty, empty) { Size = 27, Complexity = 7 };
            new ProgramFunction(expectedClass, "FunctionStep2", 0, empty, empty, empty, empty, empty) { Size = 37, Complexity = 12 };
            new ProgramFunction(expectedClass, "FunctionStep3", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "FunctionStep4", 0, empty, empty, empty, empty, empty) { Size = 15, Complexity = 4 };
            new ProgramFunction(expectedClass, "FunctionStep5", 0, empty, empty, empty, empty, empty) { Size = 11, Complexity = 4 };
            new ProgramFunction(expectedClass, "FunctionStep6", 0, empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "FunctionStep7", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "ConstructorStep0_cs", 0, empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ConstructorStep1_cs", 0, empty, empty, empty, empty, empty) { Size = 23, Complexity = 5 };
            new ProgramFunction(expectedClass, "ConstructorStep2_cs", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep3_cs", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep4_cs", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep5_cs", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep6_cs", 0, empty, empty, empty, empty, empty) { Size = 10, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep7_cs", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "ConstructorStep0_java", 0, empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ConstructorStep1_java", 0, empty, empty, empty, empty, empty) { Size = 23, Complexity = 5 };
            new ProgramFunction(expectedClass, "ConstructorStep2_java", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 2 };
            new ProgramFunction(expectedClass, "ConstructorStep3_java", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "DeconstructorStep0", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep1", 0, empty, empty, empty, empty, empty) { Size = 9, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep2", 0, empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep3", 0, empty, empty, empty, empty, empty) { Size = 6, Complexity = 2 };
            new ProgramFunction(expectedClass, "DeconstructorStep4", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "CheckControlFlowScopes", 0, empty, empty, empty, empty, empty) { Size = 19, Complexity = 5 };
            new ProgramFunction(expectedClass, "EndBracketedScope", 0, empty, empty, empty, empty, empty) { Size = 35, Complexity = 11 };
            new ProgramFunction(expectedClass, "EndBracketlessScope", 0, empty, empty, empty, empty, empty) { Size = 22, Complexity = 5 };
            new ProgramFunction(expectedClass, "CheckScopeClosersWithinFunction", 0, empty, empty, empty, empty, empty) { Size = 18, Complexity = 5 };
            new ProgramFunction(expectedClass, "CheckScopeOpenersWithinFunction", 0, empty, empty, empty, empty, empty) { Size = 16, Complexity = 6 };
            new ProgramFunction(expectedClass, "UpdateTextData", 0, empty, empty, empty, empty, empty) { Size = 4, Complexity = 1 };
            new ProgramFunction(expectedClass, "IncrementFunctionSize", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 1 };
            new ProgramFunction(expectedClass, "IgnoreEntry", 0, empty, empty, empty, empty, empty) { Size = 39, Complexity = 10 };
            new ProgramFunction(expectedClass, "RemoveFunctionSignatureFromTextData", 0, empty, empty, empty, empty, empty) { Size = 15, Complexity = 2 };
            new ProgramFunction(expectedClass, "UpdateStringBuilder", 0, empty, empty, empty, empty, empty) { Size = 10, Complexity = 3 };
            new ProgramFunction(expectedClass, "ClearCurrentItems", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            expectedClass = new ProgramClass(expectedProgramClassTypes, expectedNamespace, "RelationshipProcessor", 0, empty, empty);
            new ProgramFunction(expectedClass, "RelationshipProcessor", 0, empty, empty, empty, empty, empty) { Size = 2, Complexity = 0 };
            new ProgramFunction(expectedClass, "ProcessRelationships", 0, empty, empty, empty, empty, empty) { Size = 7, Complexity = 1 };
            new ProgramFunction(expectedClass, "SetInheritanceRelationships", 0, empty, empty, empty, empty, empty) { Size = 41, Complexity = 7 };
            new ProgramFunction(expectedClass, "SetAggregationAndUsingRelationships", 0, empty, empty, empty, empty, empty) { Size = 14, Complexity = 2 };
            new ProgramFunction(expectedClass, "SetAggregationRelationships", 0, empty, empty, empty, empty, empty) { Size = 16, Complexity = 3 };
            new ProgramFunction(expectedClass, "SetUsingRelationships", 0, empty, empty, empty, empty, empty) { Size = 37, Complexity = 7 };

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

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests.CodeAnalyzerTests
{
    [TestClass]
    public class ClassAnalyzerTests
    {
        [TestMethod]
        public void Test1()
        {
            List<string> empty = new List<string>();

            ProgramFile expectedProgramFile = new ProgramFile("TestInput1.cs", "..\\..\\TestFiles\\ClassAnalyzer");
            ProgramFile actualProgramFile = new ProgramFile("TestInput1.cs", "..\\..\\TestFiles\\ClassAnalyzer");
            ProgramClassTypeCollection expectedProgramClassTypes = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypes = new ProgramClassTypeCollection();

            ProgramInterface iAnimalActions = new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IAnimalActions", 0, empty, empty);
            ProgramInterface iHumanActions = new ProgramInterface(expectedProgramClassTypes, expectedProgramFile, "IHumanActions", 0, empty, empty);
            ProgramClass animal = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Animal", 0, empty, empty);
            ProgramClass pet = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Pet", 0, empty, empty);
            ProgramClass dog = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Dog", 0, empty, empty);
            ProgramClass human = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Human", 0, empty, empty);
            ProgramClass car = new ProgramClass(expectedProgramClassTypes, expectedProgramFile, "Car", 0, empty, empty);

            iAnimalActions.SubClasses.Add(dog);
            iAnimalActions.SubClasses.Add(human);
            iHumanActions.SubClasses.Add(human);
            animal.SubClasses.Add(pet);
            animal.SubClasses.Add(human);
            pet.SuperClasses.Add(animal);
            pet.SubClasses.Add(dog);
            pet.OwnedByClasses.Add(human);
            pet.UsedByClasses.Add(human);
            dog.SuperClasses.Add(pet);
            dog.SuperClasses.Add(iAnimalActions);
            dog.OwnedByClasses.Add(human);
            human.SuperClasses.Add(animal);
            human.SuperClasses.Add(iAnimalActions);
            human.SuperClasses.Add(iHumanActions);
            human.OwnedClasses.Add(car);
            human.OwnedClasses.Add(pet);
            human.OwnedClasses.Add(dog);
            human.UsedClasses.Add(pet);
            car.OwnedByClasses.Add(human);

            new TextParser("..\\..\\TestFiles\\ClassAnalyzer", "TestInput1.cs").ProcessFile();
            new FileAnalyzer(actualProgramFile, actualProgramClassTypes).ProcessFileCode();
            foreach (ProgramClassType programClassType in actualProgramClassTypes)
                new ClassAnalyzer(programClassType, actualProgramClassTypes).ProcessRelationships();

            CheckRelationships(expectedProgramClassTypes, actualProgramClassTypes);
        }

        [TestMethod]
        public void Test2()
        {
            List<string> empty = new List<string>();

            ProgramFile expectedProgramFile = new ProgramFile("TestInput2.cs", "..\\..\\TestFiles\\ClassAnalyzer");
            ProgramFile actualProgramFile = new ProgramFile("TestInput2.cs", "..\\..\\TestFiles\\ClassAnalyzer");
            ProgramClassTypeCollection expectedProgramClassTypes = new ProgramClassTypeCollection();
            ProgramClassTypeCollection actualProgramClassTypes = new ProgramClassTypeCollection();

            ProgramNamespace codeAnalyzer = new ProgramNamespace(expectedProgramFile, "CodeAnalyzer", 0);
            ProgramClass programType = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramType", 0, empty, empty);
            ProgramClass programDataType = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramDataType", 0, empty, empty);
            ProgramClass programClassType = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramClassType", 0, empty, empty);
            ProgramClass programFile = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramFile", 0, empty, empty);
            ProgramClass programNamespace = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramNamespace", 0, empty, empty);
            ProgramClass programClass = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramClass", 0, empty, empty);
            ProgramClass programInterface = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramInterface", 0, empty, empty);
            ProgramClass programFunction = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramFunction", 0, empty, empty);
            ProgramClass programClassTypeCollection = new ProgramClass(expectedProgramClassTypes, codeAnalyzer, "ProgramClassTypeCollection", 0, empty, empty);

            programType.SubClasses.Add(programDataType);
            programType.SubClasses.Add(programFile);
            programType.SubClasses.Add(programNamespace);
            programDataType.SuperClasses.Add(programType);
            programDataType.SubClasses.Add(programClassType);
            programDataType.SubClasses.Add(programFunction);
            programClassType.SuperClasses.Add(programDataType);
            programClassType.SubClasses.Add(programClass);
            programClassType.SubClasses.Add(programInterface);
            programClassType.SubClasses.Add(programClassTypeCollection);
            programClassType.OwnedClasses.Add(programClassTypeCollection);
            programClassType.OwnedByClasses.Add(programClass);
            programClassType.OwnedByClasses.Add(programClassTypeCollection);
            programClassType.UsedByClasses.Add(programClassTypeCollection);
            programFile.SuperClasses.Add(programType);
            programNamespace.SuperClasses.Add(programType);
            programClass.SuperClasses.Add(programClassType);
            programClass.OwnedClasses.Add(programClassType);
            programInterface.SuperClasses.Add(programClassType);
            programFunction.SuperClasses.Add(programDataType);
            programClassTypeCollection.SuperClasses.Add(programClassType);
            programClassTypeCollection.OwnedClasses.Add(programClassType);
            programClassTypeCollection.OwnedByClasses.Add(programClassType);
            programClassTypeCollection.UsedClasses.Add(programClassType);

            new TextParser("..\\..\\TestFiles\\ClassAnalyzer", "TestInput2.cs").ProcessFile();
            new FileAnalyzer(actualProgramFile, actualProgramClassTypes).ProcessFileCode();
            foreach (ProgramClassType classType in actualProgramClassTypes)
                new ClassAnalyzer(classType, actualProgramClassTypes).ProcessRelationships();

            CheckRelationships(expectedProgramClassTypes, actualProgramClassTypes);
        }

        private static void CheckRelationships(ProgramClassTypeCollection expectedProgramClassTypes, ProgramClassTypeCollection actualProgramClassTypes)
        {
            CollectionAssert.AreEqual(expectedProgramClassTypes, actualProgramClassTypes);

            for (int i = 0; i < expectedProgramClassTypes.Count; i++)
            {
                ProgramClassType expectedProgramClassType = expectedProgramClassTypes[i];
                ProgramClassType actualProgramClassType = actualProgramClassTypes[i];

                CollectionAssert.AreEqual(expectedProgramClassType.SubClasses, actualProgramClassType.SubClasses);
                CollectionAssert.AreEqual(expectedProgramClassType.SuperClasses, actualProgramClassType.SuperClasses);

                if (expectedProgramClassType.GetType() == typeof(ProgramClass) && actualProgramClassType.GetType() == typeof(ProgramClass))
                {
                    ProgramClass expectedProgramClass = (ProgramClass)expectedProgramClassType;
                    ProgramClass actualProgramClass = (ProgramClass)actualProgramClassType;

                    CollectionAssert.AreEqual(expectedProgramClass.OwnedClasses, actualProgramClass.OwnedClasses);
                    CollectionAssert.AreEqual(expectedProgramClass.OwnedByClasses, actualProgramClass.OwnedByClasses);
                    CollectionAssert.AreEqual(expectedProgramClass.UsedClasses, actualProgramClass.UsedClasses);
                    CollectionAssert.AreEqual(expectedProgramClass.UsedByClasses, actualProgramClass.UsedByClasses);
                }
            }
        }
    }
}

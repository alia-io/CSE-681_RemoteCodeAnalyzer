using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests.CodeAnalyzerTests
{
    [TestClass]
    public class TextParserTests
    {
        [TestMethod]
        public void Test1()
        {
            new TextParser("..\\..\\TestFiles\\TextParser", "TestInput1.txt").ProcessFile();

            string expected = File.ReadAllText("..\\..\\TestFiles\\TextParser\\TestExpected1.txt");
            string actual = File.ReadAllText("..\\..\\TestFiles\\TextParser\\temp\\TestInput1.txt.txt");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Test2()
        {
            new TextParser("..\\..\\TestFiles\\TextParser", "TestInput2.txt").ProcessFile();

            string expected = File.ReadAllText("..\\..\\TestFiles\\TextParser\\TestExpected2.txt");
            string actual = File.ReadAllText("..\\..\\TestFiles\\TextParser\\temp\\TestInput2.txt.txt");

            Assert.AreEqual(expected, actual);
        }
    }
}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests
{
    [TestClass]
    public class TextParserTests
    {
        [TestMethod]
        public void TextParser1()
        {
            string filePath = "..\\..\\TestFiles\\TextParser1_Input.txt";
            string tempFilePath = "..\\..\\TestFiles\\temp\\TextParser1_Input.txt.txt";
            string expected = File.ReadAllText("..\\..\\TestFiles\\temp\\TextParser1_Expected.txt");
            string actual;

            TextParser parser = new TextParser(filePath, tempFilePath);
            parser.ProcessFile();

            actual = File.ReadAllText(tempFilePath);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestParser2()
        {
            string filePath = "..\\..\\TestFiles\\TextParser2_Input.txt";
            string tempFilePath = "..\\..\\TestFiles\\temp\\TextParser2_Input.txt.txt";
            string expected = File.ReadAllText("..\\..\\TestFiles\\temp\\TextParser2_Expected.txt");
            string actual;

            TextParser parser = new TextParser(filePath, tempFilePath);
            parser.ProcessFile();

            actual = File.ReadAllText(tempFilePath);

            Assert.AreEqual(expected, actual);
        }
    }
}

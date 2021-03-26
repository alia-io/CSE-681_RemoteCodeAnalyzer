///////////////////////////////////////////////////////////////////////////////////
///                                                                             ///
///  TextParser.cs - Parses a code file into a temp parsed file                 ///
///                                                                             ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                 ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10      ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                ///
///                 Software Modeling and Analysis, 2021                        ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu            ///
///                                                                             ///
///////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This class reads each character of a code file to separate its data into
 *   logical programming items. The data is saved to a temp file for future
 *   code analysis. Words with only alphanumeric characters are parsed as a
 *   single entity. Symbols are parsed as individual characters, or, if two
 *   combined symbols have a valid syntactical meaning, as a two-character string.
 *   New lines are preserved and denoted by a single space.
 * 
 *   Public Interface
 *   ----------------
 *   TextParser textParser = new TextParser((string) directoryPath, (string) fileName);
 *   textParser.ProcessFile();
 */

using System.IO;
using System.Text;

namespace CodeAnalyzer
{
    /* Pre-processes code file into a list of strings stored in temporary txt file */
    public class TextParser
    {
        StreamReader reader;
        StreamWriter writer;
        private readonly string filePath;
        private readonly string tempFilePath;
        private readonly StringBuilder item = new StringBuilder();

        // filePath = directoryPath + "\\" + filename, 
        // tempFilePath = directoryPath + "\\temp\\" + filename + ".txt"
        public TextParser(string directoryPath, string fileName)
        {
            filePath = directoryPath + "\\" + fileName;
            tempFilePath = directoryPath + "\\temp\\" + fileName + ".txt";
        }

        /* Parses the file text into a list of strings (stored in temp txt file), dividing elements logically */
        public void ProcessFile()
        {
            string line;

            using (reader = File.OpenText(filePath))
            {
                using (writer = File.CreateText(tempFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        line = reader.ReadLine(); // Get the next line
                        foreach (char current in line) // Read the line char by char
                        {
                            if (char.IsWhiteSpace(current)) // Add the StringBuilder contents to the temp txt file
                            {
                                WriteItemToFile();
                                continue;
                            }

                            // Check special cases
                            if ((char.IsPunctuation(current) || char.IsSymbol(current)) && !current.Equals('_'))
                            {
                                // Detect double-character symbols
                                if (item.Length == 1 && (char.IsPunctuation(item[0]) || char.IsSymbol(item[0]))
                                    && this.DetectDoubleCharacter(item[0], current))
                                {
                                    item.Append(current);
                                    WriteItemToFile();
                                    continue;
                                }
                                WriteItemToFile();
                            }
                            else if (item.Length == 1 && !(item[0]).Equals('_') && (char.IsPunctuation(item[0]) || char.IsSymbol(item[0])))
                                WriteItemToFile();

                            item.Append(current);
                        }

                        WriteItemToFile();
                        writer.WriteLine(" "); // Marker for a new line
                    }
                }
            }
        }

        /* Adds the current StringBuilder contents to the temp txt file and clears the StringBuilder */
        private void WriteItemToFile()
        {
            if (item.Length > 0)
            {
                writer.WriteLine(item);
                item.Clear();
            }
        }

        /* Tests for two-character sequences that have a combined syntactical meaning */
        private bool DetectDoubleCharacter(char previous, char current)
        {
            if (previous.Equals('/') && (current.Equals('/') || current.Equals('*') || current.Equals('=')))
                return true;
            if (previous.Equals('*') && current.Equals('/'))
                return true;
            if (previous.Equals('+') && (current.Equals('+') || current.Equals('=')))
                return true;
            if (previous.Equals('-') && (current.Equals('-') || current.Equals('=')))
                return true;
            if (previous.Equals('>') && (current.Equals('>') || current.Equals('=')))
                return true;
            if (previous.Equals('<') && (current.Equals('<') || current.Equals('=')))
                return true;
            if ((previous.Equals('*') || previous.Equals('!') || previous.Equals('%')) && current.Equals('='))
                return true;
            if (previous.Equals('=') && (current.Equals('>') || current.Equals('=')))
                return true;
            if (previous.Equals('&') && current.Equals('&'))
                return true;
            if (previous.Equals('|') && current.Equals('|'))
                return true;
            if (previous.Equals('\\') && (current.Equals('\\') || current.Equals('"') || current.Equals('\'')))
                return true;
            return false;
        }
    }
}

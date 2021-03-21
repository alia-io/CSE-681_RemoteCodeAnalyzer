using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft;

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

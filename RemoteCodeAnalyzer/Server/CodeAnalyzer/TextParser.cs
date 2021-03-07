using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft;

namespace Server
{
    /* Pre-processor of file text into a list of strings */
    public class TextParser
    {
        private readonly ProgramFile programFile;
        private readonly StringBuilder stringBuilder = new StringBuilder();

        public TextParser(ProgramFile programFile) => this.programFile = programFile;

        /* Puts the file next into a string list, dividing elements logically */
        public void ProcessFile()
        {
            IEnumerator enumerator;

            // Split text by line
            string[] programLines = programFile.FileText.Split(new String[] { Environment.NewLine }, StringSplitOptions.None);

            for (int i = 0; i < programLines.Length; i++)
            {
                enumerator = programLines[i].GetEnumerator();

                while (enumerator.MoveNext()) // Read the line char by char
                {
                    if (Char.IsWhiteSpace((char)enumerator.Current)) // Add the element to the FileTextData list
                    {
                        this.AddEntryToFileTextData();
                        continue;
                    }

                    // Check special cases
                    if ((Char.IsPunctuation((char)enumerator.Current) || Char.IsSymbol((char)enumerator.Current))
                        && !((char)enumerator.Current).Equals('_'))
                    {
                        // Detect double-character symbols
                        if (stringBuilder.Length == 1 && (Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0]))
                            && this.DetectDoubleCharacter((char)stringBuilder.ToString()[0], (char)enumerator.Current))
                        {
                            stringBuilder.Append(enumerator.Current);
                            this.AddEntryToFileTextData();
                            continue;
                        }
                        this.AddEntryToFileTextData();
                    }
                    else if (stringBuilder.Length == 1 && !((char)stringBuilder.ToString()[0]).Equals('_')
                            && (Char.IsPunctuation((char)stringBuilder.ToString()[0]) || Char.IsSymbol((char)stringBuilder.ToString()[0])))
                        this.AddEntryToFileTextData();

                    stringBuilder.Append(enumerator.Current);
                }

                this.AddEntryToFileTextData();
                programFile.FileTextData.Add(" "); // Marker for a new line
            }
        }

        /* Adds the current string in the StringBuilder to the FileTextData list, then clears the StringBuilder */
        private void AddEntryToFileTextData()
        {
            if (stringBuilder.Length > 0)
            {
                programFile.FileTextData.Add(stringBuilder.ToString());
                stringBuilder.Clear();
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

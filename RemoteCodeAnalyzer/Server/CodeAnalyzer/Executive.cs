using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    /* Controls the flow of execution through the application */
    public class Executive
    {
        // Central data objects
        //private readonly InputSessionData inputSessionData = new InputSessionData();
        //private readonly CodeAnalysisData codeAnalysisData = new CodeAnalysisData();

        /* Creates the main executive objects to perform all major tasks and activities, limiting access to central data */
        public void StartCodeAnalysis()
        {
            //InputReader inputReader = new InputReader();
            //OutputWriter outputWriter = new OutputWriter();
            //int numberOfFiles;

            /* 1: Parse the input arguments */
            /*if (!inputReader.FormatInput(args)) // If input is invalid, terminate the program
            {
                Console.WriteLine("\nProgram could not be executed with the given input." + inputReader.ErrorMessage);
                return;
            }*/

            /* 2: Set the session data from input: directory path, file type, and options /S, /R, /X */
            //this.inputSessionData.SetInputSessionData(inputReader.FormattedInput);

            /* 3: Create file objects and read all files, enqueue them on the FileQueue */
            //this.inputSessionData.EnqueueFiles();
            //numberOfFiles = this.inputSessionData.FileQueue.Count();

            /* 4: Pre-process the text from each file on the FileQueue into lists of logical "words" */
            /*for (int i = 0; i < numberOfFiles; i++)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new FileProcessor(programFile).ProcessFile();
                this.inputSessionData.FileQueue.Enqueue(programFile);   // Enqueue the file again for secondary processing
            }*/

            /* 5: Use pre-processed text to establish the hierarchy of types and collect function data for each file on FileQueue */
            /*while (this.inputSessionData.FileQueue.Count > 0)
            {
                ProgramFile programFile = this.inputSessionData.FileQueue.Dequeue();
                new CodeProcessor(programFile, this.codeAnalysisData.ProgramClassTypes, this.inputSessionData.FileType).ProcessFileCode();
                this.codeAnalysisData.ProcessedFiles.Add(programFile);   // Add the file to the list of processed files
            }*/

            /* 6: Collect relationship data for each class and interface */
            //foreach (ProgramClassType programClassType in this.codeAnalysisData.ProgramClassTypes)
            //    new RelationshipProcessor(programClassType, this.codeAnalysisData.ProgramClassTypes).ProcessRelationships();

            /* 7: Print the requested code analysis data to standard output and/or XML file */
            //outputWriter.WriteOutput(this.codeAnalysisData.ProcessedFiles, this.inputSessionData.DirectoryPath, this.inputSessionData.FileType, this.inputSessionData.PrintToXml, this.inputSessionData.SetRelationshipData);
        }
    }
}

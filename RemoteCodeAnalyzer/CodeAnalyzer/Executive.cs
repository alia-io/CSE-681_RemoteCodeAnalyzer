///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  Executive.cs - Entry point to the code analyzer, controls the flow of execution    ///
///                                                                                     ///
///  Language:      C# .Net Framework 4.7.2, Visual Studio 2019                         ///
///  Platform:      Dell G5 5090, Intel Core i7-9700, 16GB RAM, Windows 10              ///
///  Application:   RemoteCodeAnalyzer - Project #4 for CSE 681:                        ///
///                 Software Modeling and Analysis, 2021                                ///
///  Author:        Alifa Stith, Syracuse University, astith@syr.edu                    ///
///                                                                                     ///
///////////////////////////////////////////////////////////////////////////////////////////

/*
 *   Module Operations
 *   -----------------
 *   This module provides the entry point into the code analyzer application, used to
 *   analyze the code of files that are part of an application. It also provides storage
 *   of thread-safe central data objects, which are passed to other modules as needed.
 *   To increase throughput, code analyzer is multithreaded and mainly uses blocking queues
 *   as a data storage interface between analysis steps.
 * 
 *   Public Interface
 *   ----------------
 *   Executive executive = new Executive((string) directoryPath, (string) project, (string) version);
 *   bool success = executive.PerformCodeAnalysis((List<XElement>) fileNames);
 */

using System;
using System.IO;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CodeAnalyzer
{
    /* Stores central data and controls the flow of execution through the application */
    public class Executive
    {
        /* Input data */
        private int numberOfFiles;
        private readonly string directoryPath;
        private readonly string project;
        private readonly string version;

        /* Queues and saved data for intermediate processing */
        private SizeLimitedBlockingQueue<string> inputFiles;
        private SizeLimitedBlockingQueue<string> parsedFiles;
        private readonly ProgramClassTypeCollection programClassTypes = new ProgramClassTypeCollection();

        /* Output data */
        private readonly ConcurrentOrderedList processedFiles = new ConcurrentOrderedList();

        public Executive(string directoryPath, string project, string version)
        {
            this.directoryPath = directoryPath;
            this.project = project;
            this.version = version;
        }

        /* Creates the main executive objects to perform all major tasks and activities, limiting access to private data members */
        public bool PerformCodeAnalysis(List<XElement> fileNames)
        {
            ThreadPool.SetMaxThreads(24, 24);

            // Set input data
            numberOfFiles = fileNames.Count;
            inputFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);
            parsedFiles = new SizeLimitedBlockingQueue<string>(numberOfFiles);

            // Create temp directory to store intermediate data
            try
            {
                Directory.CreateDirectory(directoryPath + "\\temp");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create temp directory.\n{0}", e.ToString());
                return false;
            }

            // Analyze the code
            _ = EnqueueInputFiles(fileNames); // Task to enqueue the input fileNames into the inputFiles queue
            _ = ParseInputFiles(); // Task to dequeue inputFiles, parse them, and enqueue them into parsedFiles queue
            AnalyzeParsedFiles(); // Dequeue parsedFiles and analyze them, establishing hierarchy of types and collecting function data
            Task analyzer = AnalyzeAllRelationships(); // Task to analyze relationship data

            // Write the output
            Task writer = WriteFunctionAnalysis(); // Task to function analysis XML file
            analyzer.Wait();
            RelationshipAnalysisWriter.WriteOutput(processedFiles, directoryPath, project, version); // Write relationship analysis XML file
            writer.Wait(); // Wait for all above tasks to complete
            AnalysisMetadataWriter.WriteMetadata(directoryPath); // Write metadata file to store "severity" of analysis elements for GUI to display

            // Remove the temp directory
            try
            {
                Directory.Delete(directoryPath + "\\temp", true);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to delete temp directory.\n{0}", e.ToString());
            }

            return true;
        }

        /* Task enqueues files onto the inputFiles queue */
        private async Task EnqueueInputFiles(List<XElement> fileNames)
        {
            await Task.Run(() =>
            {
                foreach (XElement file in fileNames)
                    inputFiles.Enqueue(file.Attribute("name").Value + "." + file.Attribute("type").Value);
            });
        }

        /* Task dequeues files from inputFiles queue, then concurrently sends them to TextParser for preprocessing and enqueues them onto parsedFiles queue */
        private async Task ParseInputFiles()
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < numberOfFiles; i++)
                {
                    string fileName = inputFiles.Dequeue();
                    _ = Task.Run(() =>
                    {
                        new TextParser(directoryPath, fileName).ProcessFile();
                        parsedFiles.Enqueue(fileName);
                    });
                }
            });
        }

        /* Task dequeues files from parsedFiles queue, then concurrently sends them to FileAnalyzer for processing and adds them onto processedFiles list */
        private void AnalyzeParsedFiles()
        {
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < numberOfFiles; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    string filename = parsedFiles.Dequeue();
                    ProgramFile file = new ProgramFile(filename, directoryPath);
                    new FileAnalyzer(file, programClassTypes).ProcessFileCode();
                    processedFiles.Add(file);
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        /* Task runs relationship analysis concurrently on each class/interface in programClassTypes collection to add its relationships */
        private async Task AnalyzeAllRelationships()
        {
            await Task.Run(() =>
            {
                List<Task> tasks = new List<Task>();
                foreach (ProgramClassType programClassType in programClassTypes)
                {
                    ProgramClassType current = programClassType;
                    tasks.Add(Task.Run(() => new ClassAnalyzer(current, programClassTypes).ProcessRelationships()));
                }
                Task.WaitAll(tasks.ToArray());
            });
        }

        /* Task sends the data to write the function analysis XML file */
        private async Task WriteFunctionAnalysis() =>
            await Task.Run(() => FunctionAnalysisWriter.WriteOutput(processedFiles, directoryPath, project, version));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    //public enum ControlFlowType { _if, _elseif, _else, _for, _foreach, _while, _dowhile, _switch };

    /* Processor of the file data (except relationship data), filling all internal Child ProgramType lists */
    public class FileAnalyzer
    {
        /* Saved input data */
        private readonly ProgramClassTypeCollection programClassTypes;
        private readonly ProgramFile programFile;
        private readonly string fileType;

        /* Stacks to keep track of the current scope */
        private readonly Stack<string> scopeStack = new Stack<string>();
        private readonly Stack<ProgramType> typeStack = new Stack<ProgramType>();

        /* Reader and saved input text */
        StreamReader reader;
        private int currentLine = 0;
        private readonly List<string> currentText = new List<string>();

        /* Scope syntax rules to check for */
        private readonly List<ControlFlowScopeRule> activeRules = new List<ControlFlowScopeRule>();

        public FileAnalyzer(ProgramFile programFile, ProgramClassTypeCollection programClassTypes)
        {
            this.programClassTypes = programClassTypes;
            this.programFile = programFile;
            fileType = programFile.FileType;
        }

        /* Analyzes all of the code outside of a class or interface */
        public void ProcessFileCode()
        {
            string entry;

            using (reader = File.OpenText(programFile.DirectoryPath + "\\" + programFile.Name + "." + programFile.FileType + ".txt"))
            {
                while (!reader.EndOfStream)
                {
                    entry = reader.ReadLine();

                    CheckIfNewLine(entry); // Maintain the file's line number

                    // Determine whether to ignore the entry (if it's part of a comment or string)
                    if (IgnoreEntry(entry)) continue;

                    currentText.Add(entry); // Add entry to current text list

                    if (entry.Equals("}")) // Check for the end of an existing bracketed scope
                    {
                        EndBracketedScope(null, "");
                        continue;
                    }

                    if (entry.Equals("namespace")) // Check for a new namespace
                    {
                        CreateNewNamespace();
                        continue;
                    }

                    if (entry.Equals("class") || entry.Equals("interface")) // Check for a new class or interface
                    {
                        CreateNewProgramClassType(entry);
                        continue;
                    }

                    if (entry.Equals("{")) // Push scope opener onto scopeStack
                        scopeStack.Push(entry);

                    UpdateCurrentText(null, entry);
                }
            }
        }

        /* Processes data within a ProgramClassType (class or interface) scope but outside of a function */
        private void ProcessProgramClassTypeData(string type)
        {
            string entry;
            bool newLine;

            if (!Directory.Exists(typeStack.Peek().DirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(typeStack.Peek().DirectoryPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to create temp subdirectory: {0}", e.ToString());
                }
            }

            using (StreamWriter writer = File.CreateText(typeStack.Peek().DirectoryPath + "\\" + typeStack.Peek().Name + ".txt"))
            {
                ClearCurrentItems(writer); // Add saved text to ProgramDataType's temp file & clear the list

                while (!reader.EndOfStream)
                {
                    entry = reader.ReadLine();

                    newLine = CheckIfNewLine(entry); // Maintain the file's line number

                    // Determine whether to ignore the entry (if it's part of a comment or string)
                    if (IgnoreEntry(entry)) continue;

                    // Add entry to current text list to be written to ProgramDataType's temp file
                    currentText.Add(entry);

                    if (entry.Equals("}")) // Check for the end of an existing bracketed scope
                    {
                        if (EndBracketedScope(writer, type)) return;
                        continue;
                    }

                    if (entry.Equals("class") || entry.Equals("interface")) // Check for a new class or interface
                    {
                        CreateNewProgramClassType(entry);
                        continue;
                    }

                    if (entry.Equals("{"))
                    {
                        if (CheckIfFunction()) // Check if new function is being started
                        {
                            scopeStack.Push(entry);
                            ProcessFunctionData();
                            continue;
                        }
                        else // Push scope opener onto scopeStack
                            scopeStack.Push(entry);
                    }

                    if (entry.Equals("=>"))
                    {
                        if (CheckIfFunction()) // Check if a new lambda function is being started
                        {
                            ProcessFunctionData();
                            continue;
                        }
                    }

                    UpdateCurrentText(writer, entry);
                }
            }
        }

        /* Processes data within a Function scope */
        private void ProcessFunctionData()
        {
            string entry;
            bool scopeOpener;
            bool beginningOfStream = true;
            bool newLine;

            if (!Directory.Exists(typeStack.Peek().DirectoryPath))
            {
                try
                {
                    Directory.CreateDirectory(typeStack.Peek().DirectoryPath);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unable to create temp subdirectory: {0}", e.ToString());
                }
            }

            using (StreamWriter writer = File.CreateText(typeStack.Peek().DirectoryPath + "\\" + typeStack.Peek().Name + ".txt"))
            {
                while (!reader.EndOfStream)
                {
                    entry = reader.ReadLine();
                    scopeOpener = false;

                    newLine = CheckIfNewLine(entry); // Maintain the file's line number

                    // Determine whether to ignore the entry (if it's part of a comment or string)
                    if (IgnoreEntry(entry)) continue;

                    currentText.Add(entry); // Add entry to current text list to be written to Function's temp file

                    if (!beginningOfStream && newLine) IncrementFunctionSize(); // Update function data if new line (except at beginning of function)
                    beginningOfStream = false;

                    // Check for some closing scopes: ")", "}", ";"
                    if (CheckScopeClosersWithinFunction(writer, entry)) return; // Closing scope was the end of this function

                    // Check control flow scope openers
                    if (!newLine && typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction) && CheckControlFlowScopes(writer, entry))
                        scopeOpener = true;

                    CheckScopeOpenersWithinFunction(entry, scopeOpener); // Check for some opening scopes: "(", "{", "=>"

                    UpdateCurrentText(writer, entry);
                }
            }
        }

        /* Creates a new namespace object and adds it as a child to the current type */
        private void CreateNewNamespace()
        {
            ProgramNamespace newNamespace;
            int line = currentLine;
            string entry;
            string name = "";
            bool newLine;

            scopeStack.Push("namespace"); // Push the namespace scope opener onto scopeStack

            while (!reader.EndOfStream) // Get the name of the namespace
            {
                entry = reader.ReadLine();

                newLine = CheckIfNewLine(entry); // Maintain the file's line number

                if (entry.Equals("{"))
                {
                    scopeStack.Push("{"); // Push the new scope opener onto scopeStack
                    break;
                }

                if (!newLine)
                {
                    name += entry;
                    line = currentLine;
                }
            }

            if (typeStack.Count > 0) newNamespace = new ProgramNamespace(typeStack.Peek(), name, line);
            else newNamespace = new ProgramNamespace(programFile, name, line);

            ClearCurrentItems(null);
            typeStack.Push(newNamespace);
        }

        /* Creates a new class or interface object, adds it as a child to the current type, and sends it to its analyzer */
        private void CreateNewProgramClassType(string type)
        {
            ProgramClassType programClassType;

            // Gather the ProgramClassType data
            GetClassTypeData(type, out ProgramType parent, out string name, out int line, out List<string> modifiers, out List<string> generics);

            if (programClassTypes.Contains(name))
            {
                if (GetUniqueNames(parent, programClassTypes[name].Parent, name, out string currentName, out string otherName))
                {
                    programClassTypes[name].Name = otherName;
                    name = currentName;
                }
                else
                {
                    Console.WriteLine("\n\nError: Cannot determine data for two classes with the same name.\n\n");
                    return;
                }
            }

            // Create the new class or interface object
            if (type.Equals("class")) programClassType = new ProgramClass(programClassTypes, parent, name, line, modifiers, generics);
            else programClassType = new ProgramInterface(programClassTypes, parent, name, line, modifiers, generics);

            typeStack.Push(programClassType);

            ProcessProgramClassTypeData(type); // Send to method to analyze inside of a class/interface
        }

        /* If there is a name conflict in ProgramClassTypes collection, try to set unique names */
        private bool GetUniqueNames(ProgramType currentParent, ProgramType otherParent, string name, out string currentName, out string otherName)
        {
            List<string> currentParentNames = new List<string>();
            List<string> otherParentNames = new List<string>();
            int index;

            currentName = name;
            otherName = name;

            while (currentParent != null)
            {
                currentParentNames.Add(currentParent.Name);
                currentParent = currentParent.Parent;
            }

            while (otherParent != null)
            {
                otherParentNames.Add(otherParent.Name);
                otherParent = otherParent.Parent;
            }

            for (index = 0; index < currentParentNames.Count && index < otherParentNames.Count; index++)
            {
                currentName = currentParentNames[index] + "." + currentName;
                otherName = otherParentNames[index] + "." + otherName;

                if (!currentName.Equals(otherName)) return true;
            }

            if (currentParentNames.Count > otherParentNames.Count)
            {
                currentName = currentParentNames[index] + "." + currentName;
                return true;
            }

            if (otherParentNames.Count > currentParentNames.Count)
            {
                otherName = otherParentNames[index] + "." + otherName;
                return true;
            }

            return false;
        }

        /* Creates a new function object and adds it as a child to the current type */
        private void NewFunction(string name, int line, List<string> modifiers, List<string> returnTypes, List<string> generics, List<string> parameters, List<string> baseParameters)
        {
            ProgramType parent = programFile;

            ClearCurrentItems(null);

            if (typeStack.Count > 0) parent = typeStack.Peek(); // Get the parent

            ProgramFunction programFunction = new ProgramFunction(parent, name, line, modifiers, returnTypes, generics, parameters, baseParameters);

            // Add the function and scope to scopeStack
            scopeStack.Push("function");

            typeStack.Push(programFunction);
        }

        /* Finds all data required to create a new class or interface */
        private void GetClassTypeData(string type, out ProgramType parent, out string name, out int line, out List<string> modifiers, out List<string> generics)
        {
            string entry;
            bool newLine;
            parent = programFile;
            name = "";
            line = currentLine;
            modifiers = new List<string>();
            generics = new List<string>();
            int brackets = 0;

            if (typeStack.Count > 0) parent = typeStack.Peek(); // Get the parent

            foreach (string modifier in currentText) // Get the modifiers
            {
                if (!modifier.Equals("class") && !modifiers.Equals(" "))
                    modifiers.Add(modifier);
            }

            scopeStack.Push(type); // Push the type of scope opener (class or interface)
            ClearCurrentItems(null);

            while (!reader.EndOfStream)
            {
                entry = reader.ReadLine();

                newLine = CheckIfNewLine(entry); // Maintain the file's line number

                // Determine whether to ignore the entry (if it's part of a comment or string)
                if (IgnoreEntry(entry)) continue;

                currentText.Add(entry); // Add entry to list to be added to class's/interface's temp file

                if (entry.Equals("{"))
                {
                    scopeStack.Push(entry); // Push the scope opener bracket
                    break;
                }

                if (entry.Equals("<")) brackets++;
                else if (entry.Equals(">")) brackets--;
                else if (brackets > 0) generics.Add(entry); // Save any generic types

                if (name.Length == 0) // The next entry after "class" or "interface" will be the name
                {
                    if (!newLine)
                    {
                        name = entry;
                        line = currentLine;
                    }
                }
            }
        }

        /* Detects the syntax for a normal function signature */
        private bool CheckIfFunction()
        {
            // The function requirement to check next. If this ends at 4 or 7, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            int line = currentLine;
            List<string> modifiers = new List<string>();
            List<string> returnTypes = new List<string>();
            List<string> generics = new List<string>();
            List<string> parameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int squareBrackets = 0;
            int angleBrackets = 0;

            foreach (string text in currentText)
            {
                if (text.Equals("(")) parentheses++;
                else if (text.Equals("[")) squareBrackets++;
                else if (text.Equals("<")) angleBrackets++;

                // Test the current requirement
                functionRequirement = TestFunctionRequirement(functionRequirement, text, ref name, ref modifiers, ref returnTypes, ref generics, ref parameters, squareBrackets, angleBrackets, parentheses);
                if (functionRequirement == -1) break;

                if (text.Equals(")")) parentheses--;
                else if (text.Equals("]")) squareBrackets--;
                else if (text.Equals(">")) angleBrackets--;
            }

            if (functionRequirement == 4 || functionRequirement == 7) // Function signature detected - create a new function
            {
                for (int i = currentText.FindIndex(text => text.Equals(name)) + 1; i < currentText.Count; i++)
                    if (currentText[i].Equals(" ")) line--;

                NewFunction(name, line, modifiers, returnTypes, generics, parameters, new List<string>());
                return true;
            }
            // If it failed normal function requirements, check rules for constructors and deconstructors
            else if ((fileType.Equals("cs") || fileType.Equals("txt")) && CheckIfConstructor_cs()) return true;
            else if (fileType.Equals("java") && CheckIfConstructor_java()) return true;
            else if (CheckIfDeconstructor()) return true;

            return false;
        }

        /* Detects the syntax for a C# Constructor function */
        private bool CheckIfConstructor_cs()
        {
            // The constructor requirement to check next. If this ends at 3 or 7, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            int line = currentLine;
            List<string> modifiers = new List<string>();
            List<string> parameters = new List<string>();
            List<string> baseParameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int brackets = 0;
            int periods = 0; // Used for formatting

            foreach (string text in currentText)
            {
                if (text.Equals("(")) parentheses++;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("[") || text.Equals("<")) brackets++;
                else if (text.Equals(".")) periods++;

                // Test the current requirement
                functionRequirement = TestConstructorRequirement_cs(functionRequirement, text, ref modifiers, ref name, ref parameters, ref baseParameters, brackets, parentheses, periods);
                if (functionRequirement == -1) break;

                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals("]") || text.Equals(">")) brackets--;
            }

            if (functionRequirement == 3 || functionRequirement == 7) // Constructor signature detected - create a new function
            {
                for (int i = currentText.FindIndex(text => text.Equals(name)) + 1; i < currentText.Count; i++)
                    if (currentText[i].Equals(" ")) line--;

                NewFunction(name, line, modifiers, new List<string>(), new List<string>(), parameters, baseParameters);
                return true;
            }

            return false;
        }

        /* Detects the syntax for a Java Constructor function */
        private bool CheckIfConstructor_java()
        {
            // The constructor requirement to check next. If this ends at ???, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            int line = currentLine;
            List<string> modifiers = new List<string>();
            List<string> parameters = new List<string>();

            // Ensure the same number of opening and closing parentheses/brackets
            int parentheses = 0;
            int squareBrackets = 0;
            int angleBrackets = 0;
            int periods = 0;

            foreach (string text in currentText)
            {
                if (text.Equals("(")) parentheses++;
                else if (text.Equals("[")) squareBrackets++;
                else if (text.Equals("<")) angleBrackets++;
                else if (text.Equals(".")) periods++;

                // Test the current requirement
                functionRequirement = TestConstructorRequirement_java(functionRequirement, text, ref name, ref modifiers, ref parameters, squareBrackets, angleBrackets, parentheses, periods);
                if (functionRequirement == -1) break;

                if (periods > 0 && !text.Equals(".")) periods--;
                else if (text.Equals(")")) parentheses--;
                else if (text.Equals("]")) squareBrackets--;
                else if (text.Equals(">")) angleBrackets--;
            }

            if (functionRequirement == 4 || functionRequirement == 7) // Function signature detected - create a new function
            {
                for (int i = currentText.FindIndex(text => text.Equals(name)) + 1; i < currentText.Count; i++)
                    if (currentText[i].Equals(" ")) line--;

                NewFunction(name, line, modifiers, new List<string>(), new List<string>(), parameters, new List<string>());
                return true;
            }

            return false;
        }

        /* Detects the syntax for a Deconstructor function */
        private bool CheckIfDeconstructor()
        {
            // The deconstructor requirement to check next. If this ends at 4, there is a new function. If this ends at -1, there is not a new function.
            int functionRequirement = 0;

            string name = "";
            int line = currentLine;

            foreach (string text in currentText)
            {
                // Test the current requirement
                functionRequirement = TestDeconstructorRequirement(functionRequirement, text, ref name);
                if (functionRequirement == -1) break;
            }

            if (functionRequirement == 4) // Deconstructor signature detected - create a new function
            {
                for (int i = currentText.FindIndex(text => text.Equals(name)) + 1; i < currentText.Count; i++)
                    if (currentText[i].Equals(" ")) line--;

                NewFunction(name, line, new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>());
                return true;
            }

            return false;
        }

        /* Tests each step of normal function requirements */
        private int TestFunctionRequirement(int functionRequirement, string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> generics, ref List<string> parameters, int squareBrackets, int angleBrackets, int parentheses)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find text entry that could be a return type.
                    functionRequirement = FunctionStep0(text, ref returnTypes, parentheses);
                    break;
                case 1: // To pass: Find text entry that could be a name.
                    functionRequirement = FunctionStep1(text, ref name, ref modifiers, ref returnTypes, parentheses, angleBrackets + squareBrackets);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = FunctionStep2(text, ref name, ref modifiers, ref returnTypes, ref generics, squareBrackets, angleBrackets);
                    break;
                case 3: // To pass: Find closing parenthesis.
                    functionRequirement = FunctionStep3(text, ref parameters, parentheses);
                    break;
                case 4: // To pass: Find no more text after closing parenthesis, or continue to test for function with multiple return types.
                    functionRequirement = FunctionStep4(text, ref name, ref modifiers, ref returnTypes, ref parameters);
                    break;
                case 5: // To pass: Find opening parenthesis.
                    functionRequirement = FunctionStep5(text, ref generics, angleBrackets);
                    break;
                case 6: // To pass: Find closing parenthesis.
                    functionRequirement = FunctionStep6(text, ref parameters, parentheses);
                    break;
                case 7: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = FunctionStep7(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests each step of C# constructor function requirements */
        private int TestConstructorRequirement_cs(int functionRequirement, string text, ref List<string> modifiers, ref string name, ref List<string> parameters, ref List<string> baseParameters, int brackets, int parentheses, int periods)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find text entry that could be a name.
                    functionRequirement = ConstructorStep0_cs(text, ref name);
                    break;
                case 1: // To pass: Find opening parenthesis.
                    functionRequirement = ConstructorStep1_cs(text, ref modifiers, ref name, brackets, periods);
                    break;
                case 2: // To pass: Find closing parenthesis.
                    functionRequirement = ConstructorStep2_cs(text, ref parameters, parentheses);
                    break;
                case 3: // To continue: Find colon.
                    functionRequirement = ConstructorStep3_cs(text, ref baseParameters);
                    break;
                case 4: // To pass: Find "base".
                    functionRequirement = ConstructorStep4_cs(text, ref baseParameters);
                    break;
                case 5: // To pass: Find opening parenthesis.
                    functionRequirement = ConstructorStep5_cs(text, ref baseParameters);
                    break;
                case 6: // To pass: Find closing parenthesis.
                    functionRequirement = ConstructorStep6_cs(text, ref baseParameters);
                    break;
                case 7: // To pass: Function name == class name, and find no more text after closing parenthesis.
                    functionRequirement = ConstructorStep7_cs(text, name);
                    break;
            }
            return functionRequirement;
        }

        /* Tests each step of Java constructor function requirements */
        private int TestConstructorRequirement_java(int functionRequirement, string text, ref string name, ref List<string> modifiers, ref List<string> parameters, int squareBrackets, int angleBrackets, int parentheses, int periods)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: The name of function equals name of the class.
                    functionRequirement = ConstructorStep0_java(text, ref name);
                    break;
                case 1: // To pass: Find opening parenthesis.
                    functionRequirement = ConstructorStep1_java(text, ref name, ref modifiers, squareBrackets + angleBrackets, periods);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = ConstructorStep2_java(text, ref parameters, parentheses);
                    break;
                case 3: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = ConstructorStep3_java(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests each step of deconstructor requirements */
        private int TestDeconstructorRequirement(int functionRequirement, string text, ref string name)
        {
            switch (functionRequirement)
            {
                case 0: // To pass: Find tilde.
                    functionRequirement = DeconstructorStep0(text, ref name);
                    break;
                case 1: // To pass: The name of function equals name of the class.
                    functionRequirement = DeconstructorStep1(text, ref name);
                    break;
                case 2: // To pass: Find opening parenthesis.
                    functionRequirement = DeconstructorStep2(text);
                    break;
                case 3: // To pass: Find closing parenthesis.
                    functionRequirement = DeconstructorStep3(text);
                    break;
                case 4: // To pass: Find no more text after closing parenthesis.
                    functionRequirement = DeconstructorStep4(text);
                    break;
            }
            return functionRequirement;
        }

        /* Tests step 0 of function syntax: Find text entry that could be a return type (1 = success, -1 = fail) */
        private int FunctionStep0(string text, ref List<string> returnTypes, int parentheses)
        {
            if (text.Equals(" ")) return 0;

            if (parentheses > 0) // Function has multiple return types
            {
                if (parentheses == 1 && text.Equals(")")) return 1;
                if (!text.Equals("(")) returnTypes.Add(text);
                return 0;
            }

            if ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                returnTypes.Add(text);
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of function syntax: Find text entry that could be a name (2 = success, -1 = fail) */
        private int FunctionStep1(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, int parentheses, int brackets)
        {
            if (text.Equals(" ")) return 1;

            if (parentheses > 0) // Function has multiple return types
            {
                if (text.Equals("("))
                {
                    foreach (string entry in returnTypes)
                        modifiers.Add(entry);
                    returnTypes.Clear();
                }
                else if (!text.Equals(")")) returnTypes.Add(text);
                return 1;
            }

            if (brackets > 0 || text.Equals(".") || (returnTypes.Count > 0 && returnTypes[returnTypes.Count - 1].Equals("."))) // Part of return type
            {
                returnTypes.Add(text);
                return 1;
            }

            if ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                name = text;
                return 2;
            }

            return -1;
        }

        /* Tests step 2 of function syntax: Find opening parenthesis (3 = success, -1 = fail) */
        private int FunctionStep2(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> generics, int squareBrackets, int angleBrackets)
        {
            if (text.Equals(" ")) return 2;

            if (angleBrackets > 0) // Function has a generic type attached
            {
                if (!text.Equals("<") && !text.Equals(">")) generics.Add(text);
            }

            else if (squareBrackets > 0 || text.Equals(".") || (returnTypes.Count > 0 && returnTypes[returnTypes.Count - 1].Equals("."))) // Still reading return type
            {
                if (text.Equals("[") || (squareBrackets == 0 && returnTypes.Count == 1 && name.Length > 0))
                {
                    foreach (string entry in returnTypes) modifiers.Add(entry);
                    returnTypes.Clear();
                    returnTypes.Add(name);
                    name = "";
                }
                returnTypes.Add(text);
            }

            else if ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                if (name.Length == 0) // Need to find name again
                    name = text;
                else if (returnTypes.Count == 1) // Shift everything for a new name
                {
                    foreach (string entry in returnTypes) modifiers.Add(entry);
                    returnTypes.Clear();
                    returnTypes.Add(name);
                    name = text;
                }
            }

            else if (text.Equals("(")) return 3;

            else return -1;

            return 2;
        }

        /* Tests step 3 of function syntax: Find closing parenthesis (4 = success) */
        private int FunctionStep3(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(")") && parentheses == 1)
                return 4;

            parameters.Add(text);
            return 3;
        }

        /* Tests step 4 of function syntax: Find no more text after closing parenthesis except { or =>, or (for multiple return types) find a new name (5 = pass to multiple return types, -1 = fail) */
        private int FunctionStep4(string text, ref string name, ref List<string> modifiers, ref List<string> returnTypes, ref List<string> parameters)
        {
            if (text.Equals(" ") || text.Equals("{") || text.Equals("=>")) return 4;

            // Check for a function signature with multiple return types
            if (parameters.Count > 1 && returnTypes.Count > 0 && ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || text[0].Equals('_')))
            {
                foreach (string entry in returnTypes) modifiers.Add(entry);
                modifiers.Add(name);
                returnTypes.Clear();
                name = text;
                foreach (string entry in parameters) returnTypes.Add(entry);
                parameters.Clear();
                return 5;
            }

            return -1;
        }

        /* Tests step 5 of function syntax: Find opening parenthesis (6 = success, -1 = fail) */
        private int FunctionStep5(string text, ref List<string> generics, int angleBrackets)
        {
            if (text.Equals(" ")) return 5;

            if (angleBrackets > 0) // Function has a generic type attached
            {
                if (!text.Equals("<") && !text.Equals(">")) generics.Add(text);
                return 5;
            }

            if (text.Equals("(")) return 6;

            return -1;
        }

        /* Tests step 6 of function syntax: Find closing parenthesis (7 = success) */
        private int FunctionStep6(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 6;

            if (text.Equals(")") && parentheses == 1) return 7;

            parameters.Add(text);
            return 6;
        }

        /* Tests step 7 of function syntax: Find no more text after closing parenthesis except { or => (-1 = fail) */
        private int FunctionStep7(string text)
        {
            if (text.Equals(" ") || text.Equals("{") || text.Equals("=>")) return 7;
            return -1;
        }

        /* Tests step 0 of constructor syntax (C#): Find text entry that could be a name (2 = success, -1 = fail) */
        private int ConstructorStep0_cs(string text, ref string name)
        {
            if (text.Equals(" ")) return 0;

            if ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || text[0].Equals('_'))
            {
                name = text;
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of constructor syntax (C#): Find opening parenthesis (2 = success, -1 = fail) */
        private int ConstructorStep1_cs(string text, ref List<string> modifiers, ref string name, int brackets, int periods)
        {
            if (text.Equals(" ")) return 1;

            if (text.Equals("("))
            {
                if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(name))
                    return 2;
                return -1;
            }

            if (brackets == 0 && periods == 0 && (!char.IsSymbol((char)text[0]) && !char.IsPunctuation((char)text[0])) || ((char)text[0]).Equals('_'))
            {
                modifiers.Add(name);
                name = text;
                return 1;
            }

            if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals(">") || text.Equals("]"))
            {
                name += text;
                return 1;
            }

            return -1;
        }

        /* Tests step 2 of constructor syntax (C#): Find closing parenthesis (-1 = fail) */
        private int ConstructorStep2_cs(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals(")") && parentheses == 0)
                return 3;

            parameters.Add(text);
            return 2;
        }

        /* Tests step 3 of constructor syntax (C#): Find colon (4 = success) */
        private int ConstructorStep3_cs(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(":"))
            {
                baseParameters.Add(text);
                return 4;
            }

            return -1;
        }

        /* Tests step 4 of constructor syntax (C#): Find "base" (5 = success, -1 = fail) */
        private int ConstructorStep4_cs(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 4;

            if (text.Equals("base"))
            {
                baseParameters.Add(text);
                return 5;
            }

            return -1;
        }

        /* Tests step 5 of constructor syntax (C#): Find opening parenthesis (6 = success, -1 = fail) */
        private int ConstructorStep5_cs(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 5;

            if (text.Equals("("))
            {
                baseParameters.Add(text);
                return 6;
            }

            return -1;
        }

        /* Tests step 6 of constructor syntax (C#): Find closing parenthesis (7 = success) */
        private int ConstructorStep6_cs(string text, ref List<string> baseParameters)
        {
            if (text.Equals(" ")) return 6;

            if (text.Equals(")"))
            {
                baseParameters.Add(text);
                return 7;
            }

            baseParameters.Add(text);
            return 6;
        }

        /* Tests step 7 of constructor syntax (C#): Function name == class name, and find no more text after closing parenthesis except { or => (-1 = fail) */
        private int ConstructorStep7_cs(string text, string name)
        {
            if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(name)
                && (text.Equals(" ") || text.Equals("{") || text.Equals("=>"))) return 7;
            return -1;
        }

        /* Tests step 0 of constructor syntax (Java): The name of function equals name of the class (1 = success, -1 = fail) */
        private int ConstructorStep0_java(string text, ref string name)
        {
            if (text.Equals(" ")) return 0;

            if ((!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || (text[0]).Equals('_'))
            {
                name = text;
                if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction) && typeStack.Peek().Name.Equals(name))
                    return 1;
            }

            return -1;
        }

        /* Tests step 2 of constructor syntax (Java): Find opening parenthesis (3 = success, -1 = fail) */
        private int ConstructorStep1_java(string text, ref string name, ref List<string> modifiers, int brackets, int periods)
        {
            if (text.Equals(" ")) return 1;

            if (text.Equals("("))
            {
                if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(name))
                    return 2;
                return -1;
            }

            if (brackets == 0 && periods == 0 && (!char.IsSymbol(text[0]) && !char.IsPunctuation(text[0])) || (text[0]).Equals('_'))
            {
                modifiers.Add(name);
                name = text;
                return 1;
            }

            if (brackets != 0 || periods != 0 || text.Equals(".") || text.Equals(">") || text.Equals("]"))
            {
                name += text;
                return 1;
            }

            return -1;
        }

        /* Tests step 2 of constructor syntax (Java): Find closing parenthesis (3 = success) */
        private int ConstructorStep2_java(string text, ref List<string> parameters, int parentheses)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals(")") && parentheses == 1)
                return 3;

            parameters.Add(text);
            return 2;
        }

        /* Tests step 3 of constructor syntax (Java): Find no more text after closing parenthesis except { or => (-1 = fail) */
        private int ConstructorStep3_java(string text)
        {
            if (text.Equals(" ") || text.Equals("{") || text.Equals("=>")) return 3;
            return -1;
        }

        /* Tests step 0 of deconstructor syntax: Find tilde (1 = success, -1 = fail) */
        private int DeconstructorStep0(string text, ref string name)
        {
            if (text.Equals(" ")) return 0;

            if (text.Equals("~"))
            {
                name = text;
                return 1;
            }

            return -1;
        }

        /* Tests step 1 of deconstructor syntax: The name of function equals name of the class (2 = success, -1 = fail) */
        private int DeconstructorStep1(string text, ref string name)
        {
            if (text.Equals(" ")) return 1;

            if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramClass) && typeStack.Peek().Name.Equals(text))
            {
                name += text;
                return 2;
            }

            return -1;
        }

        /* Tests step 2 of deconstructor syntax: Find opening parenthesis (3 = success, -1 = fail) */
        private int DeconstructorStep2(string text)
        {
            if (text.Equals(" ")) return 2;

            if (text.Equals("("))
                return 3;

            return -1;
        }

        /* Tests step 3 of deconstructor syntax: Find closing parenthesis (4 = success, -1 = fail) */
        private int DeconstructorStep3(string text)
        {
            if (text.Equals(" ")) return 3;

            if (text.Equals(")"))
                return 4;

            return -1;
        }

        /* Tests step 4 of deconstructor syntax: Find no more text after closing parenthesis except { or => (-1 = fail) */
        private int DeconstructorStep4(string text)
        {
            if (text.Equals(" ") || text.Equals("{") || text.Equals("=>")) return 4;
            return -1;
        }

        /* Check for control flow scope openers within functions: if, else if, else, for, for each, while, do while, switch */
        private bool CheckControlFlowScopes(StreamWriter writer, string entry)
        {
            ControlFlowScopeRule newRule;
            bool scopeOpener = false;
            List<ControlFlowScopeRule> failedRules = new List<ControlFlowScopeRule>();

            foreach (ControlFlowScopeRule rule in activeRules)
                if (rule.IsPassed(entry, scopeStack.Count))
                {
                    ((ProgramFunction)typeStack.Peek()).Complexity++;
                    scopeStack.Push(rule.GetScopeType());
                    scopeOpener = true;
                }

            if (scopeOpener) ClearCurrentItems(writer);
            else activeRules.RemoveAll(rule => rule.Complete);

            newRule = ControlFlowScopeRuleFactory.GetRule(activeRules, entry, scopeStack.Count, fileType);
            if (newRule != null) activeRules.Add(newRule);

            return scopeOpener;
        }

        /* Maintains stacks when a bracketed scope ends; returns true if the type of scope is equal to scopeType */
        private bool EndBracketedScope(StreamWriter writer, string scopeType)
        {
            bool isScopeType = false;

            if (scopeStack.Count > 0) // Pop the scope opener
                if (scopeStack.Peek().Equals("{")) scopeStack.Pop();

            if (scopeStack.Count > 0)
            {
                if (scopeStack.Peek().Equals("function") && typeStack.Count > 0)
                {
                    if (((ProgramFunction)typeStack.Peek()).Size == 0)
                        ((ProgramFunction)typeStack.Peek()).Size++; // If it exists, it uses at least one line (even if the line is shared)
                }

                if (scopeStack.Peek().Equals(scopeType))
                    isScopeType = true;

                // If there is a different ProgramType-level scope identifier, pop the identifier and the type
                if (scopeStack.Peek().Equals("namespace") || scopeStack.Peek().Equals("class")
                    || scopeStack.Peek().Equals("interface") || scopeStack.Peek().Equals("function"))
                {
                    scopeStack.Pop();
                    if (typeStack.Count > 0) typeStack.Pop();
                }
                else // If ending at least one other named scope
                    while (scopeStack.Count > 0 && (scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                            || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("for each") || scopeStack.Peek().Equals("while")
                            || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch")))
                        scopeStack.Pop();
            }

            activeRules.Clear();
            ClearCurrentItems(writer);
            return isScopeType;
        }

        /* Maintains the stack when a bracketless scope ends; returns true if the scope is a function */
        private bool EndBracketlessScope(StreamWriter writer)
        {
            bool isFunction = false;
            if (!activeRules.OfType<ForRule_CS>().Any())
            {
                while (scopeStack.Count > 0 && scopeStack.Peek().Equals("if") || scopeStack.Peek().Equals("else if") || scopeStack.Peek().Equals("else")
                    || scopeStack.Peek().Equals("for") || scopeStack.Peek().Equals("for each") || scopeStack.Peek().Equals("while")
                    || scopeStack.Peek().Equals("do while") || scopeStack.Peek().Equals("switch") || scopeStack.Peek().Equals("function"))
                {
                    if (scopeStack.Peek().Equals("function"))
                    {
                        if (typeStack.Count > 0 && typeStack.Peek().GetType() == typeof(ProgramFunction))
                        {
                            if (((ProgramFunction)typeStack.Peek()).Size == 0)
                                ((ProgramFunction)typeStack.Peek()).Size++; // If it exists, it uses at least one line (even if the line is shared)
                            typeStack.Pop();
                        }
                        isFunction = true;
                    }
                    scopeStack.Pop();
                }
                ClearCurrentItems(writer);
            }
            return isFunction;
        }

        /* Maintains the stack when there is a scope closer; returns true if a function is ending */
        private bool CheckScopeClosersWithinFunction(StreamWriter writer, string entry)
        {
            // Check for closing parenthesis
            if (entry.Equals(")") && scopeStack.Count > 0 && scopeStack.Peek().Equals("(")) scopeStack.Pop();

            if (entry.Equals("}")) // Check for the end of an existing bracketed scope
            {
                if (EndBracketedScope(writer, "function"))
                    return true;
            }

            // Check for the end of an existing bracketless scope
            if (entry.Equals(";") && EndBracketlessScope(writer)) return true; // True if bracketless scope was a function

            return false;
        }

        /* Maintains the stack when there is a scope opener */
        private void CheckScopeOpenersWithinFunction(string entry, bool scopeOpener)
        {
            if (entry.Equals("(")) scopeStack.Push(entry); // Check for open parenthesis

            if (entry.Equals("{"))
            {
                if (!scopeOpener && CheckIfFunction()) // Check if a new function is being started
                {
                    scopeStack.Push(entry);
                    ProcessFunctionData();
                }
                else // Push scope opener onto scopeStack
                    scopeStack.Push(entry);
            }

            if (entry.Equals("=>") && CheckIfFunction()) // Check if a new lambda function is being started
                ProcessFunctionData();
        }

        /* Increments the current function's size, if possible and appropriate */
        private void IncrementFunctionSize()
        {
            if (typeStack.Peek().GetType() == typeof(ProgramFunction))
                ((ProgramFunction)typeStack.Peek()).Size++;
        }

        /* Maintains stack for commented areas; returns true if text is within a comment */
        private bool IgnoreEntry(string entry)
        {
            // Determine entry is within a comment, and check for the end of the comment
            if (scopeStack.Count() > 0)
            {
                if (scopeStack.Peek().Equals("\""))
                {
                    if (entry.Equals("\"")) scopeStack.Pop();
                    return true;
                }

                if (scopeStack.Peek().Equals("'"))
                {
                    if (entry.Equals("'")) scopeStack.Pop();
                    return true;
                }

                if (scopeStack.Peek().Equals("//"))
                {
                    if (entry.Equals(" "))
                    {
                        scopeStack.Pop();
                        return false;
                    }
                    return true;
                }
                if (scopeStack.Peek().Equals("/*"))
                {
                    if (entry.Equals("*/")) scopeStack.Pop();
                    return true;
                }
            }

            // If starting a commented area, push the entry
            if (entry.Equals("\"") || entry.Equals("'") || entry.Equals("//") || entry.Equals("/*"))
            {
                scopeStack.Push(entry);
                return true;
            }

            return false;
        }

        /* Adds current text to file in the case of a new statement or scope */
        private void UpdateCurrentText(StreamWriter writer, string entry)
        {
            if (entry.Equals(";") || entry.Equals("}") || entry.Equals("{"))
            {
                if (writer != null)
                    foreach (string text in currentText)
                        if (!text.Equals(" ")) writer.WriteLine(text);
                currentText.Clear();
            }
        }

        /* Clears the current text and active rules */
        private void ClearCurrentItems(StreamWriter writer)
        {
            if (writer != null)
                foreach (string text in currentText) writer.WriteLine(text);
            currentText.Clear();
            activeRules.Clear();
        }

        /* Checks if the entry denotes a new line, increments the number of lines in the file */
        private bool CheckIfNewLine(string entry)
        {
            if (entry.Equals(" "))
            {
                currentLine++;
                currentText.Add(entry);
                return true;
            }
            return false;
        }
    }
}

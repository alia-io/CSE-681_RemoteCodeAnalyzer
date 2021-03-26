///////////////////////////////////////////////////////////////////////////////////////////
///                                                                                     ///
///  Rules.cs - Defines a set of rules to determine whether input is a valid scope      ///
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
 *   This module contains a set of rules and a rule factory, which returns a rule if the
 *   input implies the start of the rule. Each rule has a set of conditions for successive
 *   entries. If a rule is active, text entries should be sent to its IsPassed method for
 *   testing. If a rule fails, its Complete property is set to true while its IsPassed method
 *   returns false. This rule should then be removed from active rules. If a rule succeeds,
 *   its Complete property is set to true while its IsPassed method returns true. A scope of
 *   this type should then be added to the current scopes. If the Complete property is false
 *   and IsPassed returns false, then the rule should continue to be tested with the next entry.
 *   These rules are meant to be used in conjunction with FileAnalyzer, which reads parsed files
 *   entry by entry, in order to maintain the scope tree and analyze function complexity.
 * 
 *   Public Interface
 *   ----------------
 *   
 *   ControlFlowScopeRule
 *   --------------------
 *   ControlFlowScopeRule controlFlowScopeRule = new IfRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new ElseIfRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new ElseRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new ForRule_CS((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new ForRule_JAVA((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new ForEachRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new WhileRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new DoWhileRule((int) scopeCount);
 *   ControlFlowScopeRule controlFlowScopeRule = new SwitchRule((int) scopeCount);
 *   int step = controlFlowScopeRule.Step;
 *   bool complete = controlFlowScopeRule.Complete;
 *   bool passed = controlFlowScopeRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = controlFlowScopeRule.GetScopeType();
 *   
 *   IfRule
 *   ------
 *   IfRule ifRule = new IfRule((int) scopeCount);
 *   int step = ifRule.Step;
 *   bool complete = ifRule.Complete;
 *   bool passed = ifRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = ifRule.GetScopeType();
 *   
 *   ElseIfRule
 *   ----------
 *   ElseIfRule elseIfRule = new ElseIfRule((int) scopeCount);
 *   int step = elseIfRule.Step;
 *   bool complete = elseIfRule.Complete;
 *   bool passed = elseIfRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = elseIfRule.GetScopeType();
 *   
 *   ElseRule
 *   --------
 *   ElseRule elseRule = new ElseRule((int) scopeCount);
 *   int step = elseRule.Step;
 *   bool complete = elseRule.Complete;
 *   bool passed = elseRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = elseRule.GetScopeType();
 *   
 *   ForRule_CS
 *   ----------
 *   ForRule_CS forRule_CS = new ForRule_CS((int) scopeCount);
 *   int step = forRule_CS.Step;
 *   bool complete = forRule_CS.Complete;
 *   bool passed = forRule_CS.IsPassed((string) entry, (int) scopeCount);
 *   string scope = forRule_CS.GetScopeType();
 *   
 *   ForRule_JAVA
 *   ------------
 *   ForRule_JAVA forRule_JAVA = new ForRule_JAVA((int) scopeCount);
 *   int step = forRule_JAVA.Step;
 *   bool complete = forRule_JAVA.Complete;
 *   bool passed = forRule_JAVA.IsPassed((string) entry, (int) scopeCount);
 *   string scope = forRule_JAVA.GetScopeType();
 *   
 *   ForEachRule
 *   ------------
 *   ForEachRule forEachRule = new ForEachRule((int) scopeCount);
 *   int step = forEachRule.Step;
 *   bool complete = forEachRule.Complete;
 *   bool passed = forEachRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = forEachRule.GetScopeType();
 *   
 *   WhileRule
 *   ---------
 *   WhileRule whileRule = new WhileRule((int) scopeCount);
 *   int step = whileRule.Step;
 *   bool complete = whileRule.Complete;
 *   bool passed = whileRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = whileRule.GetScopeType();
 *   
 *   DoWhileRule
 *   -----------
 *   DoWhileRule doWhileRule = new DoWhileRule((int) scopeCount);
 *   int step = doWhileRule.Step;
 *   bool complete = doWhileRule.Complete;
 *   bool passed = doWhileRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = doWhileRule.GetScopeType();
 *   
 *   SwitchRule
 *   ----------
 *   SwitchRule switchRule = new SwitchRule((int) scopeCount);
 *   int step = switchRule.Step;
 *   bool complete = switchRule.Complete;
 *   bool passed = switchRule.IsPassed((string) entry, (int) scopeCount);
 *   string scope = switchRule.GetScopeType();
 *   
 *   ControlFlowScopeRuleFactory
 *   ---------------------------
 *   ControlFlowScopeRule controlFlowScopeRule = ControlFlowScopeRuleFactory.GetRule((List<ControlFlowScopeRule>) activeRules, (string) rule, (int) scopeCount, (string) fileType);
 */

using System.Linq;
using System.Collections.Generic;

namespace CodeAnalyzer
{
    /* Common superclass for all control flow scope rules */
    public abstract class ControlFlowScopeRule
    {
        public int Step { get; internal set; }
        public bool Complete { get; internal set; }
        public ControlFlowScopeRule() { Step = 0; Complete = false; }
        public abstract bool IsPassed(string entry, int scopeCount);
        public abstract string GetScopeType();
    }

    /* Tests rules for "if" statement syntax */
    public class IfRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public IfRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // Rule passed.
                    return Complete = true;
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "if";
    }

    /* Tests rules for "else if" statement syntax */
    public class ElseIfRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public ElseIfRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // Rule passed.
                    return Complete = true;
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "else if";
    }

    /* Tests rules for "else" statement syntax */
    public class ElseRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public ElseRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            // To pass: Anything except "if".
            if (entry.Equals("if"))
            {
                Complete = true;
                return false;
            }
            return Complete = true; // Rule passed.
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "else";
    }

    /* Tests rules for "for" loop syntax for C# */
    public class ForRule_CS : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public ForRule_CS(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry.
                    Step = 2;
                    break;
                case 2: // To pass: Find semicolon.
                    if (entry.Equals(";")) Step = 3;
                    break;
                case 3: // To pass: Find at least one entry.
                    Step = 4;
                    break;
                case 4: // To pass: Find semicolon.
                    if (entry.Equals(";")) Step = 5;
                    break;
                case 5: // To pass: Find at least one entry.
                    Step = 6;
                    break;
                case 6: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 7;
                    break;
                case 7: // Rule passed.
                    return Complete = true;
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "for";
    }

    /* Tests rules for "for" loop syntax for Java */
    public class ForRule_JAVA : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public ForRule_JAVA(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find openeing parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: //To pass:Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // Rule passed.
                    return Complete = true;
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "for";
    }

    /* Tests rules for "foreach" loop syntax */
    public class ForEachRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public ForEachRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // Rule passed.
                    return Complete = true;
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "for each";
    }

    /* Tests rules for "while" loop syntax */
    public class WhileRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public WhileRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // To pass: Anything except semicolon.
                    if (entry.Equals(";"))
                    {
                        Complete = true;
                        break;
                    }
                    return Complete = true; // Rule passed.
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "while";
    }

    /* Tests rules for "do while" loop syntax */
    public class DoWhileRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public DoWhileRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            return Complete = true;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "do while";
    }

    /* Tests rules for "switch" statement syntax */
    public class SwitchRule : ControlFlowScopeRule
    {
        private readonly int savedScopeCount;

        public SwitchRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

        /* Tests whether the current entry passes the rule */
        public override bool IsPassed(string entry, int scopeCount)
        {
            switch (Step)
            {
                case 0: // To pass: Find opening parenthesis.
                    if (entry.Equals("(")) Step = 1; else Complete = true;
                    break;
                case 1: // To pass: Find at least one entry inside parentheses.
                    Step = 2;
                    break;
                case 2: // To pass: Find closing parenthesis.
                    if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 3;
                    break;
                case 3: // To pass: Find opening bracket.
                    if (!entry.Equals("{"))
                    {
                        Complete = true;
                        break;
                    }
                    return Complete = true; // Rule passed.
            }
            return false;
        }

        /* Returns the type of scope */
        public override string GetScopeType() => "switch";
    }

    public static class ControlFlowScopeRuleFactory
    {
        public static ControlFlowScopeRule GetRule(List<ControlFlowScopeRule> activeRules, string rule, int scopeCount, string fileType)
        {
            if (rule == "if")
            {
                if (!activeRules.OfType<ElseRule>().Any())
                    return new IfRule(scopeCount);
                else
                {
                    activeRules.RemoveAll(item => item.GetType() == typeof(ElseRule));
                    return new ElseIfRule(scopeCount);
                }
            }

            if (rule == "else") return new ElseRule(scopeCount);

            if (rule == "for")
            {
                if (fileType.Equals("cs")) return new ForRule_CS(scopeCount);
                if (fileType.Equals("java")) return new ForRule_JAVA(scopeCount);
            }

            if (rule == "foreach" && !fileType.Equals("java")) return new ForEachRule(scopeCount);

            if (rule == "while") return new WhileRule(scopeCount);

            if (rule == "do") return new DoWhileRule(scopeCount);

            if (rule == "switch") return new SwitchRule(scopeCount);

            return null;
        }
    }
}

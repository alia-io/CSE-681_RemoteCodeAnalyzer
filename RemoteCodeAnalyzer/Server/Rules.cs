using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Rule
    {
        public interface IRule
        {
            bool IsPassed(string entry, int scopeCount);
        }

        public interface ICFScopeRule : IRule
        {
            string GetScopeType();
        }

        public abstract class CFScopeRule : ICFScopeRule
        {
            public int Step { get; internal set; }
            public bool Complete { get; internal set; }
            public CFScopeRule() { Complete = false; }
            public abstract bool IsPassed(string entry, int scopeCount);
            public abstract string GetScopeType();
        }

        /* Tests rules for "if" statement syntax */
        public class IfRule : CFScopeRule
        {
            private int savedScopeCount;

            public IfRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

            public override bool IsPassed(string entry, int scopeCount)
            {
                switch (Step)
                {
                    case 0: // To pass: Find "if".
                        if (entry.Equals("if")) Step = 1;
                        break;
                    case 1: // To pass: Find opening parenthesis.
                        if (entry.Equals("(")) Step = 2; else Complete = true;
                        break;
                    case 2: // To pass: Find at least one entry inside parentheses.
                        Step = 3;
                        break;
                    case 3: // To pass: Find closing parenthesis.
                        if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 4;
                        break;
                    case 4: // Rule passed.
                        return Complete = true;
                }
                return false;
            }

            public override string GetScopeType() { return "if"; }
        }

        /* Tests rules for "else if" statement syntax */
        public class ElseIfRule : CFScopeRule
        {
            private int savedScopeCount;

            public ElseIfRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

            public override bool IsPassed(string entry, int scopeCount)
            {
                switch (Step)
                {
                    case 0: // To pass: Find "else".
                        if (entry.Equals("else")) Step = 1;
                        break;
                    case 1: // To pass: Find "if".
                        if (entry.Equals("if")) Step = 2; else Complete = true;
                        break;
                    case 2: // To pass: Find opening parenthesis.
                        if (entry.Equals("(")) Step = 3; else Complete = true;
                        break;
                    case 3: // To pass: Find at least one entry inside parentheses.
                        Step = 4;
                        break;
                    case 4: // To pass: Find closing parenthesis.
                        if (entry.Equals(")") && savedScopeCount == scopeCount) Step = 5;
                        break;
                    case 5: // Rule passed.
                        return Complete = true;
                }
                return false;
            }

            public override string GetScopeType() { return "else if"; }
        }

        /* Tests rules for "else" statement syntax */
        public class ElseRule : CFScopeRule
        {
            private int savedScopeCount;

            public ElseRule(int scopeCount) : base() { savedScopeCount = scopeCount; }

            public override bool IsPassed(string entry, int scopeCount)
            {
                switch (Step)
                {
                    case 0: // To pass: Find "else".
                        if (entry.Equals("else")) Step = 1;
                        break;
                    case 1: // To pass: Anything except "if".
                        if (entry.Equals("if"))
                        {
                            Complete = true;
                            break;
                        }
                        // Add new "else" scope
                        return Complete = true;
                }
                return false;
            }

            public override string GetScopeType() { return "else"; }
        }

        public static class CFScopeRuleFactory
        {
            public static CFScopeRule GetRule(List<CFScopeRule> activeRules, string rule, int scopeCount)
            {
                if (rule == "if")
                {
                    if (!activeRules.OfType<ElseRule>().Where(item => item.Step == 1).Any())
                        return new IfRule(scopeCount);
                    else
                    {
                        activeRules.RemoveAll(item => item.GetType() == typeof(ElseRule) && ((ElseRule)item).Step == 1);
                        return new ElseIfRule(scopeCount);
                    }
                }

                if (rule == "else") return new ElseRule(scopeCount);

                return null;
            }
        }
    }
}

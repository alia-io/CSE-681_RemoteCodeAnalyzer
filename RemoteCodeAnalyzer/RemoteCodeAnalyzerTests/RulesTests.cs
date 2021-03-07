using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Server;

namespace RemoteCodeAnalyzerTests
{
    [TestClass]
    public class RulesTests
    {
        [TestMethod]
        public void IfRule1()
        {
            string[] input = { "if", "(", "x", "<", "33", ")", "{" };
            int scopeCount = 4;
            IfRule rule = new IfRule(scopeCount);
            bool expected = true;
            bool actual = rule.Complete;

            foreach (string entry in input)
            {
                if (entry.Equals("(")) scopeCount++;
                if (entry.Equals(")")) scopeCount--;
                actual = rule.IsPassed(entry, scopeCount);
            }

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, rule.Complete);
        }

        [TestMethod]
        public void IfRule2()
        {
            string[] input = { "if", "(", "x", "!", "=", "5", "{" };
            int scopeCount = 7;
            IfRule rule = new IfRule(scopeCount);
            int expectedStep = 3;
            bool expected = false;
            bool actual = rule.Complete;

            foreach (string entry in input)
            {
                if (entry.Equals("(")) scopeCount++;
                if (entry.Equals(")")) scopeCount--;
                actual = rule.IsPassed(entry, scopeCount);
            }

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expectedStep, rule.Step);
        }

        [TestMethod]
        public void ElseIfRule()
        {
            string[] input = { "else", "if", "(", "text", ".", "Equals", "(", "\"", "hello", "\"", ")", ")", "if" };
            int scopeCount = 5;
            ElseIfRule rule = new ElseIfRule(scopeCount);
            bool expected = true;
            bool actual = rule.Complete;

            foreach (string entry in input)
            {
                if (entry.Equals("(")) scopeCount++;
                if (entry.Equals(")")) scopeCount--;
                actual = rule.IsPassed(entry, scopeCount);
            }

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(expected, rule.Complete);
        }

        [TestMethod]
        public void CFScopeRuleFactory1()
        {
            List<CFScopeRule> activeRules = new List<CFScopeRule>();
            int scopeCount = 4;
            string fileType = "*.cs";
            string entry;
            string[] input = { "int", "magic", "=", "7", ";", "int", "x", "=", "6", ";", "int", "code", ";", " ", "bool", 
                "found", "=", "false", ";", "if", "(", "x", "<=", "10", ")", "{", "if", "(", "x", "<", "0", ")", "code",
                "=", "-", "100", ";", "else", "if", "(", "x", "<", "magic", ")", "code", "=", "-", "10", ";", "else", "if",
                "(", "x", ">", "magic", ")", "code", "=", "10", ";", "else", "{", "code", "=", "0", ";", "found", "=", "true",
                ";", "}", "}", "else", "{", "code", "=", "100", ";", "}" };
            List<string> expectedScopes = new List<string>() { "if", "if", "else if", "else if", "else", "else" };
            List<string> actualScopes = new List<string>();

            for (int i = 0; i < input.Length; i++)
            {
                entry = input[i];
                bool scopeOpener = false;
                CFScopeRule newRule = CFScopeRuleFactory.GetRule(activeRules, entry, scopeCount, fileType);
                List<CFScopeRule> failedRules = new List<CFScopeRule>();

                if (entry.Equals("(") || entry.Equals("{")) scopeCount++;
                if (entry.Equals(")") || entry.Equals("}")) scopeCount--;

                if (newRule != null) activeRules.Add(newRule);

                foreach (CFScopeRule rule in activeRules)
                {
                    if (rule.IsPassed(entry, scopeCount))
                    {
                        actualScopes.Add(rule.GetScopeType());
                        scopeOpener = true;
                    }
                }

                if (scopeOpener) activeRules.Clear();
                else activeRules.RemoveAll(rule => rule.Complete);
            }

            CollectionAssert.AreEqual(expectedScopes, actualScopes);
        }
    }
}

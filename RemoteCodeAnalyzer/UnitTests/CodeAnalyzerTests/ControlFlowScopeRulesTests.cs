﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeAnalyzer;

namespace UnitTests.CodeAnalyzerTests
{
    [TestClass]
    public class ControlFlowScopeRulesTests
    {
        [TestMethod]
        public void IfRule1()
        {
            string[] input = { "(", "x", "<", "33", ")", "{" };
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
            string[] input = { "(", "x", "!", "=", "5", "{" };
            int scopeCount = 7;
            IfRule rule = new IfRule(scopeCount);
            int expectedStep = 2;
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
            string[] input = { "(", "text", ".", "Equals", "(", "\"", "hello", "\"", ")", ")", "if" };
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
            List<ControlFlowScopeRule> activeRules = new List<ControlFlowScopeRule>();
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
                ControlFlowScopeRule newRule;
                bool scopeOpener = false;
                //CFScopeRule newRule = CFScopeRuleFactory.GetRule(activeRules, entry, scopeCount, fileType);
                List<ControlFlowScopeRule> failedRules = new List<ControlFlowScopeRule>();

                if (entry.Equals("(") || entry.Equals("{")) scopeCount++;
                if (entry.Equals(")") || entry.Equals("}")) scopeCount--;

                foreach (ControlFlowScopeRule rule in activeRules)
                {
                    if (rule.IsPassed(entry, scopeCount))
                    {
                        actualScopes.Add(rule.GetScopeType());
                        scopeOpener = true;
                    }
                }

                newRule = ControlFlowScopeRuleFactory.GetRule(activeRules, entry, scopeCount, fileType);
                if (newRule != null) activeRules.Add(newRule);

                if (scopeOpener) activeRules.Clear();
                else activeRules.RemoveAll(rule => rule.Complete);
            }

            foreach (string scope in actualScopes)
            {
                Console.WriteLine(scope);
            }

            CollectionAssert.AreEqual(expectedScopes, actualScopes);
        }
    }

    [TestClass]
    public class FunctionScopeRulesTests
    {
        [TestMethod]
        public void ConstructorRule1()
        {
            string[] input = { "public", "Pet",
                "(", "int", "locationX", ",", "int", "locationY", ",", "int", "speed", ",", "int", "price", ")",
                ":", "base", "(", "locationX", ",", "locationY", ",", "speed", ")", "{" };

            //TODO:
            Assert.AreEqual(0, 0);
        }
    }
}

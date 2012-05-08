using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeAwhile.CSharp.Utilities.Test.CommandLineParserTests
{
    [TestClass]
    public class CommandLineParserBasicTests
    {
        [TestMethod]
        public void CanParseParameters()
        {
            CommandLineParser clp = new CommandLineParser(new string[] { "/" }, new string[] { ":" });
            clp.Add<bool>("activate");
            clp.Add<bool>("option");

            string[] unParsed;
            Dictionary<string, object> results = clp.ParseCommandLine(new string[] { "/activate:false", "skipThisParam", "/option:true" }, out unParsed);

            Assert.IsTrue(results.Count == 2, @"There should only be 2 options parsed");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "activate"), @"The ""activate"" key should exist in the result");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "option"), @"The ""option"" key should exist in the result");
            Assert.IsFalse((bool)results.Single(kvp => kvp.Key == "activate").Value, @"The ""activate"" object should be false");
            Assert.IsTrue((bool)results.Single(kvp => kvp.Key == "option").Value, @"The ""option"" object should be true");
            Assert.IsTrue(unParsed.Single() == "skipThisParam", @"There should be one unparsed option in the command line: skipThisParam");
        }

        [TestMethod]
        public void CanParseConsecutiveParameters()
        {
            CommandLineParser clp = new CommandLineParser(new string[] { "/" }, new string[] { ":" });
            clp.Add<bool>("activate");
            clp.Add<bool>("option");

            string[] unParsed;
            Dictionary<string, object> results = clp.ParseCommandLine(new string[] { "/activate:false /option:true" }, out unParsed);

            Assert.IsTrue(results.Count == 2, @"There should only be 2 options parsed");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "activate"), @"The ""activate"" key should exist in the result");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "option"), @"The ""option"" key should exist in the result");
            Assert.IsFalse((bool)results.Single(kvp => kvp.Key == "activate").Value, @"The ""activate"" object should be false");
            Assert.IsTrue((bool)results.Single(kvp => kvp.Key == "option").Value, @"The ""option"" object should be true");
            Assert.IsFalse(unParsed.Any(), @"There should be no unparsed options in the command line");
        }

        [TestMethod]
        public void CanParseSwitches()
        {
            CommandLineParser clp = new CommandLineParser(new string[] { "/" }, new string[] { ":" });
            clp.Add<bool>("activate");
            clp.Add<bool>("option");

            string[] unParsed;
            Dictionary<string, object> results = clp.ParseCommandLine(new string[] { "/activate:false", "/skipThisParam", "/option" }, out unParsed);

            Assert.IsTrue(results.Count == 2, @"There should only be 2 options parsed");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "activate"), @"The ""activate"" key should exist in the result");
            Assert.IsTrue(results.Any(kvp => kvp.Key == "option"), @"The ""option"" key should exist in the result");
            Assert.IsFalse((bool)results.Single(kvp => kvp.Key == "activate").Value, @"The ""activate"" object should be false");
            Assert.IsTrue((bool)results.Single(kvp => kvp.Key == "option").Value, @"The ""option"" object should be true");
            Assert.IsTrue(unParsed.Single() == "/skipThisParam", @"There should be one unparsed option in the command line: /skipThisParam");
        }
    }
}

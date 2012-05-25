namespace CodeAwhile.CSharp.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    // TODO:
    // 1. aliases, e.g., -v and --verbose
    // 2. array types
    // 3. optional vs. required (so a validation pass on the results?)
    // 4. usages
    public class CommandLineParser
    {
        public CommandLineParser(IList<string> paramDelims, IList<string> valueDelims)
        {
            _paramDelims = paramDelims.ToArray();
            _valueDelims = valueDelims.ToArray();
            _parameters = new Dictionary<string, Type>();
            _commandLineRegex = BuildCommandLineRegex(_paramDelims, _valueDelims);
        }

        // TODO: need an add that specifies usage
        public void Add<T>(string paramName)
        {
            Add(paramName, typeof(T));
        }

        public void Add(string paramName, Type paramType)
        {
            _parameters.Add(paramName, paramType);
        }

        // TODO: do we want to print the usage or return it so it can be formatted?
        public void PrintUsage()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, object> ParseCommandLine(string[] args, out string[] unparsed)
        {
            var paramsWithValues = new Dictionary<string, object>();
            var unparsedStrings = new List<string>();
            foreach (var arg in args)
            {
                var m = _commandLineRegex.Match(arg);
                var swtch = GetSwitch(m);
                var param = GetParam(m);
                var value = GetValue(m);

                // Bail if the regex doesn't match or if the parameter hasn't been declared
                if (!m.Success || (!_parameters.ContainsKey(swtch) && !_parameters.ContainsKey(param)))
                {
                    unparsedStrings.Add(arg);
                    continue;
                }

                // Really there are only two options, but for consistency they are enumerated explicitly
                if (!String.IsNullOrEmpty(swtch))
                {
                    paramsWithValues.Add(swtch, ParseToSupportedType(_defaultSwitchValue, _parameters[swtch]));
                }
                else if (!String.IsNullOrEmpty(param))
                {
                    paramsWithValues.Add(param, ParseToSupportedType(GetValue(m), _parameters[param]));
                }
                else
                {
                    // we shouldn't be here... unless someone broke the regular expression.
                    throw new ApplicationException(
                        String.Format("Parsing command line failed at argument: '{0}'\nUnrecognized parameter.", arg));
                }
            }

            unparsed = unparsedStrings.ToArray();
            return paramsWithValues;
        }

        // TODO@crbouch: There's gotta be a better way than this. Reflection is hella slow.
        //         Maybe some straight-up reflection inside a T4 or something to generate the methods... I don't know.
        public object ParseToSupportedType(string value, Type type)
        {
            var meth = type.GetMethod("Parse",
                                      System.Reflection.BindingFlags.Static |
                                      System.Reflection.BindingFlags.Public);
            if (meth != null)
            {
                return meth.Invoke(null, new object[] { value });
            }
            return null;
        }

        protected Regex BuildCommandLineRegex(string[] paramDelims, string[] valueDelims)
        {
            string paramFormat = @"(?:(?:{0})(?<paramName>\w+)(?:{1})(?<value>\w+))|(?:(?:{0})(?<switch>\w+))";
            return new Regex(
                String.Format(paramFormat, String.Join("|", paramDelims), String.Join("|", valueDelims)),
                RegexOptions.Compiled);
        }

        private string GetSwitch(Match m)
        {
            return m.Groups["switch"].Value;
        }

        private string GetParam(Match m)
        {
            return m.Groups["paramName"].Value;
        }

        private string GetValue(Match m)
        {
            return m.Groups["value"].Value;
        }

        protected string _defaultSwitchValue = "true";
        protected string _defaultParamDelim = " ";
        protected Dictionary<string, Type> _parameters;
        protected string[] _paramDelims;
        protected string[] _valueDelims;
        protected Regex _commandLineRegex;
    }
}

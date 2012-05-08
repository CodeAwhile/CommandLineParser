namespace CodeAwhile.CSharp.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

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

        // TODO@crbouch: this got messy quick. Need to refactor this method.
        // TODO@crbouch: also, this is breaking one of the tests because it's stripping off the _paramDelims of a param that's not known
        public Dictionary<string, object> ParseCommandLine(string[] args, out string[] unparsed)
        {
            string cmdLine = String.Join(_defaultParamDelim, args);

            var pieces = cmdLine.Split(_paramDelims, StringSplitOptions.RemoveEmptyEntries)
                                .Select(param => param.Split(new string[] { _defaultParamDelim },
                                                             StringSplitOptions.RemoveEmptyEntries));

            unparsed = pieces.SelectMany(paramArray => paramArray.Where((param, index) => index > 0)).ToArray();

            pieces = pieces.Select(paramArray => paramArray[0])
                           .Select(param => param.Split(_valueDelims, StringSplitOptions.RemoveEmptyEntries));

            unparsed = unparsed.Union(pieces.Where(paramValueArray => !_parameters.ContainsKey(paramValueArray[0])).SelectMany(paramValueArray => paramValueArray)).ToArray();
            pieces = pieces.Where(paramValueArray => _parameters.ContainsKey(paramValueArray[0]));
            
            var parameters = pieces.Where(paramValueArray => paramValueArray.Length > 1);
            var switches   = pieces.Where(paramValueArray => paramValueArray.Length == 1)
                                   .Select(paramValueArray => new string[] { paramValueArray[0], _defaultSwitchValue });

            return parameters.Union(switches)
                             .ToDictionary(paramValueArray => paramValueArray[0],
                                           paramValueArray => ParseToSupportedType(paramValueArray[1], _parameters[paramValueArray[0]]));
        }

        // TODO@crbouch: There's gotta be a better way than this. Reflection is hella slow.
        //               Maybe some straight-up reflection inside a T4 or something to generate the methods... I don't know.
        public object ParseToSupportedType(string value, Type type)
        {
            var meth = type.GetMethod("Parse", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (meth != null)
            {
                return meth.Invoke(null, new object[] { value });
            }
            return null;
        }

        protected string _defaultSwitchValue = "true";
        protected string _defaultParamDelim = " ";
        protected Dictionary<string, Type> _parameters;
        protected string[] _paramDelims;
        protected string[] _valueDelims;
    }
}

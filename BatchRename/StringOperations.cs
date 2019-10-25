﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename
{
    public abstract class OptArgs
    {
    }

    public abstract class StringOperations
    {
        public OptArgs Arguments { get; set; }
        public abstract string Operate(string input);
    }


    // CLASS REPLACE STRING
    public class ReplaceArgs : OptArgs
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public class ReplaceStringOpertation : StringOperations
    {

        public override string Operate(string input)
        {
            string result;
            var args = Arguments as ReplaceArgs;
            result = input.Replace(args.From, args.To);
            return result;
        }
    }

    //CLASS NEW CASE

    public class CaseArg : OptArgs
    {
        public string Case { get; set; }
    }

    public class NewCaseStringOperation : StringOperations
    {
        static string UpperFirstLetter(string input)
        {
            StringBuilder result = new StringBuilder(input);

            if (result[0] >= 'a' && result[0] <= 'z')
            {
                result[0] = (char)('A' + (input[0] - 'a'));
            }

            for (int i = 1; i < input.Length; i++)
            {
                if (input[i] >= 'a' && input[i] <= 'z' && input[i - 1] == ' ')
                {
                    result[i] = (char)('A' + (input[i] - 'a')); //change le
                }
            }
            return result.ToString();
        }

        public override string Operate(string input)
        {
            string result = input;
            var arg = Arguments as CaseArg;

            if (arg.Case == "Lower")
            {
                result = input.ToLower();
            }
            if (arg.Case == "Upper")
            {
                result = input.ToUpper();
            }
            if (arg.Case == "Upper First Letter")
            {
                result = UpperFirstLetter(input);
            }
            return result;
        }


    }


    //FULL NAME NORMALIZATION CLASS

    public class NormalizationStringOperation : StringOperations
    {
        public override string Operate(string input)
        {
            string result = String.Join(" ", input.Split(new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries));
            return result;
        }
    }

    //GUID CLASS
    public class UniqueNameStringOperation : StringOperations
    {
        public override string Operate(string input)
        {
            Guid guid = Guid.NewGuid();
            string result = guid.ToString();
            return result;
        }
    }
}

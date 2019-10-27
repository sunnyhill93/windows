﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BatchRename
{
 
    public class ReplaceArgs : StringArgs
    {
        public string From { get; set; }
        public string To { get; set; }
    }

    public class ReplaceOperation : StringOperation
    {
        public override string Name => "Replace";

        public override string Description
        {
            get
            {
                string from = (Args as ReplaceArgs).From;
                string to = (Args as ReplaceArgs).To;
                return ($"Replace substring {from} with {to}");
            }
        }

        public ReplaceOperation()
        {
            Args = new ReplaceArgs()
            {
                From = "<edit>",
                To = "<edit>"
            };
        }

        public override string OperateString(string input)
        {
            string result;
            result = input.Replace((Args as ReplaceArgs).From, (Args as ReplaceArgs).To);
            return result;
        }

        public override void OpenDialog()
        {
            var screen = new ReplaceStringDialog(Args);
            screen.OptArgsChange += ChangeReplaceArgs;
            if (screen.ShowDialog() == true)
            {

            }
        }

        void ChangeReplaceArgs(string from, string to)
        {
            (Args as ReplaceArgs).From = from;
            (Args as ReplaceArgs).To = to;
        }
    }
}
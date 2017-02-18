using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBotRedux
{
    class Result
    {
        private string message;
        private bool succeeded;
        public Result(bool s, string m = "")
        {
            succeeded = s;
            message = m;
        }
        public string GetMessage()
        {
            return message;
        }
        public bool GetSuccess()
        {
            return succeeded;
        }
    }
}

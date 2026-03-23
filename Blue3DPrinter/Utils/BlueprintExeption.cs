using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blue3DPrinter
{
    [Serializable]
    public class BlueprintException : Exception
    {
        public long fileposition;

        public BlueprintException() { }

        public BlueprintException(string message, long atfileposition) : base(message)
        {
            fileposition = atfileposition;
        }
    }
}

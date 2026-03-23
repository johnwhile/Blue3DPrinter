
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityTool
{
    public abstract class UnityFile : IDisposable
    {
        protected string FileName;
        protected string FilePath;


        ~UnityFile()
        {
            Dispose();
        }

        public abstract void Dispose();
    }
}

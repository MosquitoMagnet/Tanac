using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VM.CodeCompletion
{
    /// <summary>
    /// This interface allows to provide more information for scripts such as using statements, etc.
    /// </summary>
    public interface ICSharpScriptProvider
    {
        string GetUsing();
        string GetVars();
        string GetNamespace();
    }
}

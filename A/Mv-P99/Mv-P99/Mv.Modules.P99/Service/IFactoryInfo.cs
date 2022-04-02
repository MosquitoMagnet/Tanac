using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.P99.Service
{
    public interface IFactoryInfo
    {
        string CheckPass(string code, string station);
        string Upload(string code, string fileName,Dictionary<string, string> dic);

    }
}

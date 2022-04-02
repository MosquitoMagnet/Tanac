using System;
using System.Collections;

namespace Mv.Modules.RD402.Service
{
    public class DebugGetSn : IGetSn
    {
        public (bool, string) getsn(Hashtable hashtable)
        {
            //   F79016205CSSTCX3S + T10N 
            var randomString = GetRandomString(17, true, false, true, false, "");
            randomString += "+" + GetRandomString(2, true, false, true, false, "");
            randomString += hashtable["axis"];
            randomString += GetRandomString(1, true, false, true, false, "");
            return (true, randomString);
        }
        public string GetRandomString(int length, bool useNum, bool useLow, bool useUpp, bool useSpe, string custom)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = custom;
            if (useNum == true) { str += "0123456789"; }
            if (useLow == true) { str += "abcdefghijklmnopqrstuvwxyz"; }
            if (useUpp == true) { str += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; }
            if (useSpe == true) { str += "!\"#$%&'()*+,-./:;<=>?@[\\]^_`{|}~"; }
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
    }
}
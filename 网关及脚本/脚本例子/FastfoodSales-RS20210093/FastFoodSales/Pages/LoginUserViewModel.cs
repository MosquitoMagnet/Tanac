using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Pages
{
    public class LoginUserViewModel
    {
        public LoginUserViewModel(string password)
        {
            this.Value = password;
        }
        public string Value { get; set; }
    }

}

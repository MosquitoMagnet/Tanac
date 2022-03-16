using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAQ.Service
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        private bool _currentValue;
        public bool CurrentValue
        {
            get => _currentValue;
            set
            {
                ValueChanged = ((_currentValue) != (value));
                _currentValue = value;
            }
        }
    }
}

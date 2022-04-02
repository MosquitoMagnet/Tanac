using System;
using System.Collections.Generic;
using System.Text;

namespace Mv.Modules.RD402.Hive.Services
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        public string OldValue { get; private set; }
        private string _currentValue;
        public string CurrentValue
        {
            get => _currentValue;
            set
            {
                ValueChanged = ((_currentValue) != (value));
                OldValue = _currentValue;
                _currentValue = value;
            }
        }
    }
}

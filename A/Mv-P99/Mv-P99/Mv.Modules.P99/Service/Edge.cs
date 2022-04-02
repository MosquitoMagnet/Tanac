namespace Mv.Modules.P99.Service
{
    public struct Edge
    {
        public bool ValueChanged { get; private set; }
        public short OldValue { get; private set; }
        private short _currentValue;
        public short CurrentValue
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
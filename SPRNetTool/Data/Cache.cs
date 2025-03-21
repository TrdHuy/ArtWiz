using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtWiz.Data
{
    public class Cache<T>
    {
        private T? _value;
        private DateTime _lastUpdateTime;
        private readonly TimeSpan _expirationTime;

        public Cache(TimeSpan expirationTime)
        {
            _expirationTime = expirationTime;
            _lastUpdateTime = DateTime.MinValue;
        }

        public bool IsValid => _value != null && (DateTime.UtcNow - _lastUpdateTime) < _expirationTime;

        public T? Value
        {
            get => IsValid ? _value : default;
            set
            {
                _value = value;
                _lastUpdateTime = DateTime.UtcNow;
            }
        }
    }
}

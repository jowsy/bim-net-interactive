using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jowsy.Revit.KernelAddin.Core
{
    public sealed class Variables
    {

        public event EventHandler<EventArgs> VariablesChanged;

        private readonly Dictionary<string, object> _variables = new Dictionary<string, object>();

        public Dictionary<string, object> GetVariables() => _variables;

        public void Add(string name, object value)
        {


            if (!_variables.ContainsKey(name))
            {
                _variables.Add(name, value);
                VariablesChanged.Invoke(this, EventArgs.Empty);
                return;
            }

            _variables[name] = value;
            VariablesChanged.Invoke(this, EventArgs.Empty);
        }

        public void Remove(string name, object value)
        {

            _variables.Remove(name);
            VariablesChanged.Invoke(this, EventArgs.Empty);
        }

        internal bool TryGetValue(string name, out object value)
        {
            return _variables.TryGetValue(name, out value);
        }

        internal bool TryGetValue<T>(string name, out T value)
        {
            _variables.TryGetValue(name, out object objectValue);
            try
            {
                value = (T)objectValue;
            }
            catch (Exception)
            {
                value = default;
                return false;
            }
            return false;
        }
    }
}

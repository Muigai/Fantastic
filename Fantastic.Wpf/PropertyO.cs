using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;

namespace Fantastic.Wpf
{
    public sealed class PropertyO<T> : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        readonly Func<T> getValue;

        public PropertyO(object source, PropertyInfo propertyInfo, DispatcherTimer timer)
        {

            getValue = (Func<T>)propertyInfo.GetGetMethod()
                                 .CreateDelegate(typeof(Func<T>), source); ;

            var value = default(T);

            timer.Tick +=
                (_, __) =>
                {

                    var newValue = getValue();

                    if (EqualityComparer<T>.Default.Equals(newValue, value))
                    {
                        return;
                    }

                    value = newValue;

                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                };
        }

        public T Value
        {
            get
            {
                return getValue();
            }
        }

        public override string ToString()
        {
            return (Value as object) == null ? null : Value.ToString();
        }
    }

}

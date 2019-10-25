using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Threading;
using Fantastic.Utils;

namespace Fantastic.Wpf
{
    using static LambdaUtil;

    public sealed class PropertyI<T> : IDataErrorInfo, INotifyPropertyChanged, IValue
    {

        public event PropertyChangedEventHandler PropertyChanged;

        T value;

        readonly Action<T> setSourceValue;

        readonly Func<string> errorSource;

        public PropertyI(object source, PropertyInfo propertyInfo)
        {

            value = (T)propertyInfo.GetGetMethod()
                        .Invoke(source, null);

            setSourceValue = (Action<T>)propertyInfo.GetSetMethod()
                                         .CreateDelegate(typeof(Action<T>), source);

            var errorInfo = source as IDataErrorInfo;

            errorSource = errorInfo == null ? fun(() => string.Empty) : () => errorInfo[propertyInfo.Name];
        }

        public T Value
        {
            get
            {
                return value;
            }
            set
            {

                if (EqualityComparer<T>.Default.Equals(this.value, value))
                {
                    return;
                }

                this.value = value;

                setSourceValue(value);

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Error"));
            }
        }

        string IDataErrorInfo.this[string _] => errorSource();

        string IDataErrorInfo.Error => errorSource();

        public override string ToString()
        {
            return (value as object) == null ? null : value.ToString();
        }

        object IValue.Value => Value;

    }

}

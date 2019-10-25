using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Threading;
using Fantastic.Utils;

namespace Fantastic.Wpf
{
    using static EnumerableUtils;
    using static LambdaUtil;

    public sealed class PropertyCollection<T, U> : ObservableCollection<U>, IDataErrorInfo, INotifyPropertyChanged, IEnumerable<U>
        where T : ICollection<U>
    {

        readonly Func<string> errorSource;

        public PropertyCollection(object source, PropertyInfo propertyInfo, DispatcherTimer timer)
        {

            var value = (T)propertyInfo.GetGetMethod()
                            .Invoke(source, null);

            value.ForEach(a => Add(a));

            var errorInfo = source as IDataErrorInfo;

            errorSource = errorInfo == null ? fun(() => string.Empty) : () => errorInfo[propertyInfo.Name];

            var itemCount = value.Count;

            var updatedFromSource = false;

            CollectionChanged +=
                (_, args) =>
                {
                    if (updatedFromSource)
                    {
                        return;
                    }

                    args.NewItems?
                        .Cast<U>()
                        .Where(a => a != null)
                        .ForEach(a => value.Add(a));

                    args.OldItems?
                        .Cast<U>()
                        .Where(a => a != null)
                        .ForEach(a => value.Remove(a));

                    itemCount = value.Count;
                };

            timer.Tick +=
                (_, __) =>
                {
                    if(itemCount != value.Count)
                    {
                        updatedFromSource = true;

                        ClearItems();

                        value.ForEach(a => Add(a));

                        itemCount = value.Count;

                        updatedFromSource = false;
                    }
                };
        }

        public ObservableCollection<U> Value => this;

        string IDataErrorInfo.this[string _] => errorSource();

        string IDataErrorInfo.Error => errorSource();
    }

}

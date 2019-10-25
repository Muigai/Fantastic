using System;
using System.Collections.Generic;
using System.Text;

namespace Fantastic.Wpf
{
    public interface IViewController<T>
    {
        T Model { get; }

        bool IsReadOnly { get; }
    }
}

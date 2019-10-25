using System;
using System.Collections.Generic;
using System.Text;

namespace Fantastic.Utils
{
    public static class LambdaUtil
    {
#pragma warning disable IDE1006 // Naming Styles
        public static Func<T> fun<T>(Func<T> f) => f;


        public static Func<T, U> fun<T, U>(Func<T, U> f) => f;

        public static Func<T, U, V> fun<T, U, V>(Func<T, U, V> f) => f;

        public static Func<T, U, V, W> fun<T, U, V, W>(Func<T, U, V, W> f) => f;

        public static Func<T, U, V, W, X> fun<T, U, V, W, X>(Func<T, U, V, W, X> f) => f;

        public static Action act(Action act) => act;

        public static Action<T> act<T>(Action<T> act) => act;
#pragma warning restore IDE1006 // Naming Styles
    }

}

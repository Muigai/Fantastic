using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Fantastic.Utils
{
    public static class ValidationUtil
    {

        public static string GetErrors(this IEnumerable<IDataErrorInfo> source) =>
            string.Join(Environment.NewLine, source.Select(b => b.Error));

        public static string GetErrors<T>(this Dictionary<string, Func<T, string>> validation, T instance) =>
            string.Join(Environment.NewLine,
                        validation.Values
                                  .Select(a => a(instance))
                                  .Where(a => !string.IsNullOrWhiteSpace(a)));

        public static string GetError<T>(this Dictionary<string, Func<T, string>> validation, T instance, string property) =>
            validation.ContainsKey(property) ? validation[property](instance) : string.Empty;

        public static string NotNullOrWhitespace(string s, string errorMessage) =>
            string.IsNullOrWhiteSpace(s) ? errorMessage : string.Empty;

        public static string NotNull<T>(T s, string errorMessage) where T : class =>
            s == null ? errorMessage : string.Empty;

        public static string Positive(int s, string errorMessage) => s <= 0 ? errorMessage : string.Empty;

        public static string NotNegative(decimal s, string errorMessage) => s < 0 ? errorMessage : string.Empty;

        public static string ValidEmail(string email, string errorMessage)
        {

            if (string.IsNullOrWhiteSpace(email)
                || !email.Contains("@")
                || !email.Contains(".")
                || email.Split(new[] { '@' }, StringSplitOptions.RemoveEmptyEntries).Length < 2
                || email.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).Length < 2)
            {
                return errorMessage;
            }

            return string.Empty;
        }

    }

}

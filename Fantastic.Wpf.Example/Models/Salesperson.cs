using Fantastic.Utils;
using System;
using System.Collections.Generic;

namespace Fantastic.Wpf.Example.Models
{
    public sealed partial class Salesperson
    {

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public Branch Branch { get; set; }

        public Guid Id { get; set; }

        public string FullName => $"{FirstName} {LastName}";

        public override bool Equals(object obj) =>
            obj?.GetType() == typeof(Salesperson) && ((Salesperson)obj).Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => FullName;
    }

}

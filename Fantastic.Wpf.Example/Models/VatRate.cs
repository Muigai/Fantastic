using Fantastic.Utils;
using System;
using System.Collections.Generic;

namespace Fantastic.Wpf.Example.Models
{
    public sealed class VatRate
    {

        public string Name { get; set; }

        public decimal Rate { get; set; }

        public Guid Id { get; set; }

        public override bool Equals(object obj) =>
             obj?.GetType() == typeof(VatRate) && ((VatRate)obj).Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => Name;
    }

}

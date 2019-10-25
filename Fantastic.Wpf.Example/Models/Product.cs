using Fantastic.Utils;
using System;
using System.Collections.Generic;

namespace Fantastic.Wpf.Example.Models
{
    public sealed partial class Product
    {

        public string Name { get; set; }

        public VatRate VatRate { get; set; }

        public decimal SalePrice { get; set; }

        public Guid Id { get; set; }

        public override bool Equals(object obj) =>
            obj?.GetType() == typeof(Product) && ((Product)obj).Id == Id;

        public override int GetHashCode() => Id.GetHashCode();

        public override string ToString() => Name;
    }

}

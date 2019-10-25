using Fantastic.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Fantastic.Wpf.Example.Models
{
    using static ValidationUtil;
    using SaleDetailValidation = Dictionary<string, Func<SaleDetail, string>>;

    public sealed class SaleDetail : IDataErrorInfo
    {

        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal? SalePrice => Product?.SalePrice;

        public decimal? TotalAmount => SalePrice * Quantity;

        public decimal? VatAmount => Product?.VatRate.Rate * TotalAmount;

        static SaleDetailValidation validation =
            new SaleDetailValidation
            {
                { nameof(Product), a => NotNull(a.Product, "Select a product")},
                { nameof(Quantity), a => Positive(a.Quantity, "Quantity must be greater than 0.") },
            };

        string IDataErrorInfo.Error => validation.GetErrors(this);

        string IDataErrorInfo.this[string columnName] => validation.GetError(this, columnName);

    }

}

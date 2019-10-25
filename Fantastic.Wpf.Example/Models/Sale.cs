using Fantastic.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Fantastic.Wpf.Example.Models
{
    using static ValidationUtil;
    using SaleValidation = Dictionary<string, Func<Sale, string>>;

    public sealed class Sale : IDataErrorInfo
    {

        public Sale()
        {
            SaleDate = DateTime.Now;

            SaleDetails = new List<SaleDetail>();
        }

        public DateTime SaleDate { get; set; }

        public Branch Branch { get; set; }

        public Customer Customer { get; set; }

        public Salesperson Salesperson { get; set; }

        public List<SaleDetail> SaleDetails { get; }

        public decimal? TotalAmount => SaleDetails.Sum(a => a.TotalAmount);

        public decimal? VatAmount => SaleDetails.Sum(a => a.VatAmount);

        public decimal? GrossAmount => TotalAmount - VatAmount;

        public Guid Id { get; set; }

        static SaleValidation validation =
            new SaleValidation
            {
                { nameof(Branch), a => NotNull(a.Branch, "The branch must be specified.") },
                { nameof(Customer), a => NotNull(a.Customer, "The customer must be specified.") },
                { nameof(Salesperson), a => NotNull(a.Salesperson, "The salesperson must be specified.") },
                { nameof(SaleDetails), a => a.SaleDetails.GetErrors() },
            };

        string IDataErrorInfo.Error => validation.GetErrors(this);

        string IDataErrorInfo.this[string columnName] => validation.GetError(this, columnName);

    }

}

using Fantastic.Wpf.Example.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Fantastic.Wpf.Example.Controllers
{
    public sealed partial class SaleViewController : IViewController<Sale>
    {

        public SaleViewController(Sale sale)
        {

            Model = sale;

            string readFile(string fileName)
            {
                var assemblyFullName = Assembly.GetCallingAssembly()
                                       .FullName;

                var assembly = assemblyFullName.Substring(0, assemblyFullName.IndexOf(","));

                var stream = System.Windows.Application.GetResourceStream(new Uri(string.Format(assembly + ";component/Resources/{0}", fileName),
                                                                                          UriKind.Relative));
                var bytes = new byte[stream.Stream.Length];
                
                stream.Stream.Read(bytes, 0, bytes.Length);

                return Encoding.UTF8.GetString(bytes); ;
            }
            var vat = new VatRate()
            {
                Id = Guid.NewGuid(),
                Name = "Vat",
                Rate = 0.16M
            };

            Products = readFile("products.txt")
                               .Split(Environment.NewLine)
                               .Select((a, i) => (a,i))
                               .Where(a => a.i % 10 == 0)
                               .Select(a => a.a.Split('\t'))
                               .Select(a => new Product
                               {
                                   Id = Guid.NewGuid(),
                                   Name = a[0],
                                   SalePrice = decimal.Parse(a[1]),
                                   VatRate = vat,
                               })
                               .ToArray();

            Customers = readFile("customers.txt")
                               .Split(Environment.NewLine)
                               .Select((a, i) => (a, i))
                               .Where(a => a.i % 10 == 0)
                               .Select(a => a.a.Split('\t'))
                               .Select(a => new Customer
                               {
                                   Id = Guid.NewGuid(),
                                   Name = a[0],
                               })
                               .ToArray();

            Branches = new[]
            {
                new Branch()
                {
                    Name = "HQ",
                    Id = Guid.NewGuid()
                },
                new Branch()
                {
                    Name = "Nairobi Depot",
                    Id = Guid.NewGuid()
                },
            };

            Salespersons = new[]
            {
                new Salesperson()
                {
                    FirstName = "James",
                    LastName = "Bond",
                    Id = Guid.NewGuid()
                },
                new Salesperson()
                {
                    FirstName = "John",
                    LastName = "McClane",
                    Id = Guid.NewGuid()
                }
            };

        }

        public ICollection<Branch> Branches { get;  }

        public ICollection<Salesperson> Salespersons { get;  }

        public ICollection<Customer> Customers { get;  }

        public ICollection<Product> Products { get;  }

        public Sale Model { get; }

        public bool IsReadOnly => false;

        public void AddSaleDetail()
        {

            var item = new SaleDetail();

            Model.SaleDetails
                 .Add(item);
        }
    }

}

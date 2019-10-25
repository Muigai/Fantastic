using Fantastic.Wpf.Example.Controllers;
using Fantastic.Wpf.Example.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fantastic.Wpf.Example.Views
{
    /// <summary>
    /// Interaction logic for Sale.xaml
    /// </summary>
    using Binder = ConventionBinding<SaleViewController, Sale, SaleWindow>;

    public partial class SaleWindow : Window
    {

        private readonly SaleViewController controller;

        private readonly Binder binder;

        public SaleWindow()
        {

            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            InitializeComponent();

            controller = new SaleViewController(new Sale());

            DataContext = controller;

            binder = new Binder();

            binder.Bind(this, controller);

        }

        public void WireUp(object sender, EventArgs e)
        {
            var element = (FrameworkElement)sender;
            binder.WireUp(element, this, controller);
        }

    }
}

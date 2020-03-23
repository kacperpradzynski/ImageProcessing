using ImageProcessingCore.NonLinearFilters;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageProcessingView.Operations
{
    /// <summary>
    /// Logika interakcji dla klasy Sobel.xaml
    /// </summary>
    public partial class Sobel : UserControl, IProcessing
    {
        public Sobel()
        {
            InitializeComponent();
        }

        public IProcessingStrategy GetOperationStrategy()
        {
            return new SobelOperator();
        }
    }
}

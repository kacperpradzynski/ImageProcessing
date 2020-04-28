using ImageProcessingCore.FourierOperations;
using ImageProcessingCore.Strategy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Logika interakcji dla klasy SpectrumFilter.xaml
    /// </summary>
    public partial class SpectrumFilter : UserControl, IProcessing, INotifyPropertyChanged
    {
        public SpectrumFilter()
        {
            InitializeComponent();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9.-]");
            e.Handled = regex.IsMatch(e.Text);
        }

        public IProcessingStrategy GetOperationStrategy()
        {
            int val = Int32.Parse(RadiusModificationSpectrum.Text);
            double val2 = Double.Parse(AngleModification.Text);
            double val3 = Double.Parse(AngleOffsetModification.Text);
            return new SpectrumFilterOperator(val, val2, val3);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

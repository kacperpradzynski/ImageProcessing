using ImageProcessingCore.BasicImageOperations;
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
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ImageProcessingView.Operations
{
    /// <summary>
    /// Logika interakcji dla klasy Brightness.xaml
    /// </summary>
    public partial class Brightness : UserControl, IProcessing, INotifyPropertyChanged
    {
        public Brightness()
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
            int val = Int32.Parse(BrightnessModification.Text);
            return new BrightnessOperator(val);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using ImageProcessingCore.Histogram;
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
    /// Logika interakcji dla klasy HistogramRayleigh.xaml
    /// </summary>
    public partial class HistogramRayleigh : UserControl, IProcessing, INotifyPropertyChanged
    {
        private bool _isRGB;

        public bool IsRGB
        {
            get { return _isRGB; }
            set
            {
                if (_isRGB != value)
                {
                    _isRGB = value;
                    OnPropertyChanged();
                    ChangeColor();
                }
            }
        }
        public HistogramRayleigh()
        {
            InitializeComponent();
            DataContext = this;
            _isRGB = true;
            IsRGB = false;
            RedMin.Text = "0";
            RedMax.Text = "255";
            GreenMin.Text = "0";
            GreenMax.Text = "255";
            BlueMin.Text = "0";
            BlueMax.Text = "255";
            GrayMin.Text = "0";
            GrayMax.Text = "255";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        public IProcessingStrategy GetOperationStrategy()
        {
            int minR, maxR, minG, maxG, minB, maxB;
            if (IsRGB)
            {
                minR = Int32.Parse(RedMin.Text);
                maxR = Int32.Parse(RedMax.Text);
                minG = Int32.Parse(GreenMin.Text);
                maxG = Int32.Parse(GreenMax.Text);
                minB = Int32.Parse(BlueMin.Text);
                maxB = Int32.Parse(BlueMax.Text);
            } else
            {
                minR = Int32.Parse(GrayMin.Text);
                maxR = Int32.Parse(GrayMax.Text);
                minG = Int32.Parse(GrayMin.Text);
                maxG = Int32.Parse(GrayMax.Text);
                minB = Int32.Parse(GrayMin.Text);
                maxB = Int32.Parse(GrayMax.Text);
            }
            return new RaleighOperator(minR, maxR, minG, maxG, minB, maxB);
        }

        private void ChangeColor()
        {
            GrayGroup.Visibility = Visibility.Collapsed;
            RedGroup.Visibility = Visibility.Collapsed;
            GreenGroup.Visibility = Visibility.Collapsed;
            BlueGroup.Visibility = Visibility.Collapsed;

            if (_isRGB)
            {
                RedGroup.Visibility = Visibility.Visible;
                GreenGroup.Visibility = Visibility.Visible;
                BlueGroup.Visibility = Visibility.Visible;
            } else
            {
                GrayGroup.Visibility = Visibility.Visible;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

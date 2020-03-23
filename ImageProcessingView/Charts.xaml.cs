using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using LiveCharts;
using LiveCharts.Wpf;

namespace ImageProcessingView
{
    /// <summary>
    /// Logika interakcji dla klasy Charts.xaml
    /// </summary>
    public partial class Charts : UserControl, INotifyPropertyChanged
    {
        private SeriesCollection _inputRed, _inputGreen, _inputBlue, _outputRed, _outputGreen, _outputBlue;

        public SeriesCollection InputRed
        {
            get { return _inputRed; }
            set
            {
                if (_inputRed != value)
                {
                    _inputRed = value;
                    OnPropertyChanged();
                }
            }
        }
        public SeriesCollection InputGreen
        {
            get { return _inputGreen; }
            set
            {
                if (_inputGreen != value)
                {
                    _inputGreen = value;
                    OnPropertyChanged();
                }
            }
        }
        public SeriesCollection InputBlue
        {
            get { return _inputBlue; }
            set
            {
                if (_inputBlue != value)
                {
                    _inputBlue = value;
                    OnPropertyChanged();
                }
            }
        }
        public SeriesCollection OutputRed
        {
            get { return _outputRed; }
            set
            {
                if (_outputRed != value)
                {
                    _outputRed = value;
                    OnPropertyChanged();
                }
            }
        }
        public SeriesCollection OutputGreen
        {
            get { return _outputGreen; }
            set
            {
                if (_outputGreen != value)
                {
                    _outputGreen = value;
                    OnPropertyChanged();
                }
            }
        }
        public SeriesCollection OutputBlue
        {
            get { return _outputBlue; }
            set
            {
                if (_outputBlue != value)
                {
                    _outputBlue = value;
                    OnPropertyChanged();
                }
            }
        }

        public Charts()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

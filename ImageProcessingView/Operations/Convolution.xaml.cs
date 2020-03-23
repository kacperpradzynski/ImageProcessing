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
    /// Logika interakcji dla klasy Convolution.xaml
    /// </summary>
    public partial class Convolution : UserControl, IProcessing, INotifyPropertyChanged
    {
        private string[][] _string2DArray { get; set; }
        public string[][] String2DArray
        {
            get { return _string2DArray; }
            set
            {
                if (_string2DArray != value)
                {
                    _string2DArray = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _maskSize { get; set; }
        public string MaskSize
        {
            get { return _maskSize; }
            set
            {
                if (_maskSize != value)
                {
                    _maskSize = value;
                    OnPropertyChanged();
                    ChangeArraySize();
                }
            }
        }
        public Convolution()
        {
            InitializeComponent();
            MaskSize = "3";
            DataContext = this;
        }

        private void ChangeArraySize()
        {
            int size = Int32.Parse(_maskSize);
            List<string[]> list = new List<string[]>();
            string[] row;
            string[][] column = new string[size][];
            for (int i = 0; i < size; i++)
            {
                row = new string[size];
                for (int j = 0; j < size; j++)
                {
                    row[j] = "1";
                }
                list.Add(row);
            }
            for (int i = 0; i < size; i++)
            {
                column[i] = list[i];
            }
            String2DArray = column;
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]");
            e.Handled = regex.IsMatch(e.Text);
        }

        public IProcessingStrategy GetOperationStrategy()
        {
            int[] mask = new int[String2DArray.Length * String2DArray.Length];
            for (int i = 0; i < String2DArray.Length; i++)
            {
                for (int j = 0; j < String2DArray.Length; j++)
                {
                    mask[i * String2DArray.Length + j] = Int32.Parse(String2DArray[i][j]);
                }
            }
            return new ImageProcessingCore.LinearFilters.ConvolutionFilter(mask);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

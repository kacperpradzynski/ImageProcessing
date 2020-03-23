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

namespace ImageProcessingView
{
    /// <summary>
    /// Logika interakcji dla klasy Images.xaml
    /// </summary>
    public partial class Images : UserControl, INotifyPropertyChanged
    {
        private BitmapImage _bitmapImageInput, _bitmapImageOutput;

        public BitmapImage BitmapImageInput
        {
            get { return _bitmapImageInput; }
            set
            {
                if (_bitmapImageInput != value)
                {
                    _bitmapImageInput = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage BitmapImageOutput
        {
            get { return _bitmapImageOutput; }
            set
            {
                if (_bitmapImageOutput != value)
                {
                    _bitmapImageOutput = value;
                    OnPropertyChanged();
                }
            }
        }
        public Images()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

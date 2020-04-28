using ImageProcessingCore.Segmentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
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
    /// Logika interakcji dla klasy SegmentationView.xaml
    /// </summary>
    public partial class SegmentationView : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<MaskModel> _masks;
        private BitmapImage _bitmapImageInput, _bitmapImageOutput, _bitmapImageWithMask;
        public Bitmap inputWithMask, output;
        private Bitmap input;

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

        public ObservableCollection<MaskModel> Masks
        {
            get { return _masks; }
            set
            {
                if (_masks != value)
                {
                    _masks = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage BitmapImageWithMask
        {
            get { return _bitmapImageWithMask; }
            set
            {
                if (_bitmapImageWithMask != value)
                {
                    _bitmapImageWithMask = value;
                    OnPropertyChanged();
                }
            }
        }
        public SegmentationView()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void ApplyMask_Click(object sender, RoutedEventArgs e)
        {
            Button selectedMask = sender as Button;
            inputWithMask = SegmentationOperator.ApplyMask(input, selectedMask.CommandParameter as Bitmap);
            BitmapImageWithMask = BitmapToBitmapImage(inputWithMask);
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void InitializeMasks(List<Bitmap> bitmapMasks, Bitmap input, Bitmap output)
        {
            this.input = input;
            this.output = output;
            BitmapImageOutput = BitmapToBitmapImage(output);
            Masks = new ObservableCollection<MaskModel>();
            foreach (Bitmap bmp in bitmapMasks)
            {
                Masks.Add(new MaskModel { Bitmap = bmp, Image = BitmapToBitmapImage(bmp) });
            }
            inputWithMask = SegmentationOperator.ApplyMask(input, Masks.First().Bitmap);
            BitmapImageWithMask = BitmapToBitmapImage(inputWithMask);
        }
    }
}

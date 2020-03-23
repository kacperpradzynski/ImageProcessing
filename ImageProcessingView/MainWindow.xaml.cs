using ImageProcessingCore.Histogram;
using Microsoft.Win32;
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
using LiveCharts;
using LiveCharts.Wpf;
using ImageProcessingCore.Strategy;
using ImageProcessingView.Operations;
using ImageProcessingCore.Helpers;

namespace ImageProcessingView
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ObservableCollection<string> _operations;
        private string _selectedOperation;
        private UserControl currentOperation;
        private BitmapImage _bitmapImageInput, _bitmapImageOutput;
        public Bitmap input, output;
        private bool _imagesView, _chartsView, unlocked;
        public ImageProcessor processor;

        public ObservableCollection<string> Operations
        {
            get { return _operations; }
            set
            {
                if (_operations != value)
                {
                    _operations = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedOperation
        {
            get { return _selectedOperation; }
            set
            {
                if (_selectedOperation != value)
                {
                    _selectedOperation = value;
                    OnPropertyChanged();
                    currentOperation = GetActiveUserControl(_selectedOperation);
                    ChangeActiveOperation();
                }
            }
        }

        public bool ImagesView
        {
            get { return _imagesView; }
            set
            {
                if (_imagesView != value)
                {
                    _imagesView = value;
                    OnPropertyChanged();
                    ChangeActiveView();
                }
            }
        }
        public bool ChartsView
        {
            get { return _chartsView; }
            set
            {
                if (_chartsView != value)
                {
                    _chartsView = value;
                    OnPropertyChanged();
                    ChangeActiveView();
                }
            }
        }

        public BitmapImage BitmapImageInput
        {
            get { return _bitmapImageInput; }
            set
            {
                if (_bitmapImageInput != value)
                {
                    _bitmapImageInput = value;
                    OnPropertyChanged();
                    ImagesUserControl.BitmapImageInput = value;
                    HistogramRayleigh.IsRGB = ImageHelper.GetBitsPerPixel(input.PixelFormat) > 8;
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
                    ImagesUserControl.BitmapImageOutput = value;
                }
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Operations = new ObservableCollection<string>() { "Brightness", "Contrast", "Negative" , "Median Filter", "Average Filter", "Convolution Filter", "Sobel Filter", "Raleigh Probability Density" };
            SelectedOperation = Operations[0];
            this.unlocked = true;
            ImagesView = true;
        }

        private void ChangeActiveOperation()
        {
            Brightness.Visibility = Visibility.Collapsed;
            Contrast.Visibility = Visibility.Collapsed;
            Negative.Visibility = Visibility.Collapsed;
            MedianFilter.Visibility = Visibility.Collapsed;
            AverageFilter.Visibility = Visibility.Collapsed;
            ConvolutionFilter.Visibility = Visibility.Collapsed;
            SobelFilter.Visibility = Visibility.Collapsed;
            HistogramRayleigh.Visibility = Visibility.Collapsed;

            currentOperation.Visibility = Visibility.Visible;
        }

        private void ChangeActiveView()
        {
            ImagesUserControl.Visibility = Visibility.Collapsed;
            ChartsUserControl.Visibility = Visibility.Collapsed;

            if(ImagesView && !ChartsView)
            {
                ImagesUserControl.Visibility = Visibility.Visible;
            } else if (!ImagesView && ChartsView)
            {
                ChartsUserControl.Visibility = Visibility.Visible;
            }
        }

        private UserControl GetActiveUserControl(string operation)
        {
            switch (operation)
            {
                case "Brightness":
                    return Brightness;
                case "Contrast":
                    return Contrast;
                case "Negative":
                    return Negative;
                case "Median Filter":
                    return MedianFilter;
                case "Average Filter":
                    return AverageFilter;
                case "Convolution Filter":
                    return ConvolutionFilter;
                case "Sobel Filter":
                    return SobelFilter;
                case "Raleigh Probability Density":
                    return HistogramRayleigh;
                default:
                    return Negative;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG",
                RestoreDirectory = true
            };
            var result = ofd.ShowDialog();
            if (result == true)
            {
                input = new Bitmap(ofd.FileName);
                BitmapImageInput = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute));
                ShowOnHistogram(true);
                ApplyButton.IsEnabled = true;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (output != null)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    FileName = "Output",
                    Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG",
                    RestoreDirectory = true
                };
                var result = sfd.ShowDialog();
                if (result == true)
                {
                    output.Save(sfd.FileName);
                }
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            processor = new ImageProcessor(((IProcessing)currentOperation).GetOperationStrategy(), input);
            if (input != null && unlocked)
            {
                Task.Run(() => Process());
            }
        }

        internal void Process()
        {
            unlocked = false;
            output = processor.Process();
            MemoryStream ms = new MemoryStream();
            output.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            image.Freeze();
            BitmapImageOutput = image;
            
            this.Dispatcher.Invoke(() => {
                SaveButton.IsEnabled = true;
                ShowOnHistogram(false);
            });
            unlocked = true;
        }

        private void ShowOnHistogram(bool isInputImage)
        {
            List<int[]> histogram = HistogramGenerator.Generate(isInputImage ? input : output);
            SolidColorBrush redColor, greenColor, blueColor;
            string redTitle, greenTitle, blueTitle;

            if (ImageHelper.GetBitsPerPixel(isInputImage ? input.PixelFormat : output.PixelFormat) > 8)
            {
                redColor = System.Windows.Media.Brushes.Red;
                greenColor = System.Windows.Media.Brushes.Green;
                blueColor = System.Windows.Media.Brushes.Blue;
                redTitle = "Red";
                greenTitle = "Green";
                blueTitle = "Blue";
            } else
            {
                redColor = System.Windows.Media.Brushes.Gray;
                greenColor = System.Windows.Media.Brushes.Gray;
                blueColor = System.Windows.Media.Brushes.Gray;
                redTitle = "Gray";
                greenTitle = "Gray";
                blueTitle = "Gray";
            }
            
            SeriesCollection red = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = redTitle,
                    Values = new ChartValues<double> {},
                    ColumnPadding = 0,
                    Fill = redColor
                }
            };
            SeriesCollection green = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = greenTitle,
                    Values = new ChartValues<double> {},
                    ColumnPadding = 0,
                    Fill = greenColor
                }
            };
            SeriesCollection blue = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = blueTitle,
                    Values = new ChartValues<double> {},
                    ColumnPadding = 0,
                    Fill = blueColor
                }
            };

            for (int i=0; i<histogram[0].Length; i++)
            {
                red[0].Values.Add((double)histogram[0][i]);
                green[0].Values.Add((double)histogram[1][i]);
                blue[0].Values.Add((double)histogram[2][i]);
            }

            if (isInputImage)
            {
                ChartsUserControl.InputRed = red;
                ChartsUserControl.InputGreen = green;
                ChartsUserControl.InputBlue = blue;
            } else
            {
                ChartsUserControl.OutputRed = red;
                ChartsUserControl.OutputGreen = green;
                ChartsUserControl.OutputBlue = blue;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

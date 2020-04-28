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
using ImageProcessingCore;
using System.Globalization;
using System.Threading;
using ImageProcessingCore.Segmentation;

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
        private BitmapImage _bitmapImageInput, _bitmapImageOutput, _bitmapImageInputMagnitude, _bitmapImageOutputMagnitude, _bitmapImageInputPhase, _bitmapImageOutputPhase;
        public Bitmap input, output;
        private bool unlocked, saveUnlocked = false;
        public ImageProcessor processor;
        private string activeView;
        private ImageModel inputImageModel, outputImageModel;


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

        public ImageModel InputImageModel
        {
            get { return inputImageModel; }
            set
            {
                if (inputImageModel != value)
                {
                    inputImageModel = value;
                }
            }
        }

        public ImageModel OutputImageModel
        {
            get { return outputImageModel; }
            set
            {
                if (outputImageModel != value)
                {
                    outputImageModel = value;
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
                }
            }
        }

        public BitmapImage BitmapImageInputMagnitude
        {
            get { return _bitmapImageInputMagnitude; }
            set
            {
                if (_bitmapImageInputMagnitude != value)
                {
                    _bitmapImageInputMagnitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage BitmapImageOutputMagnitude
        {
            get { return _bitmapImageOutputMagnitude; }
            set
            {
                if (_bitmapImageOutputMagnitude != value)
                {
                    _bitmapImageOutputMagnitude = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage BitmapImageInputPhase
        {
            get { return _bitmapImageInputPhase; }
            set
            {
                if (_bitmapImageInputPhase != value)
                {
                    _bitmapImageInputPhase = value;
                    OnPropertyChanged();
                }
            }
        }

        public BitmapImage BitmapImageOutputPhase
        {
            get { return _bitmapImageOutputPhase; }
            set
            {
                if (_bitmapImageOutputPhase != value)
                {
                    _bitmapImageOutputPhase = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            activeView = "_Image (Spatial Domain)";
            Operations = new ObservableCollection<string>() { "Brightness", "Contrast", "Negative" , "Median Filter", "Average Filter", "Convolution Filter", "Sobel Filter", "Raleigh Probability Density", "Low Pass Filter", "High Pass Filter", "Band Pass Filter", "Band Stop Filter", "Spectrum Filter", "Phase Modification", "Segmentation (Region Growing)" };
            SelectedOperation = Operations[0];
            this.unlocked = true;
            ImagesUserControl.Visibility = Visibility.Visible;
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
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
            LowPassFilter.Visibility = Visibility.Collapsed;
            HighPassFilter.Visibility = Visibility.Collapsed;
            BandPassFilter.Visibility = Visibility.Collapsed;
            BandStopFilter.Visibility = Visibility.Collapsed;
            SpectrumFilter.Visibility = Visibility.Collapsed;
            PhaseModification.Visibility = Visibility.Collapsed;
            Segmentation.Visibility = Visibility.Collapsed;

            currentOperation.Visibility = Visibility.Visible;
        }

        private void ChangeActiveViewToSegmentation()
        {
            activeView = "_Segmentation";
            ViewButton.IsEnabled = false;
            MagnitudeButton.IsEnabled = false;
            PhaseButton.IsEnabled = false;

            ImagesUserControl.Visibility = Visibility.Collapsed;
            ChartsUserControl.Visibility = Visibility.Collapsed;
            FrequencyMagnitudeUserControl.Visibility = Visibility.Collapsed;
            FrequencyPhaseUserControl.Visibility = Visibility.Collapsed;

            SegmentationUserControl.Visibility = Visibility.Visible;

            MasksButton.Visibility = Visibility.Visible;
            ImageWithMaskButton.Visibility = Visibility.Visible;
            SegmentedImageButton.Visibility = Visibility.Visible;
            SegmentationSeparator.Visibility = Visibility.Visible;

            SaveButton.IsEnabled = false;
        }

        public void ChangeActiveView(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            
            if (!activeView.Equals(menuItem.Header.ToString()))
            {
                activeView = menuItem.Header.ToString();

                ImagesUserControl.Visibility = Visibility.Collapsed;
                ChartsUserControl.Visibility = Visibility.Collapsed;
                FrequencyMagnitudeUserControl.Visibility = Visibility.Collapsed;
                FrequencyPhaseUserControl.Visibility = Visibility.Collapsed;
                SegmentationUserControl.Visibility = Visibility.Collapsed;

                switch (activeView)
                {
                    case "_Image (Spatial Domain)":
                        ImagesUserControl.Visibility = Visibility.Visible;
                        break;
                    case "_Histogram (Spatial Domain)":
                        ChartsUserControl.Visibility = Visibility.Visible;
                        break;
                    case "_Magnitude (Frequency Domain)":
                        FrequencyMagnitudeUserControl.Visibility = Visibility.Visible;
                        break;
                    case "_Phase (Frequency Domain)":
                        FrequencyPhaseUserControl.Visibility = Visibility.Visible;
                        break;
                    default:
                        ImagesUserControl.Visibility = Visibility.Visible;
                        break;
                }
            }
        }

        private UserControl GetActiveUserControl(string operation)
        {
            if (!operation.Equals("Segmentation") && activeView.Equals("_Segmentation"))
            {
                activeView = "_Image (Spatial Domain)";

                ImagesUserControl.Visibility = Visibility.Collapsed;
                ChartsUserControl.Visibility = Visibility.Collapsed;
                FrequencyMagnitudeUserControl.Visibility = Visibility.Collapsed;
                FrequencyPhaseUserControl.Visibility = Visibility.Collapsed;
                SegmentationUserControl.Visibility = Visibility.Collapsed;

                ViewButton.IsEnabled = true;
                MagnitudeButton.IsEnabled = true;
                PhaseButton.IsEnabled = true;
                ImagesUserControl.Visibility = Visibility.Visible;

                MasksButton.Visibility = Visibility.Collapsed;
                ImageWithMaskButton.Visibility = Visibility.Collapsed;
                SegmentedImageButton.Visibility = Visibility.Collapsed;
                SegmentationSeparator.Visibility = Visibility.Collapsed;

                SaveButton.IsEnabled = saveUnlocked;
            }

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
                case "Low Pass Filter":
                    return LowPassFilter;
                case "High Pass Filter":
                    return HighPassFilter;
                case "Band Pass Filter":
                    return BandPassFilter;
                case "Band Stop Filter":
                    return BandStopFilter;
                case "Spectrum Filter":
                    return SpectrumFilter;
                case "Phase Modification":
                    return PhaseModification;
                case "Segmentation (Region Growing)":
                    ChangeActiveViewToSegmentation();
                    return Segmentation;
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
                Bitmap loadedImage = new Bitmap(ofd.FileName);
                if (ImageHelper.GetBitsPerPixel(loadedImage.PixelFormat) == 1)
                {
                    input = ImageHelper.ConvertToPixelFormat(loadedImage, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                } else
                {
                    input = loadedImage;
                }
                InputImageModel = new ImageModel(input);
                BitmapImageInputMagnitude = BitmapToBitmapImage(InputImageModel.MagnitudeImage);
                BitmapImageInputPhase = BitmapToBitmapImage(InputImageModel.PhaseImage);
                BitmapImageInput = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute));
                SegmentationUserControl.BitmapImageInput = BitmapImageInput;
                ShowOnHistogram(true);
                ApplyButton.IsEnabled = true;
                MagnitudeButton.IsEnabled = true;
                MagnitudeInputButton.IsEnabled = true;
                PhaseButton.IsEnabled = true;
                PhaseInputButton.IsEnabled = true;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Bitmap imageToSave;
            string fileName;
            switch (((MenuItem)e.Source).Name)
            {
                case "SaveButton":
                    imageToSave = output;
                    fileName = "Output";
                    break;
                case "MagnitudeInputButton":
                    imageToSave = inputImageModel.MagnitudeImage;
                    fileName = "Magnitude";
                    break;
                case "MagnitudeOutputButton":
                    imageToSave = outputImageModel.MagnitudeImage;
                    fileName = "Magnitude";
                    break;
                case "PhaseInputButton":
                    imageToSave = inputImageModel.PhaseImage;
                    fileName = "Phase";
                    break;
                case "PhaseOutputButton":
                    imageToSave = outputImageModel.PhaseImage;
                    fileName = "Phase";
                    break;
                case "ImageWithMaskButton":
                    imageToSave = SegmentationUserControl.inputWithMask;
                    fileName = "ImageWithMask";
                    break;
                case "SegmentedImageButton":
                    imageToSave = SegmentationUserControl.output;
                    fileName = "SegmentedImage";
                    break;
                case "MasksButton":
                    SaveMasks();
                    return;
                default:
                    imageToSave = input;
                    fileName = "Input";
                    break;
            }
            if (imageToSave != null)
            {
                SaveFileDialog sfd = new SaveFileDialog()
                {
                    FileName = fileName,
                    Filter = "Image Files (*.bmp;*.jpg;*.jpeg,*.png)|*.BMP;*.JPG;*.JPEG;*.PNG",
                    RestoreDirectory = true
                };
                var result = sfd.ShowDialog();
                if (result == true)
                {
                    imageToSave.Save(sfd.FileName);
                }
            }
        }

        private void SaveMasks()
        {
            string path = ".";
            System.Windows.Forms.FolderBrowserDialog folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = folderDialog.SelectedPath;
                int counter = 1;
                foreach (var mask in SegmentationUserControl.Masks)
                {
                    mask.Bitmap.Save($"{path}/{counter++}_mask.png");
                }
            }
        }

        private void Apply_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                processor = new ImageProcessor(((IProcessing)currentOperation).GetOperationStrategy(), inputImageModel);
                if (input != null && unlocked)
                {
                    Task.Run(() => Process());
                }
            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message, currentOperation.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        internal void Process()
        {
            unlocked = false;
            OutputImageModel = processor.Process();
            output = OutputImageModel.SpatialDomain;
            BitmapImageOutputMagnitude = BitmapToBitmapImage(OutputImageModel.MagnitudeImage);
            BitmapImageOutputPhase = BitmapToBitmapImage(OutputImageModel.PhaseImage);
            BitmapImageOutput = BitmapToBitmapImage(output);

            if (activeView.Equals("_Segmentation"))
            {
                this.Dispatcher.Invoke(() => {
                    List<Bitmap> bitmapMasks = ((SegmentationOperator)processor.strategy).masks;
                    SegmentationUserControl.InitializeMasks(bitmapMasks, input, output);

                    MasksButton.IsEnabled = true;
                    ImageWithMaskButton.IsEnabled = true;
                    SegmentedImageButton.IsEnabled = true;
                });
                
            }

            this.Dispatcher.Invoke(() => {
                if (!activeView.Equals("_Segmentation"))
                {
                    SaveButton.IsEnabled = true;
                }
                saveUnlocked = true;
                MagnitudeOutputButton.IsEnabled = true;
                PhaseOutputButton.IsEnabled = true;
                ShowOnHistogram(false);
            });
            unlocked = true;
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

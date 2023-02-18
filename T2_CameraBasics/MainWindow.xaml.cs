https://tutorcs.com
WeChat: cstutorcs
QQ: 749389476
Email: tutorcs@163.com
﻿using System;
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

using Microsoft.Kinect;
using System.IO;

namespace T2_CameraBasics
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor sensor = null;

        private FrameDescription colorFrameDescription = null;
        private byte[] colorData = null;

        private WriteableBitmap colorImageBitmap = null;

        // class exercise
        private int offsetRed = 0;
        private int offsetGreen = 0;
        private int offsetBlue = 0;

        private BitmapSource pictureBitmap = null;
        private bool takePicture = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("window loaded");

            sensor = KinectSensor.GetDefault();
            // sensor.Open();

            ColorFrameReader colorFrameReader = sensor.ColorFrameSource.OpenReader();

            buttonStop.Click += ButtonStop_Click;

            colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;

            // -------------------------------------------------
            // allocate storage for color data
            // get default frame description 
            // colorFrameDescription = sensor.ColorFrameSource.FrameDescription; // Raw: Yuy2 (2 byptes per pixel)

            // It's more common to use Bgra color model 
            colorFrameDescription = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // intermediate storage for receiving frame data from the sensor 
            colorData = new byte[colorFrameDescription.LengthInPixels * colorFrameDescription.BytesPerPixel];

            // -------------------------------------------------
            // Create an image buffer 
            colorImageBitmap = new WriteableBitmap(
                      colorFrameDescription.Width,
                      colorFrameDescription.Height,
                      96, // dpi-x
                      96, // dpi-y
                      PixelFormats.Bgr32, // pixel format  
                      null);

            kinectVideo.Source = colorImageBitmap;
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            // System.Console.WriteLine("frame arrived");

            // using statement automatically takes care of disposing of 
            // the ColorFrame object when you are done using it
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame == null) return;

                colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Bgra);

                // perform image processing before writing the image data into colorImageBitmap

                for (uint i = 0; i < colorData.Length; i += colorFrameDescription.BytesPerPixel /*4*/)
                {
                    // colorData[i] += 50; // might cause an overflow problem

                    //colorData[i] = (byte)~colorData[i]; // blue
                    //colorData[i + 1] = (byte)~colorData[i + 1]; // green
                    //colorData[i + 2] = (byte)~colorData[i + 2]; // red

                    // class exercise               
                    int newV = colorData[i] + offsetBlue; // blue
                    if (newV < 0) newV = 0;
                    else if (newV > 255) newV = 255;
                    colorData[i] = (byte)newV;

                    newV = colorData[i + 1] + offsetGreen; // green
                    if (newV < 0) newV = 0;
                    else if (newV > 255) newV = 255;
                    colorData[i + 1] = (byte)newV;

                    newV = colorData[i + 2] + offsetRed; // red
                    if (newV < 0) newV = 0;
                    else if (newV > 255) newV = 255;
                    colorData[i + 2] = (byte)newV;
                }

                // save image 
                if (takePicture) // save image 
                {
                    pictureBitmap = BitmapSource.Create(
                        colorFrameDescription.Width, colorFrameDescription.Height, 96, 96, PixelFormats.Bgr32, null,
                        colorData, colorFrameDescription.Width * (int)colorFrameDescription.BytesPerPixel);

                    takePicture = false;
                }

                // write the image data into colorImageBitmap
                colorImageBitmap.WritePixels(
                  new Int32Rect(0, 0,
                  colorFrameDescription.Width, colorFrameDescription.Height), // source rect
                  colorData, // pixel data
                             // stride: width in bytes of a single row of pixel data
                  colorFrameDescription.Width * (int)(colorFrameDescription.BytesPerPixel),
                  0 // offset 
               );
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            sensor.Close();
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            System.Console.WriteLine("start button clicked");

            // MessageBox.Show("start button clicked", "camera basics");
            sensor.Open();
        }

        private void SliderRed_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            offsetRed = (int)sliderRed.Value;
        }

        private void SliderGreen_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            offsetGreen = (int)sliderGreen.Value;
        }

        private void SliderBlue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            offsetBlue = (int)sliderBlue.Value;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Set the flag to trigger a snapshot
            takePicture = true;

            // Configure save file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "SnapShot"; // Default file name
            dlg.DefaultExt = ".jpg"; // Default file extension
            dlg.Filter = "Pictures (.jpg)|*.jpg"; // Filter files by extension

            // Process save file dialog box results
            if (dlg.ShowDialog() == true)
            {
                // Save document
                string filename = dlg.FileName;
                // add using System.IO to the top of the program 
                using (FileStream stream = new FileStream(filename, FileMode.Create))
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(pictureBitmap));
                    encoder.Save(stream);
                }
            }
        }
    }
}

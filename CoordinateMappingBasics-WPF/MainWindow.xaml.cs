//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.CoordinateMappingBasics
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Threading;
    using Microsoft.Kinect;




    /// <summary>
    /// The test class for our example.
    /// </summary>
    class ThreadArgument
    {
        public WriteableBitmap wbm { get; set; }
        public string path { get; set; }
    }






    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        private const string OBJECT = "Test";
        private const string SAVE_PATH = "C:/Users/ammirato/Documents/KinectData/" + OBJECT + "/";

        private const string RGB_EXTENSION = "rgb";
        private const string UNREG_DEPTH_EXTENSION = "unreg_rawdepth";
        private const string REG_DEPTH_EXTENSION = "rawdepth";


        private const string SAVE_PATH_RGB = SAVE_PATH + "/" + RGB_EXTENSION+ "/";
        private const string SAVE_PATH_UNREG_DEPTH = SAVE_PATH + "/" + UNREG_DEPTH_EXTENSION + "/";
        private const string SAVE_PATH_REG_DEPTH = SAVE_PATH + "/" + REG_DEPTH_EXTENSION + "/";



        private BackgroundWorker backgroundWorker;

        //this dont work, out of memory at ~100(is it acutally or jsut thinks it is?)
        private WriteableBitmap[] colorWbms;
        private int rgbCounter = 0;



        // <summary>
        /// writes to output files
        /// </summary>
        private MATWriter matfw = null;


        /// <summary>
        /// Size of the RGB pixel in the bitmap
        /// </summary>
        private readonly int bytesPerPixel = (PixelFormats.Bgr32.BitsPerPixel + 7) / 8;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for depth/color/body index frames
        /// </summary>
        private MultiSourceFrameReader multiFrameSourceReader = null;

        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap bitmap = null;

        /// <summary>
        /// The size in bytes of the bitmap back buffer
        /// </summary>
        private uint bitmapBackBufferSize = 0;

        /// <summary>
        /// Intermediate storage for the color to depth mapping
        /// </summary>
        private DepthSpacePoint[] colorMappedToDepthPoints = null;

        private bool gotDepthRegistration = false;


        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {

            this.backgroundWorker = new BackgroundWorker();
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.DoWork +=
            new DoWorkEventHandler(this.backgroundWorker_DoWork);
            this.backgroundWorker.ProgressChanged +=
                new ProgressChangedEventHandler(this.backgroundWorker_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted +=
                new RunWorkerCompletedEventHandler(this.backgroundWorker_RunWorkerCompleted);

            this.kinectSensor = KinectSensor.GetDefault();

            System.IO.Directory.CreateDirectory(SAVE_PATH);
            System.IO.Directory.CreateDirectory(SAVE_PATH_RGB);
            System.IO.Directory.CreateDirectory(SAVE_PATH_UNREG_DEPTH);
            System.IO.Directory.CreateDirectory(SAVE_PATH_REG_DEPTH);


            this.colorWbms = new WriteableBitmap[1000];

            this.multiFrameSourceReader = this.kinectSensor.OpenMultiSourceFrameReader(FrameSourceTypes.Depth | FrameSourceTypes.Color | FrameSourceTypes.BodyIndex);

            this.multiFrameSourceReader.MultiSourceFrameArrived += this.Reader_MultiSourceFrameArrived;

            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            FrameDescription depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            int depthWidth = depthFrameDescription.Width;
            int depthHeight = depthFrameDescription.Height;

            FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.FrameDescription;

            int colorWidth = colorFrameDescription.Width;
            int colorHeight = colorFrameDescription.Height;

            this.colorMappedToDepthPoints = new DepthSpacePoint[colorWidth * colorHeight];

            this.bitmap = new WriteableBitmap(colorWidth, colorHeight, 96.0, 96.0, PixelFormats.Bgra32, null);
            
            // Calculate the WriteableBitmap back buffer size
            this.bitmapBackBufferSize = (uint)((this.bitmap.BackBufferStride * (this.bitmap.PixelHeight - 1)) + (this.bitmap.PixelWidth * this.bytesPerPixel));
                                   
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            this.kinectSensor.Open();

            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            this.DataContext = this;

            this.InitializeComponent();
        }












        /// <summary>
        /// INotifyPropertyChangedPropertyChanged event to allow window controls to bind to changeable data
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets the bitmap to display
        /// </summary>
        public ImageSource ImageSource
        {
            get
            {
                return this.bitmap;
            }
        }

        /// <summary>
        /// Gets or sets the current status text to display
        /// </summary>
        public string StatusText
        {
            get
            {
                return this.statusText;
            }

            set
            {
                if (this.statusText != value)
                {
                    this.statusText = value;

                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.multiFrameSourceReader != null)
            {
                // MultiSourceFrameReder is IDisposable
                this.multiFrameSourceReader.Dispose();
                this.multiFrameSourceReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }


            //  ******************* SAVE RGB  AT ENDDDDDDDDD ************************


            for (int i = 0; i < rgbCounter; i++)
            {
                WriteableBitmap wbm = colorWbms[i];
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder3 = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder3.Frames.Add(BitmapFrame.Create(wbm));


                string path3 = SAVE_PATH_RGB + OBJECT + Convert.ToInt64(i).ToString("D12") + "_" + RGB_EXTENSION + ".png";

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path3, FileMode.Create))
                    {

                            encoder3.Save(fs);
                    }
                }
                catch (IOException)
                {
                    //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
            }







            //  ******************* SAVE RGB  AT ENDDDDDDDDD************************


        }//end closing

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
         /*   // Create a render target to which we'll render our composite image
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)CompositeImage.ActualWidth, (int)CompositeImage.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);

            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                VisualBrush brush = new VisualBrush(CompositeImage);
                dc.DrawRectangle(brush, null, new Rect(new Point(), new Size(CompositeImage.ActualWidth, CompositeImage.ActualHeight)));
            }

            renderBitmap.Render(dv);

            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

            string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            string path = Path.Combine(myPhotos, "KinectScreenshot-CoordinateMapping-" + time + ".png");

            // Write the new file to disk
            try
            {
                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    encoder.Save(fs);
                }

                this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
            }
            catch (IOException)
            {
                this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
            }
          * */
        }


























        /// <summary>
        /// Handles the depth/color/body index frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            int depthWidth = 0;
            int depthHeight = 0;
                    
            DepthFrame depthFrame = null;
            ColorFrame colorFrame = null;
            BodyIndexFrame bodyIndexFrame = null;
            bool isBitmapLocked = false;

            MultiSourceFrame multiSourceFrame = e.FrameReference.AcquireFrame();           

            // If the Frame has expired by the time we process this event, return.
            if (multiSourceFrame == null)
            {
                return;
            }

            // We use a try/finally to ensure that we clean up before we exit the function.  
            // This includes calling Dispose on any Frame objects that we may have and unlocking the bitmap back buffer.
            try
            {                
                depthFrame = multiSourceFrame.DepthFrameReference.AcquireFrame();
                colorFrame = multiSourceFrame.ColorFrameReference.AcquireFrame();
                bodyIndexFrame = multiSourceFrame.BodyIndexFrameReference.AcquireFrame();

                // If any frame has expired by the time we process this event, return.
                // The "finally" statement will Dispose any that are not null.
                if ((depthFrame == null) || (colorFrame == null) || (bodyIndexFrame == null))
                {
                    return;
                }


               
                FrameDescription depthFrameDescription = depthFrame.FrameDescription;

                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;
                
                // Process Depth
                //FrameDescription depthFrameDescription = depthFrame.FrameDescription;

                depthWidth = depthFrameDescription.Width;
                depthHeight = depthFrameDescription.Height;

                // Access the depth frame data directly via LockImageBuffer to avoid making a copy
                using (KinectBuffer depthFrameData = depthFrame.LockImageBuffer())
                {
                    this.coordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(
                        depthFrameData.UnderlyingBuffer,
                        depthFrameData.Size,
                        this.colorMappedToDepthPoints);


                    //ushort [] data = new ushort[depthWidth*depthHeight];
                   // depthFrame.CopyFrameDataToArray(data);
                   // string filePath = SAVE_PATH + OBJECT + Convert.ToInt64(depthFrame.RelativeTime.TotalMilliseconds).ToString("D12") + ".MAT";
                   // this.matfw = new MATWriter("depthmat", filePath, data, depthFrameDescription.Height, depthFrameDescription.Width);

                }
                if (!this.gotDepthRegistration)
                {

                    //string toWrite = new string();
                    var csv = new System.Text.StringBuilder();
                    foreach (DepthSpacePoint dp in colorMappedToDepthPoints)
                    {
                        var x = dp.X.ToString();
                        var y = dp.Y.ToString();
                        var newLine = string.Format("{0},{1}{2}", x, y, Environment.NewLine);
                        csv.Append(newLine); 
                    }

                    File.WriteAllText((SAVE_PATH_REG_DEPTH +"/mapping.csv"), csv.ToString());
                    this.gotDepthRegistration = true;
                }


                

                // ********************************** UNREG DEPTH  *****************************

                WriteableBitmap unregDeptWbm = new WriteableBitmap(depthWidth, depthHeight, 96.0, 96.0, PixelFormats.Gray16, null);
                depthFrame.CopyFrameDataToIntPtr(
                                    unregDeptWbm.BackBuffer,
                                    (uint)(depthWidth * depthHeight * 2));

                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(unregDeptWbm));


                string path = SAVE_PATH_UNREG_DEPTH + OBJECT + Convert.ToInt64(depthFrame.RelativeTime.TotalMilliseconds).ToString("D12") + "_" + UNREG_DEPTH_EXTENSION + ".png";
               
                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(path, FileMode.Create))
                    {
                        //encoder.Save(fs);
                    }
                }
                catch (IOException)
                {
                    //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }
                         

                
                // ********************************** END UNREG DEPTH  *****************************





                // ********************************** RGB    *****************************

                // Process Color

                // Lock the bitmap for writing
                this.bitmap.Lock();
                isBitmapLocked = true;
                Console.Write(rgbCounter);
                Console.Write("\n");

                //save the colorframe
                WriteableBitmap rgbWbm = new WriteableBitmap(colorFrame.FrameDescription.Width, colorFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
                colorFrame.CopyConvertedFrameDataToIntPtr(
                                        rgbWbm.BackBuffer,
                                        (uint)(colorFrame.FrameDescription.Width * colorFrame.FrameDescription.Height * 4),
                                        ColorImageFormat.Bgra);

                //colorWbms[rgbCounter] = rgbWbm;
                //rgbCounter += 1;
               // wbm.AddDirtyRect(new Int32Rect(0, 0, wbm.PixelWidth, wbm.PixelHeight));

                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder3 = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder3.Frames.Add(BitmapFrame.Create(rgbWbm));


                string path3 = SAVE_PATH_RGB + OBJECT + Convert.ToInt64(colorFrame.RelativeTime.TotalMilliseconds).ToString("D12") + "_"+ RGB_EXTENSION + ".png";

                // write the new file to disk
                try
                {
                    if (true)
                    //if (this.backgroundWorker.IsBusy)
                    {
                        // FileStream is IDisposable
                        using (FileStream fs = new FileStream(path3, FileMode.Create))
                        {
                            this.Dispatcher.Invoke((Action)(() =>
    {
                                    encoder3.Save(fs);
    }));
                            
                        }
                    }
                    else
                    {
                        //Mutex mu = new Mutex(false);
                        
                        ThreadArgument arg = new ThreadArgument { wbm = rgbWbm.Clone(), path =path3 };
                        this.backgroundWorker.RunWorkerAsync(arg);
                    }
                    
                }
                catch (IOException)
                {
                    //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                }


                // ********************************** END RGB  *****************************











                // ********************************** REGISTRATION  *****************************
                
                
                // We'll access the body index data directly to avoid a copy
                using (KinectBuffer bodyIndexData = bodyIndexFrame.LockImageBuffer())
                {
                    unsafe
                    {
                        byte* bodyIndexDataPointer = (byte*)bodyIndexData.UnderlyingBuffer;

                        int colorMappedToDepthPointCount = this.colorMappedToDepthPoints.Length;

                        ushort[] colorDepthMap = new ushort[colorMappedToDepthPointCount];

                        fixed (DepthSpacePoint* colorMappedToDepthPointsPointer = this.colorMappedToDepthPoints)
                        {
                            // Treat the color data as 4-byte pixels
                            uint* bitmapPixelsPointer = (uint*)this.bitmap.BackBuffer;

                            ushort [] depthMap = new ushort[depthWidth*depthHeight];
                            depthFrame.CopyFrameDataToArray(depthMap);

                            // Loop over each row and column of the color image
                            // Zero out any pixels that don't correspond to a body index
                            for (int colorIndex = 0; colorIndex < colorMappedToDepthPointCount; ++colorIndex)
                            {
                                float colorMappedToDepthX = colorMappedToDepthPointsPointer[colorIndex].X;
                                float colorMappedToDepthY = colorMappedToDepthPointsPointer[colorIndex].Y;

                                // The sentinel value is -inf, -inf, meaning that no depth pixel corresponds to this color pixel.
                                if (!float.IsNegativeInfinity(colorMappedToDepthX) &&
                                    !float.IsNegativeInfinity(colorMappedToDepthY))
                                {
                                    // Make sure the depth pixel maps to a valid point in color space
                                    int depthX = (int)(colorMappedToDepthX + 0.5f);
                                    int depthY = (int)(colorMappedToDepthY + 0.5f);

                                    // If the point is not valid, there is no body index there.
                                    if ((depthX >= 0) && (depthX < depthWidth) && (depthY >= 0) && (depthY < depthHeight))
                                    {
                                        int depthIndex = (depthY * depthWidth) + depthX;
                                        
                                        colorDepthMap[colorIndex] = depthMap[depthIndex];

                                        // If we are tracking a body for the current pixel, do not zero out the pixel
                                       // if (bodyIndexDataPointer[depthIndex] != 0xff)
                                        //{
                                       //     continue;
                                       // }
                                    }
                                }

                               // bitmapPixelsPointer[colorIndex] = 0;
                            }




                            
                            WriteableBitmap wbm2 = new WriteableBitmap(colorFrame.FrameDescription.Width, colorFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray16, null);
                            wbm2.WritePixels(new Int32Rect(0, 0, colorFrame.FrameDescription.Width, colorFrame.FrameDescription.Height), colorDepthMap, colorFrame.FrameDescription.Width*2, 0);

                            
                            // create a png bitmap encoder which knows how to save a .png file
                            BitmapEncoder encoder2 = new PngBitmapEncoder();
                            
                            // create frame from the writable bitmap and add to encoder
                            encoder2.Frames.Add(BitmapFrame.Create(wbm2));


                            string path2 = SAVE_PATH_REG_DEPTH + OBJECT + Convert.ToInt64(depthFrame.RelativeTime.TotalMilliseconds).ToString("D12") + "_" + REG_DEPTH_EXTENSION+ ".png";

                            // write the new file to disk
                            try
                            {
                                // FileStream is IDisposable
                                using (FileStream fs = new FileStream(path2, FileMode.Create))
                                {
                                   // encoder2.Save(fs);
                                }
                            }
                            catch (IOException)
                            {
                                //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                            }


                        }//fixed

                        this.bitmap.AddDirtyRect(new Int32Rect(0, 0, this.bitmap.PixelWidth, this.bitmap.PixelHeight));
                    }
                } 
                 
                 // ************************************* END REGISTRATION ******************************* 
            }
            finally
            {
                if (isBitmapLocked)
                {
                    this.bitmap.Unlock();
                }

                if (depthFrame != null)
                {
                    depthFrame.Dispose();
                }

                if (colorFrame != null)
                {
                    colorFrame.Dispose();
                }

                if (bodyIndexFrame != null)
                {
                    bodyIndexFrame.Dispose();
                }
            }
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }







        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            //for (int i = 1; (i <= 10); i++)
           // {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    //break;
                }
                else
                {

                    this.Dispatcher.Invoke((Action)(() =>
    {
        // your code here.


                    // Perform a time consuming operation and report progress.
                    ThreadArgument arg = (ThreadArgument)e.Argument;


                    // create a png bitmap encoder which knows how to save a .png file
                    BitmapEncoder encoder = new PngBitmapEncoder();

                    WriteableBitmap wbm = arg.wbm;
                    //wbm.TryLock(new System.Windows.Duration(new TimeSpan(0,0,1)));

                    // create frame from the writable bitmap and add to encoder
                    encoder.Frames.Add(BitmapFrame.Create(wbm));
                    // FileStream is IDisposable
                    using (FileStream fs = new FileStream(arg.path, FileMode.Create))
                    {
                               encoder.Save(fs);
                    }


  }));

                }
           // }
        }













        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Console.Write(e.ProgressPercentage.ToString() + "%");
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if ((e.Cancelled == true))
            {
                Console.Write("Canceled!");
            }

            else if (!(e.Error == null))
            {
                Console.Write("Error: " + e.Error.Message);
            }

            else
            {
                Console.Write("Done!");
            }
        }
    }
}

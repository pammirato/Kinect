//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
namespace Microsoft.Samples.Kinect.ColorBasics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    //using System.Windows.Forms;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;
    using System.Runtime.Serialization.Formatters.Binary;


    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string TIMESTAMP = "timestamp";
        private const string OBJECT = "Test";
        private const string SAVE_PATH = "C:/Users/ammirato/Documents/KinectData/" + OBJECT + "/";

        private bool captureColor = true;
        private bool captureDepth = true;
        private bool captureBody = false;

        private bool saveColorDataAtEnd = false;
        private bool batchSaveColorData = false;
        private bool saveBodyDataAtEnd = true;
        private bool saveDepthDataAtEnd = true;
        
        
        private int colorFramesNotSaved = 0;
        private const int colorBatchSize = 2;

        private List<BitmapEncoder> colorEncodersList = new List<BitmapEncoder>();

        //to save colorBatchSize colorFrames at a time
        private BitmapEncoder [] colorEncoders = new BitmapEncoder[colorBatchSize];
        private long[] colorTimeStamps = new long[colorBatchSize];

        //to save colorImages
        BitmapEncoder colorEncoder = new PngBitmapEncoder();


        /// <summary>
        /// Radius of drawn hand circles
        /// </summary>
        private const double HandSize = 30;

        /// <summary>
        /// Thickness of drawn joint lines
        /// </summary>
        private const double JointThickness = 3;

        /// <summary>
        /// Thickness of clip edge rectangles
        /// </summary>
        private const double ClipBoundsThickness = 10;

        /// <summary>
        /// Constant for clamping Z values of camera space points from being negative
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as closed
        /// </summary>
        private readonly Brush handClosedBrush = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as opened
        /// </summary>
        private readonly Brush handOpenBrush = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));

        /// <summary>
        /// Brush used for drawing hands that are currently tracked as in lasso (pointer) position
        /// </summary>
        private readonly Brush handLassoBrush = new SolidColorBrush(Color.FromArgb(128, 0, 0, 255));

        /// <summary>
        /// Brush used for drawing joints that are currently tracked
        /// </summary>
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));

        /// <summary>
        /// Brush used for drawing joints that are currently inferred
        /// </summary>        
        private readonly Brush inferredJointBrush = Brushes.Yellow;

        /// <summary>
        /// Pen used for drawing bones that are currently inferred
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// Drawing group for body rendering output
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// Drawing image that we will display
        /// </summary>
        private DrawingImage imageSource;


        /// <summary>
        /// Coordinate mapper to map one type of point to another
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// Reader for body frames
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// Array for the bodies
        /// </summary>
        private Body[] bodies = null;

        /// <summary>
        /// definition of bones
        /// </summary>
        private List<Tuple<JointType, JointType>> bones;

        /// <summary>
        /// Width of display (depth space)
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// Height of display (depth space)
        /// </summary>
        private int displayHeight;

        /// <summary>
        /// List of colors for each body tracked
        /// </summary>
        private List<Pen> bodyColors;


        private Dictionary<string, string> [] bodyTrackers;
        //private Dictionary<string, string> bodyTrackingData;













        private Stopwatch stopWatch = null;

        private int counter = 0;

        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

        /// <summary>
        /// Reader for color frames
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// Reader for depth frames
        /// </summary>
        private DepthFrameReader depthFrameReader = null;

        /// <summary>
        /// Description of the data contained in the depth frame
        /// </summary>
        private FrameDescription depthFrameDescription = null;

        
        
        // <summary>
        /// writes to output files
        /// </summary>
        private MATWriter matfw = null;

        /// <summary>
        /// String path to save the framefiles
        /// </summary>
        private string filePath = null;

        public int count = 0;


        /// <summary>
        /// Intermediate storage for frame data
        /// </summary>
        public ushort[] depthFrameData = null;

        // holds all the depth data to write at the end
        private List<ushort[]> allDepthData;
        private List<ColorSpacePoint[]> allColorSpacePoints;

        private List<long> depthTimeStamps;
        private List<long> colorTimeStamps2;

        //private List<ColorFrame> allColorData;


        /// <summary>
        /// Bitmap to display
        /// </summary>
        private WriteableBitmap colorBitmap = null;

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            if (!this.batchSaveColorData)
            {
                this.saveBodyDataAtEnd = false;
            }



           // this.KeyDown += new KeyEventHandler(this.Form1_KeyPress);
            System.IO.Directory.CreateDirectory(SAVE_PATH);


            this.stopWatch = new Stopwatch();
            stopWatch.Start();

            // get the kinectSensor object
            this.kinectSensor = KinectSensor.GetDefault();

            // open the reader for the color frames
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();


            if (captureDepth)
            {
                // open the reader for the depth frames
                this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
                this.depthFrameReader.FrameArrived += this.Depth_Reader_FrameArrived;
                // get FrameDescription from DepthFrameSource
                this.depthFrameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
                // allocate space to put the pixels being received and converted
                this.depthFrameData = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height];
                this.allDepthData = new List<ushort[]>();
                this.allColorSpacePoints = new List<ColorSpacePoint[]>();

                this.depthTimeStamps = new List<long>();
                this.colorTimeStamps2 = new List<long>();
            }//if captureDepth

            if (captureColor)
            {
                // wire handler for frame arrival
                this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

                // create the colorFrameDescription from the ColorFrameSource using Bgra format
                FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

                // create the bitmap to display
                this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            }//if capture color


            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();


            this.KeyUp += new System.Windows.Input.KeyEventHandler(tb_KeyDown);

            


            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

            if (captureBody)
            {
                // open the reader for the body frames
                this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();

                // a bone defined as a line between two joints
                this.bones = new List<Tuple<JointType, JointType>>();

                // Torso
                this.bones.Add(new Tuple<JointType, JointType>(JointType.Head, JointType.Neck));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.ShoulderLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.SpineBase, JointType.HipLeft));

                // Right Arm
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.ElbowRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.WristRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.HandRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HandRight, JointType.HandTipRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristRight, JointType.ThumbRight));

                // Left Arm
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.ElbowLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.WristLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.HandLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HandLeft, JointType.HandTipLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ThumbLeft));

                // Right Leg
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HipRight, JointType.KneeRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeRight, JointType.AnkleRight));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.FootRight));

                // Left Leg
                this.bones.Add(new Tuple<JointType, JointType>(JointType.HipLeft, JointType.KneeLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.AnkleLeft));
                this.bones.Add(new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.FootLeft));

                // populate body colors, one for each BodyIndex
                this.bodyColors = new List<Pen>();

                this.bodyColors.Add(new Pen(Brushes.Red, 6));
                this.bodyColors.Add(new Pen(Brushes.Orange, 6));
                this.bodyColors.Add(new Pen(Brushes.Green, 6));
                this.bodyColors.Add(new Pen(Brushes.Blue, 6));
                this.bodyColors.Add(new Pen(Brushes.Indigo, 6));
                this.bodyColors.Add(new Pen(Brushes.Violet, 6));

                // Create the drawing group we'll use for drawing
                this.drawingGroup = new DrawingGroup();

                // Create an image source that we can use in our image control
                //this.imageSource = new DrawingImage(this.drawingGroup);

                // use the window object as the view model in this simple example
                //this.DataContext = this;
                this.bodyTrackers = new Dictionary<string, string>[6];
                //this.bodyTrackingData = new Dictionary<string,string>();


                if (this.bodyFrameReader != null)
                {
                    this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
                }
            }//if capture body
            

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
                return this.colorBitmap;
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

                    // notify any bound elements that the text has changed
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs("StatusText"));
                    }
                }
            }
        }




        private void tb_KeyDown(object sender, KeyEventArgs e)
        {

            char x = (char)e.Key;

            if (e.Key == Key.D)
            {
                CancelEventArgs args = new CancelEventArgs();
                
                //MainWindow_Closing(this, args);
                this.Close();
            }

        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            closeProgram();
        }




        private void closeProgram()
        {

            if (this.colorFrameReader != null)
            {
                // ColorFrameReder is IDisposable
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }


            stopWatch.Stop();
            stopWatch.Reset();

            //write out the bidy info
            // int i = bodyTrackers.GetLength();

            this.StatusText = "Saving Body";

            if (saveBodyDataAtEnd && captureBody)
            {


                string allData;
                for (int i = 0; i < bodyTrackers.GetLength(0); i++)//for each body
                {
                    if (bodyTrackers[i] == null)
                    {
                        continue;//skip empty things
                    }
                    allData = "";
                    foreach (KeyValuePair<string, string> kvp in bodyTrackers[i])//for ech joint
                    {
                        //write all the data for that joint
                        allData += kvp.Key + "  " + kvp.Value + System.Environment.NewLine;
                    }

                    if (allData != null)
                    {
                        //write the data for that body to a file.
                        System.IO.File.WriteAllText(@SAVE_PATH + "Body" + i.ToString() + ".txt", allData);
                    }
                }//end for
            }


            this.StatusText = "Saving Depth";

            //long[] timeStamps = this.depthTimeStamps.ToArray();
            if (saveDepthDataAtEnd && captureDepth)
            {
                counter = 0;

                foreach (ushort[] data in allDepthData)
                {
                    /*filePath = SAVE_PATH + OBJECT + this.depthTimeStamps[counter].ToString("D12") + ".bin";
                    //FileStream is IDisposable
                    using (FileStream fs = new FileStream(filePath, FileMode.Create))
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        try
                        {                
                              formatter.Serialize(fs, data);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Failed to serialize. Reason: " + e.Message);
                            throw;
                        }
                        finally
                        {
                            fs.Close();
                        }
                    }*/
                    filePath = SAVE_PATH + OBJECT + this.depthTimeStamps[counter].ToString("D12") + ".MAT";
                    this.matfw = new MATWriter("depthmat", filePath, data, depthFrameDescription.Height, depthFrameDescription.Width);

                    counter++;
                }
            }


            this.StatusText = "Saving Color";
            if (saveColorDataAtEnd && captureColor)
            {
                counter = 0;
                foreach (BitmapEncoder be in this.colorEncodersList)
                {


                    //x = ts.Milliseconds;
                    string path = SAVE_PATH + OBJECT + this.colorTimeStamps2[counter].ToString("D12") + ".png";// Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");
                    counter = counter + 1;

                    //filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/KinectToMatLab" + "/Depth" + time.ToString() + ".MAT";
                    // write the new file to disk
                    try
                    {
                        //FileStream is IDisposable
                        using (FileStream fs = new FileStream(path, FileMode.Create))
                        {
                            be.Save(fs);
                        }

                        //this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
                    }
                    catch (IOException)
                    {
                        this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                    }
                }//for each bitmap in allColorData

            }//if savecolorData

            Console.Out.WriteLine("the end.");

        }//close program

        /// <summary>
        /// Handles the user clicking on the screenshot button
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void ScreenshotButton_Click(object sender, RoutedEventArgs e)
        {
        /*    if (this.colorBitmap != null)
            {
                // create a png bitmap encoder which knows how to save a .png file
                BitmapEncoder encoder = new PngBitmapEncoder();

                // create frame from the writable bitmap and add to encoder
                encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));

                string time = System.DateTime.Now.ToString("hh'-'mm'-'ss", CultureInfo.CurrentUICulture.DateTimeFormat);

                string myPhotos = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

                string path = Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");

                // write the new file to disk
                try
                {
                    // FileStream is IDisposable
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
            }*/
        } //screenshot button click 

        /// <summary>
        /// Handles the color frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
           
            
            // ColorFrame is IDisposable
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {

                    
                    if(true)//else
                    {
                        FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                        WriteableBitmap wbm = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

                        using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                        {
                            this.colorBitmap.Lock();

                            if (!saveColorDataAtEnd)
                            {
                                // verify data and write the new color frame data to the display bitmap
                                if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
                                {
                                    colorFrame.CopyConvertedFrameDataToIntPtr(
                                        this.colorBitmap.BackBuffer,
                                        (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                        ColorImageFormat.Bgra);

                                    this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight));

                                }


                                this.colorBitmap.Unlock();

                            }
                            // -------------------------------------------------------

                            if (saveColorDataAtEnd)
                            {


                                wbm.Lock();

                                // verify data and write the new color frame data to the display bitmap
                                if ((colorFrameDescription.Width ==wbm.PixelWidth) && (colorFrameDescription.Height == wbm.PixelHeight))
                                {
                                    

                                    colorFrame.CopyConvertedFrameDataToIntPtr(
                                        wbm.BackBuffer,
                                        (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                        ColorImageFormat.Bgra);

                                    wbm.AddDirtyRect(new Int32Rect(0, 0, wbm.PixelWidth, wbm.PixelHeight));
                                }


                                wbm.Unlock();
                               // this.colorBitmap = wbm;
                            }






                            //save the color file
                            if (this.colorBitmap != null || wbm != null)
                            {

                                if (saveColorDataAtEnd)
                                {
                                    //put it here to write out later
                                    //allColorData.Add(colorFrame);
                                    BitmapEncoder temp = new PngBitmapEncoder();
                                    temp.Frames.Add(BitmapFrame.Create(wbm));
                                    this.colorEncodersList.Add(temp);
                                    this.colorTimeStamps2.Add(Convert.ToInt64(colorFrame.RelativeTime.TotalMilliseconds));
                                    return;
                                }

                                if (!saveColorDataAtEnd)
                                {
                                    if (!batchSaveColorData)
                                    {
                                        // create a png bitmap encoder which knows how to save a .png file
                                        BitmapEncoder encoder = new PngBitmapEncoder();


                                        //encoder.Frames.
                                        // create frame from the writable bitmap and add to encoder
                                        encoder.Frames.Add(BitmapFrame.Create(this.colorBitmap));


                                        string path = SAVE_PATH + OBJECT + Convert.ToInt64(colorFrame.RelativeTime.TotalMilliseconds).ToString("D12") + ".png";
                                        //int time = depthFrame.RelativeTime.Milliseconds;
                                        //filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/KinectToMatLab" + "/Depth" + time.ToString() + ".MAT";
                                        // write the new file to disk
                                        try
                                        {
                                            // FileStream is IDisposable
                                            using (FileStream fs = new FileStream(path, FileMode.Create))
                                            {
                                                encoder.Save(fs);
                                            }
                                        }
                                        catch (IOException)
                                        {
                                            //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                                        }

                                        Predicate<ushort []> predicate = FindDepth;
                                        DepthSpacePoint [] mappedPoints = new DepthSpacePoint[colorFrameDescription.Width*colorFrameDescription.Height];
                                        this.kinectSensor.CoordinateMapper.MapColorFrameToDepthSpace(allDepthData.FindLast(predicate), mappedPoints);
                                    }
                                    else
                                    {





                                        if (this.colorFramesNotSaved >= colorBatchSize)
                                        {
                                            for (int i = 0; i < colorBatchSize; i++) // (BitmapEncoder be in this.colorEncoders)
                                            {
                                                string fpath = SAVE_PATH + OBJECT + this.colorTimeStamps[i].ToString("D12") + ".png";// Path.Combine(myPhotos, "KinectScreenshot-Color-" + time + ".png");
                                                counter = counter + 1;

                                                try
                                                {
                                                    // FileStream is IDisposable
                                                    using (FileStream fs = new FileStream(fpath, FileMode.Create))
                                                    {
                                                        this.colorEncoders[i].Save(fs);
                                                    }
                                                }
                                                catch (IOException)
                                                {
                                                    //this.StatusText = string.Format(Properties.Resources.FailedScreenshotStatusTextFormat, path);
                                                }
                                                colorFramesNotSaved--;//we just saved a frame
                                            }//for each bitmap encoder  

                                        }//if not saved 5 or more

                                        this.colorEncoders[colorFramesNotSaved] = new PngBitmapEncoder();
                                        this.colorEncoders[colorFramesNotSaved].Frames.Add(BitmapFrame.Create(this.colorBitmap.Clone()));
                                        this.colorTimeStamps[colorFramesNotSaved] = Convert.ToInt64(colorFrame.RelativeTime.TotalMilliseconds);//stopWatch.ElapsedMilliseconds;
                                        colorFramesNotSaved++;

                                        // this.StatusText = string.Format(Properties.Resources.SavedScreenshotStatusTextFormat, path);
                                    }
                                }
                            }


                        }//if colorbitmap != null
                    }//else save colordataatend
                }//if colorframe
            }//using
        }//color frame arrived





        private static bool FindDepth(ushort [] obj)
        {
            return true;
        }

        /// <summary>
        /// Handles the event which the sensor becomes unavailable (E.g. paused, closed, unplugged).
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Sensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            // on failure, set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.SensorNotAvailableStatusText;
        }



        /// <summary>
        /// Handles the depth frame data arriving from the sensor
        /// </summary>
        private void Depth_Reader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
        {
            if (true)//(this.StatusText == Properties.Resources.RunningStatusText)
            {
               
                using (DepthFrame depthFrame = e.FrameReference.AcquireFrame())
                {
                    if (depthFrame != null)
                    {
                        // the fastest way to process the body index data is to directly access 
                        // the underlying buffer
                        using (Microsoft.Kinect.KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
                        {

                            

                            {
                                this.depthFrameData = new ushort[this.depthFrameDescription.Width * this.depthFrameDescription.Height]; ;
                                //get the depth data
                                depthFrame.CopyFrameDataToArray(depthFrameData);


                                if (saveDepthDataAtEnd)
                                {
                                    //map depth to color space
                                    ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[depthFrameData.Length];
                                    this.kinectSensor.CoordinateMapper.MapDepthFrameToColorSpace(depthFrameData, colorSpacePoints);

                                    CameraSpacePoint[] cameraSpacePoints = new CameraSpacePoint[depthFrameData.Length];
                                    this.kinectSensor.CoordinateMapper.MapDepthFrameToCameraSpace(depthFrameData, cameraSpacePoints);
                                    allColorSpacePoints.Add(colorSpacePoints);


                                    allDepthData.Add(depthFrameData);
                                    this.depthTimeStamps.Add(Convert.ToInt64(depthFrame.RelativeTime.TotalMilliseconds));//stopWatch.ElapsedMilliseconds);
                                }
                                else//save now
                                {
                                    int time = depthFrame.RelativeTime.Milliseconds;
                                   
                                    filePath = SAVE_PATH + OBJECT + Convert.ToInt64(depthFrame.RelativeTime.TotalMilliseconds).ToString("D12")  + ".MAT";
                                    counter = counter + 1;
                                    this.matfw = new MATWriter("depthmat", filePath, depthFrameData, depthFrame.FrameDescription.Height, depthFrame.FrameDescription.Width);
                                    
                                }   //save
                            }//nothing
                        }//using buffer
                    }//if farme != null
                }//using depthframe
            }//if(true)
            else
            {
                //SaveParamsToFile(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/KinectToMatLab" + "/Intrinsic parameters.txt");
                //this.StatusText = Properties.Resources.SensorIsAvailableStatusText;
            }
        }//method depth frame arrived












        /// <summary>
        /// Handles the body frame data arriving from the sensor
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;

            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true;
                }
            }

            if (dataReceived)
            {
                using (DrawingContext dc = this.drawingGroup.Open())
                {
                    // Draw a transparent background to set the render size
                    dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                    int penIndex = 0;
                    int bodynum = 1;
                    foreach (Body body in this.bodies)
                    {
                        Pen drawPen = this.bodyColors[penIndex++];

                        if (body.IsTracked)
                        {
                            this.DrawClippedEdges(body, dc);

                            IReadOnlyDictionary<JointType, Joint> joints = body.Joints;

                            // convert the joint points to depth (display) space
                            Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();

                            foreach (JointType jointType in joints.Keys)
                            {
                                // sometimes the depth(Z) of an inferred joint may show as negative
                                // clamp down to 0.1f to prevent coordinatemapper from returning (-Infinity, -Infinity)
                                CameraSpacePoint position = joints[jointType].Position;
                                if (position.Z < 0)
                                {
                                    position.Z = InferredZPositionClamp;
                                }

                                DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                                jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                            }

                            this.DrawBody(joints, jointPoints, dc, drawPen,bodynum);
                            bodynum = bodynum + 1;
                            
                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                    }

                    // prevent drawing outside of our render area
                    this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                }
            }
        }

        /// <summary>
        /// Draws a body
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// <param name="drawingPen">specifies color to draw a specific body</param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen, int bodynum)
        {

            //System.IO.File.WriteAllLines(@"C:\Users\Public\TestFolder\WriteLines.txt", lines);
            
            

            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }


            Dictionary<string,string> currentBodyDict;
            bool first = false;

            if (bodyTrackers[bodynum] != null)
            {
                currentBodyDict = bodyTrackers[bodynum];
            }
            else
            {
                currentBodyDict = new Dictionary<string,string>();
                bodyTrackers[bodynum] = currentBodyDict;
                first = true;
            }


            // for recording data
            string x = "";
            
            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

                if (saveBodyDataAtEnd)
                {
                    //this is the first time we are writing data for this body, so put the enrty in the dict.
                    if (first)
                    {
                        currentBodyDict.Add(jointType.ToString(), jointPoints[jointType].ToString());
                    }//end if first
                    else
                    {
                        string data = currentBodyDict[jointType.ToString()]; ///get the data so far
                        data += "-" + jointPoints[jointType].ToString(); //append the new data
                        currentBodyDict[jointType.ToString()] = data;//replace the old data
                    }
                }//if saveatend
                else
                {
                    x += jointType.ToString() + " " + jointPoints[jointType].ToString() + System.Environment.NewLine;
                }
                


                TrackingState trackingState = joints[jointType].TrackingState;

                if (trackingState == TrackingState.Tracked)
                {
                    drawBrush = this.trackedJointBrush;
                }
                else if (trackingState == TrackingState.Inferred)
                {
                    drawBrush = this.inferredJointBrush;
                }

                if (drawBrush != null)
                {

                    drawingContext.DrawEllipse(drawBrush, null, jointPoints[jointType], JointThickness, JointThickness);
                }
            }//for each jointtpye


            if (saveBodyDataAtEnd)
            {
                //this is the first time we are writing data for this body, so put the enrty in the dict.
                if (first)
                {
                    currentBodyDict.Add(TIMESTAMP, stopWatch.ElapsedMilliseconds.ToString() + ",-1");
                }//end if first
                else
                {
                    string data = currentBodyDict[TIMESTAMP]; ///get the data so far
                    data += "-" + stopWatch.ElapsedMilliseconds.ToString() + ",-1"; //append the new data
                    currentBodyDict[TIMESTAMP] = data;//replace the old data
                }
            }//if saveatend
            else //(!saveBodyDataAtEnd)//save it now?
            {
                System.IO.File.WriteAllText(@SAVE_PATH + "Body" + bodynum.ToString() + stopWatch.ElapsedMilliseconds.ToString("D12") + ".txt", x);
            }
        }

        /// <summary>
        /// Draws one bone of a body (joint to joint)
        /// </summary>
        /// <param name="joints">joints to draw</param>
        /// <param name="jointPoints">translated positions of joints to draw</param>
        /// <param name="jointType0">first joint of bone to draw</param>
        /// <param name="jointType1">second joint of bone to draw</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        /// /// <param name="drawingPen">specifies color to draw a specific bone</param>
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints[jointType0];
            Joint joint1 = joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked)
            {
                return;
            }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
            {
                drawPen = drawingPen;
            }

            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// Draws a hand symbol if the hand is tracked: red circle = closed, green circle = opened; blue circle = lasso
        /// </summary>
        /// <param name="handState">state of the hand</param>
        /// <param name="handPosition">position of the hand</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawHand(HandState handState, Point handPosition, DrawingContext drawingContext)
        {
            switch (handState)
            {
                case HandState.Closed:
                    drawingContext.DrawEllipse(this.handClosedBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Open:
                    drawingContext.DrawEllipse(this.handOpenBrush, null, handPosition, HandSize, HandSize);
                    break;

                case HandState.Lasso:
                    drawingContext.DrawEllipse(this.handLassoBrush, null, handPosition, HandSize, HandSize);
                    break;
            }
        }

        /// <summary>
        /// Draws indicators to show which edges are clipping body data
        /// </summary>
        /// <param name="body">body to draw clipping information for</param>
        /// <param name="drawingContext">drawing context to draw to</param>
        private void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            FrameEdges clippedEdges = body.ClippedEdges;

            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
            }
        }










    }//stay in
}

//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
//Lines of Code: 24-31,41-72, 194-207 , 414-562 and 600
//Made By Spyridon Couvaras.
//------------------------------------------------------------------------------

namespace Microsoft.Samples.Kinect.BodyBasics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Kinect;


    public static class Globals
    {

        public static Int32 counter =0; 
        public static bool rec = false;  //start/stop recording
        public static bool hands = false; //hands model use boolean
        public static bool head = false;   //head model use boolean
    }



    /// <summary>
    /// Interaction logic for MainWindow
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        void OnClick(object sender, RoutedEventArgs e) //click button to start recording
        {
            if (Globals.rec == false) { Record.Foreground = new SolidColorBrush(Colors.Red); }
            else { Record.Foreground = new SolidColorBrush(Colors.Black); }
           // MessageBox.Show("On Press click event occurs.");
      
            Globals.rec=!(Globals.rec); //HERE WE TRIGGER RECORDING
            Debug.Write("Record: ");
            Debug.WriteLine(Globals.rec);
        }

        void OnClick1(object sender, RoutedEventArgs e) //click button to trigger hand model
        {
            if (Globals.hands == false) { Hands.Foreground = new SolidColorBrush(Colors.Red); }
            else { Hands.Foreground = new SolidColorBrush(Colors.Black); }
            // MessageBox.Show("On Press click event occurs.");

            Globals.hands = !(Globals.hands); //HERE WE TRIGGER hand record
            Debug.Write("Hands: ");
            Debug.WriteLine(Globals.hands);
        }

        void OnClick2(object sender, RoutedEventArgs e) //click button to trigger head model
        {
            if (Globals.head == false) { Head.Foreground = new SolidColorBrush(Colors.Red); }
            else { Head.Foreground = new SolidColorBrush(Colors.Black); }
            // MessageBox.Show("On Press click event occurs.");

            Globals.head = !(Globals.head); //HERE WE TRIGGER head record
            Debug.Write("Head: ");
            Debug.WriteLine(Globals.hands);
        }


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
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor kinectSensor = null;

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

        //recently added
        private int bodyIndex;

        // flag to asses if a body is currently tracked
        private bool bodyTracked = false;
        //above code recently added


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

        /// <summary>
        /// Current status text to display
        /// </summary>
        private string statusText = null;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        /// 
        public MainWindow()
        {
            string root = Directory.GetCurrentDirectory();
            DirectoryInfo di = new DirectoryInfo(root);

            //USED TO INITILIAZE CONNECTION WITH ROS
            MLApp.MLApp matlab = new MLApp.MLApp();
            matlab.Visible = 0;
            string ex0 = matlab.Execute("rosinit('192.168.1.74');"); //CHANGE IP!!!!!, CHECK THE IP OF THE OTHER LISTENER
            string ex1 = matlab.Execute("chatpub = rospublisher('/chatter', 'std_msgs/String');");
            string ex2 = matlab.Execute("msg = rosmessage(chatpub);");


            string changepath = matlab.Execute("cd " + di.Parent.Parent.Parent.FullName); //change MATLAB path to directory that has the functions
           // Debug.WriteLine("EDW");
            //Debug.WriteLine(di.Parent.Parent.Parent.FullName);

            // one sensor is currently supported
            this.kinectSensor = KinectSensor.GetDefault();

            // get the coordinate mapper
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;

            // get the depth (display) extents
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;

            // get size of joint space
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;

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

            // set IsAvailableChanged event notifier
            this.kinectSensor.IsAvailableChanged += this.Sensor_IsAvailableChanged;

            // open the sensor
            this.kinectSensor.Open();

            // set the status text
            this.StatusText = this.kinectSensor.IsAvailable ? Properties.Resources.RunningStatusText
                                                            : Properties.Resources.NoSensorStatusText;

            // Create the drawing group we'll use for drawing
            this.drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // use the window object as the view model in this simple example
            this.DataContext = this;

            // initialize the components (controls) of the window
            this.InitializeComponent();



          //  string what_last = matlab.Execute("rosshutdown");

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
                return this.imageSource;
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

        /// <summary>
        /// Execute start up tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
            }
        }

        /// <summary>
        /// Execute shutdown tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                // BodyFrameReader is IDisposable
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.kinectSensor != null)
            {
                this.kinectSensor.Close();
                this.kinectSensor = null;
            }
        }

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

                    //  public TimeSpan RelativeTime;
                    //Debug.WriteLine(head.Position.X);


                    Body body = null;
                    if (this.bodyTracked) //HERE WE TRACK THE CLOSEST BODY TO THE CAMERA
                    {
                        if (this.bodies[this.bodyIndex].IsTracked)
                        {
                            body = this.bodies[this.bodyIndex];
                        }
                        else
                        {
                            bodyTracked = false;
                        }
                    }
                    if (!bodyTracked)
                    {
                        for (int i = 0; i < this.bodies.Length; ++i)
                        {
                            if (this.bodies[i].IsTracked)
                            {
                                this.bodyIndex = i;
                                this.bodyTracked = true;
                                break;
                            }
                        }
                    }

                    if (body != null && this.bodyTracked && body.IsTracked)
                    {
                        // body represents your single tracked skeleton







                        int penIndex = 0;
  
                        
                        Pen drawPen = this.bodyColors[penIndex++];

                        //GRAB THE JOINTS YOU WANT TO SAVE
                        Joint leftHand = body.Joints[JointType.HandLeft];
                        Joint rightHand = body.Joints[JointType.HandRight];
                        Joint head = body.Joints[JointType.Head];
                        Joint leftshoulder = body.Joints[JointType.ShoulderLeft];
                        Joint rightshoulder = body.Joints[JointType.ShoulderRight];
                        Joint neck = body.Joints[JointType.Neck];

                        //  Debug.WriteLine(Globals.counter);



                        //MATLAB FUNCTION --16/01
                        MLApp.MLApp matlab = new MLApp.MLApp();
                        matlab.Visible = 0;


                        if (Globals.rec == false) { Globals.counter = 0; }

                        //HERE OUR COUNTER VALUE AND THE RECORD BUTTON TRIGGERS THE WRITTING OF DATA
                        if (Globals.counter % 1 == 0 && Globals.rec == true)
                        {


                            //used to gather more data-----NOT USED ANY MORE
                            /* using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Spyros\Desktop\Right.txt", true))
                              {
                                  //file.Write(Globals.counter);
                                  //file.Write(";");

                                  //HEAD-NECK vector

                                  file.Write(head.Position.X - neck.Position.X);
                                  file.Write(",");
                                  file.Write(head.Position.Y - neck.Position.Y);
                                  file.Write(",");
                                  file.Write(head.Position.Z - neck.Position.Z);
                                  file.Write(",");

                                  //Left shoulder-left hand vector
                                  file.Write(leftshoulder.Position.X - leftHand.Position.X);
                                  file.Write(",");
                                  file.Write(leftshoulder.Position.Y - leftHand.Position.Y);
                                  file.Write(",");
                                  file.Write(leftshoulder.Position.Z - leftHand.Position.Z);
                                  file.Write(",");

                                  //right shoulder-right hand vector
                                  file.Write(rightshoulder.Position.X - rightHand.Position.X);
                                  file.Write(",");
                                  file.Write(rightshoulder.Position.Y - rightHand.Position.Y);
                                  file.Write(",");
                                  file.Write(rightshoulder.Position.Z - rightHand.Position.Z);
                                  file.WriteLine("");



                              }*/



                            //RECENTLY ADDED CODE 16/01
                            using (System.IO.StreamWriter file2 = new System.IO.StreamWriter(@"C:\Users\", true))
                            {

                                //string what = matlab.Execute("cd "+ head.Position.X); //CHANGE to location of matlab functions!!!!!

                                //predict movement here

                                //USED FOR ARMS
                                if (Globals.hands == true)
                                {//predict result through matlab
                                    string output = matlab.Execute("y=predmovement_2([" + (leftshoulder.Position.Y - leftHand.Position.Y) + "," + (rightshoulder.Position.X - rightHand.Position.X) + "," + (rightshoulder.Position.Y - rightHand.Position.Y) + "])");
                                }
                                //USED FOR HEAD
                                if (Globals.head == true)
                                {//predict result through matlab
                                    string output = matlab.Execute("y=predmovement([" + (head.Position.X - neck.Position.X) + "," + (head.Position.Y - neck.Position.Y) + "," + (head.Position.Z - neck.Position.Z) + "])");
                                }

                                    double a = matlab.GetVariable("y", "base"); //we get the variable that we want
                                if (a == 0)
                                {
                                    //file2.WriteLine("LEFT OBJECT;");
                                    string what3 = matlab.Execute("msg.Data = '0';"); 

 
                                }
                                else if (a == 1)
                                {
             
                                    string what4 = matlab.Execute("msg.Data = '-1';");
                                   
                                }
                                else if (a == -1)
                                {
                                    //file2.WriteLine("RIGHT OBJECT;");
                                    string what111 = matlab.Execute("msg.Data = '1';");

                                }


                            }

                            //Debug.WriteLine("New data added");
                            //SENT DATA ACROSS NETWORK
                            string what5 = matlab.Execute("send(chatpub,msg);");

                            //Debug.WriteLine(Globals.counter);
                        }

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

                            this.DrawBody(joints, jointPoints, dc, drawPen);

                            this.DrawHand(body.HandLeftState, jointPoints[JointType.HandLeft], dc);
                            this.DrawHand(body.HandRightState, jointPoints[JointType.HandRight], dc);
                        }
                        // prevent drawing outside of our render area
                        this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));



                    }
                    Globals.counter++;




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
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints, IDictionary<JointType, Point> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            // Draw the bones
            foreach (var bone in this.bones)
            {
                this.DrawBone(joints, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }

            // Draw the joints
            foreach (JointType jointType in joints.Keys)
            {
                Brush drawBrush = null;

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

        
    }
}





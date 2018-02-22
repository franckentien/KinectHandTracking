using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectHandTracking
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Members

        private KinectSensor _sensor;
        private MultiSourceFrameReader _reader;
        private IList<Body> _bodies;

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor == null) return;
            _sensor.Open();
            _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color |
                                                         FrameSourceTypes.Depth | FrameSourceTypes.Infrared |
                                                         FrameSourceTypes.Body);
            _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _reader?.Dispose();
            _sensor?.Close();
        }

        private void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            // use for display capture image of sensor on "camera" item
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    Camera.Source = frame.ToBitmap();
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame == null) return;
                // use to reset display canvas, without it all canvas are display since start of launch 
                Canvas.Children.Clear();

                _bodies = new Body[frame.BodyFrameSource.BodyCount];
                frame.GetAndRefreshBodyData(_bodies);

                foreach (var body in _bodies)
                {
                    if (body == null) continue;
                    if (!body.IsTracked) continue;
                    var handRight = body.Joints[JointType.HandRight];
                    var handLeft = body.Joints[JointType.HandLeft];

                    if (handRight.TrackingState != TrackingState.NotTracked)
                    {
                        var handRightPosition = handRight.Position;
                        _sensor.CoordinateMapper.MapCameraPointToColorSpace(handRightPosition);
                        DrawHand(Canvas, handRight, _sensor.CoordinateMapper, body.HandRightState);
                    }
                    if (handLeft.TrackingState == TrackingState.NotTracked) continue;
                    var handLeftPosition = handLeft.Position;
                    _sensor.CoordinateMapper.MapCameraPointToColorSpace(handLeftPosition);
                    DrawHand(Canvas, handLeft, _sensor.CoordinateMapper, body.HandLeftState);
                }
            }
        }

        private static void DrawHand(Panel canvas, Joint hand, CoordinateMapper mapper, HandState handstate)
        {
            //chose image 
            var imgState = "-";
            switch (handstate)
            {
                case HandState.Open:
                    imgState = "img/paper.jpg";
                    break;
                case HandState.Closed:
                    imgState = "img/rock.jpg";
                    break;
                case HandState.Lasso:
                    imgState = "img/scissors.jpg";
                    break;
                case HandState.Unknown:
                    imgState = "img/Unknown.jpg";
                    break;
                case HandState.NotTracked:
                    imgState = "img/NotTracked.jpg";
                    break;
            }
            var image = new Image();
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri("pack://application:,,,/" + imgState);
            bitmapImage.EndInit();
            image.Stretch = Stretch.Fill;
            image.Source = bitmapImage;

            //move image 
            var point = hand.Scale(mapper);
            Canvas.SetLeft(image, point.X - 25);
            Canvas.SetTop(image, point.Y - 25);
            canvas.Children.Add(image);
        }
    }

    #endregion
}
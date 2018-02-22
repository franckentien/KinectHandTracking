using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;

namespace KinectHandTracking
{
    public static class Extensions
    {
        #region Camera

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            var width = frame.FrameDescription.Width;
            var height = frame.FrameDescription.Height;
            var format = PixelFormats.Bgr32;

            var pixels = new byte[width*height*((format.BitsPerPixel + 7)/8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            var stride = width*format.BitsPerPixel/8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body

        public static Point Scale(this Joint joint, CoordinateMapper mapper)
        {
            var point = new Point();

            var colorPoint = mapper.MapCameraPointToColorSpace(joint.Position);
            point.X = float.IsInfinity(colorPoint.X) ? 0.0 : colorPoint.X;
            point.Y = float.IsInfinity(colorPoint.Y) ? 0.0 : colorPoint.Y;

            return point;
        }

        #endregion
    }
}
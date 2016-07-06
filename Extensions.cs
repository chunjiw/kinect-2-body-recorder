using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KinectStreams
{
    public static class Extensions
    {
        #region Camera

        public static ImageSource ToBitmap(this ColorFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            byte[] pixels = new byte[width * height * ((format.BitsPerPixel + 7) / 8)];

            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToArray(pixels);
            }
            else
            {
                frame.CopyConvertedFrameDataToArray(pixels, ColorImageFormat.Bgra);
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this DepthFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort minDepth = frame.DepthMinReliableDistance;
            ushort maxDepth = frame.DepthMaxReliableDistance;

            ushort[] pixelData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(pixelData);

            int colorIndex = 0;
            for (int depthIndex = 0; depthIndex < pixelData.Length; ++depthIndex)
            {
                ushort depth = pixelData[depthIndex];

                byte intensity = (byte)(depth >= minDepth && depth <= maxDepth ? depth : 0);

                pixels[colorIndex++] = intensity; // Blue
                pixels[colorIndex++] = intensity; // Green
                pixels[colorIndex++] = intensity; // Red

                ++colorIndex;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        public static ImageSource ToBitmap(this InfraredFrame frame)
        {
            int width = frame.FrameDescription.Width;
            int height = frame.FrameDescription.Height;
            PixelFormat format = PixelFormats.Bgr32;

            ushort[] frameData = new ushort[width * height];
            byte[] pixels = new byte[width * height * (format.BitsPerPixel + 7) / 8];

            frame.CopyFrameDataToArray(frameData);

            int colorIndex = 0;
            for (int infraredIndex = 0; infraredIndex < frameData.Length; infraredIndex++)
            {
                ushort ir = frameData[infraredIndex];

                byte intensity = (byte)(ir >> 7);

                pixels[colorIndex++] = (byte)(intensity / 1); // Blue
                pixels[colorIndex++] = (byte)(intensity / 1); // Green   
                pixels[colorIndex++] = (byte)(intensity / 0.4); // Red

                colorIndex++;
            }

            int stride = width * format.BitsPerPixel / 8;

            return BitmapSource.Create(width, height, 96, 96, format, null, pixels, stride);
        }

        #endregion

        #region Body

        public static Joint ScaleTo(this Joint joint, double width, double height, float skeletonMaxX, float skeletonMaxY)
        {
            joint.Position = new CameraSpacePoint
            {
                X = Scale(width, skeletonMaxX, joint.Position.X),
                Y = Scale(height, skeletonMaxY, -joint.Position.Y),
                Z = joint.Position.Z
            };

            return joint;
        }

        public static Joint ScaleTo(this Joint joint, double width, double height)
        {
            return ScaleTo(joint, width, height, 1.0f, 1.0f);
        }

        private static float Scale(double maxPixel, double maxSkeleton, float position)
        {
            float value = (float)((((maxPixel / maxSkeleton) / 2) * position) + (maxPixel / 2));

            if (value > maxPixel)
            {
                return (float)maxPixel;
            }

            if (value < 0)
            {
                return 0;
            }

            return value;
        }

        #endregion

        #region Drawing

        public static void WriteSkeleton(this Body body, string filePath)
        {
            if (body == null) return;
            
            string headx = body.Joints[JointType.Head].Position.X.ToString();
            string heady = body.Joints[JointType.Head].Position.Y.ToString();
            string headz = body.Joints[JointType.Head].Position.Z.ToString();
            int heads = (int)body.Joints[JointType.Head].TrackingState;

            string neckx = body.Joints[JointType.Neck].Position.X.ToString();
            string necky = body.Joints[JointType.Neck].Position.Y.ToString();
            string neckz = body.Joints[JointType.Neck].Position.Z.ToString();
            int necks = (int)body.Joints[JointType.Neck].TrackingState;

            string spineshoulderx = body.Joints[JointType.SpineShoulder].Position.X.ToString();
            string spineshouldery = body.Joints[JointType.SpineShoulder].Position.Y.ToString();
            string spineshoulderz = body.Joints[JointType.SpineShoulder].Position.Z.ToString();
            int spineshoulders = (int)body.Joints[JointType.SpineShoulder].TrackingState;

            string spinemidx = body.Joints[JointType.SpineMid].Position.X.ToString();
            string spinemidy = body.Joints[JointType.SpineMid].Position.Y.ToString();
            string spinemidz = body.Joints[JointType.SpineMid].Position.Z.ToString();
            int spinemids = (int)body.Joints[JointType.SpineMid].TrackingState;

            string spinebasex = body.Joints[JointType.SpineBase].Position.X.ToString();
            string spinebasey = body.Joints[JointType.SpineBase].Position.Y.ToString();
            string spinebasez = body.Joints[JointType.SpineBase].Position.Z.ToString();
            int spinebases = (int)body.Joints[JointType.SpineBase].TrackingState;

            string shoulderrightx = body.Joints[JointType.ShoulderRight].Position.X.ToString();
            string shoulderrighty = body.Joints[JointType.ShoulderRight].Position.Y.ToString();
            string shoulderrightz = body.Joints[JointType.ShoulderRight].Position.Z.ToString();
            string shoulderleftx = body.Joints[JointType.ShoulderLeft].Position.X.ToString();
            string shoulderlefty = body.Joints[JointType.ShoulderLeft].Position.Y.ToString();
            string shoulderleftz = body.Joints[JointType.ShoulderLeft].Position.Z.ToString();
            int shoulderrights = (int)body.Joints[JointType.ShoulderRight].TrackingState;
            int shoulderlefts = (int)body.Joints[JointType.ShoulderLeft].TrackingState;

            string elbowrightx = body.Joints[JointType.ElbowRight].Position.X.ToString();
            string elbowrighty = body.Joints[JointType.ElbowRight].Position.Y.ToString();
            string elbowrightz = body.Joints[JointType.ElbowRight].Position.Z.ToString();
             string elbowleftx = body.Joints[JointType.ElbowLeft].Position.X.ToString();
             string elbowlefty = body.Joints[JointType.ElbowLeft].Position.Y.ToString();
             string elbowleftz = body.Joints[JointType.ElbowLeft].Position.Z.ToString();
            int elbowrights = (int)body.Joints[JointType.ElbowRight].TrackingState;
             int elbowlefts = (int)body.Joints[JointType.ElbowLeft].TrackingState;
           
             string wristrightx = body.Joints[JointType.WristRight].Position.X.ToString();
             string wristrighty = body.Joints[JointType.WristRight].Position.Y.ToString();
             string wristrightz = body.Joints[JointType.WristRight].Position.Z.ToString();
             string wristleftx = body.Joints[JointType.WristLeft].Position.X.ToString();
             string wristlefty = body.Joints[JointType.WristLeft].Position.Y.ToString();
             string wristleftz = body.Joints[JointType.WristLeft].Position.Z.ToString();
            int wristrights = (int)body.Joints[JointType.WristRight].TrackingState;
             int wristlefts = (int)body.Joints[JointType.WristLeft].TrackingState;

            string handrightx = body.Joints[JointType.HandRight].Position.X.ToString();
            string handrighty = body.Joints[JointType.HandRight].Position.Y.ToString();
            string handrightz = body.Joints[JointType.HandRight].Position.Z.ToString();
             string handleftx = body.Joints[JointType.HandLeft].Position.X.ToString();
             string handlefty = body.Joints[JointType.HandLeft].Position.Y.ToString();
             string handleftz = body.Joints[JointType.HandLeft].Position.Z.ToString();
            int handrights = (int)body.Joints[JointType.HandRight].TrackingState;
             int handlefts = (int)body.Joints[JointType.HandLeft].TrackingState;
                                                      
            string handtiprightx = body.Joints[JointType.HandTipRight].Position.X.ToString();
            string handtiprighty = body.Joints[JointType.HandTipRight].Position.Y.ToString();
            string handtiprightz = body.Joints[JointType.HandTipRight].Position.Z.ToString();
             string handtipleftx = body.Joints[JointType.HandTipLeft].Position.X.ToString();
             string handtiplefty = body.Joints[JointType.HandTipLeft].Position.Y.ToString();
             string handtipleftz = body.Joints[JointType.HandTipLeft].Position.Z.ToString();
            int handtiprights = (int)body.Joints[JointType.HandTipRight].TrackingState;
             int handtiplefts = (int)body.Joints[JointType.HandTipLeft].TrackingState;

             string position = headx + "," +
                               heady + "," +
                               headz + "," +
                               heads.ToString() + "," +
                               neckx + "," +
                               necky + "," +
                               neckz + "," +
                               necks.ToString() + "," +
                               spineshoulderx + "," +
                               spineshouldery + "," +
                               spineshoulderz + "," +
                               spineshoulders.ToString() + "," +
                               spinemidx + "," +
                               spinemidy + "," +
                               spinemidz + "," +
                               spinemids.ToString() + "," +
                               spinebasex + "," +
                               spinebasey + "," +
                               spinebasez + "," +
                               spinebases.ToString() + "," +
                               shoulderrightx + "," +
                               shoulderrighty + "," +
                               shoulderrightz + "," +
                               shoulderrights.ToString() + "," +
                               shoulderleftx + "," +
                               shoulderlefty + "," +
                               shoulderleftz + "," +
                               shoulderlefts.ToString() + "," +
                               elbowrightx + "," +
                               elbowrighty + "," +
                               elbowrightz + "," +
                               elbowrights.ToString() + "," +
                               elbowleftx + "," +
                               elbowlefty + "," +
                               elbowleftz + "," +
                               elbowlefts.ToString() + "," +
                               wristrightx + "," +
                               wristrighty + "," +
                               wristrightz + "," +
                               wristrights.ToString() + "," +
                               wristleftx + "," +
                               wristlefty + "," +
                               wristleftz + "," +
                               wristlefts.ToString() + "," +
                               handrightx + "," +
                               handrighty + "," +
                               handrightz + "," +
                               handrights.ToString() + "," +
                               handleftx + "," +
                               handlefty + "," +
                               handleftz + "," +
                               handlefts.ToString() + "," +
                               handtiprightx + "," +
                               handtiprighty + "," +
                               handtiprightz + "," +
                               handtiprights.ToString() + "," +
                               handtipleftx + "," +
                               handtiplefty + "," +
                               handtipleftz + "," +
                               handtiprights.ToString();

        }

        public static void DrawSkeleton(this Canvas canvas, Body body)
        {
            if (body == null) return;

            foreach (Joint joint in body.Joints.Values)
            {
                canvas.DrawPoint(joint);
            }

            canvas.DrawLine(body.Joints[JointType.Head], body.Joints[JointType.Neck]);
            canvas.DrawLine(body.Joints[JointType.Neck], body.Joints[JointType.SpineShoulder]);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderLeft]);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.ShoulderRight]);
            canvas.DrawLine(body.Joints[JointType.SpineShoulder], body.Joints[JointType.SpineMid]);
            canvas.DrawLine(body.Joints[JointType.ShoulderLeft], body.Joints[JointType.ElbowLeft]);
            canvas.DrawLine(body.Joints[JointType.ShoulderRight], body.Joints[JointType.ElbowRight]);
            canvas.DrawLine(body.Joints[JointType.ElbowLeft], body.Joints[JointType.WristLeft]);
            canvas.DrawLine(body.Joints[JointType.ElbowRight], body.Joints[JointType.WristRight]);
            canvas.DrawLine(body.Joints[JointType.WristLeft], body.Joints[JointType.HandLeft]);
            canvas.DrawLine(body.Joints[JointType.WristRight], body.Joints[JointType.HandRight]);
            canvas.DrawLine(body.Joints[JointType.HandLeft], body.Joints[JointType.HandTipLeft]);
            canvas.DrawLine(body.Joints[JointType.HandRight], body.Joints[JointType.HandTipRight]);
            canvas.DrawLine(body.Joints[JointType.HandTipLeft], body.Joints[JointType.ThumbLeft]);
            canvas.DrawLine(body.Joints[JointType.HandTipRight], body.Joints[JointType.ThumbRight]);
            canvas.DrawLine(body.Joints[JointType.SpineMid], body.Joints[JointType.SpineBase]);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipLeft]);
            canvas.DrawLine(body.Joints[JointType.SpineBase], body.Joints[JointType.HipRight]);
            canvas.DrawLine(body.Joints[JointType.HipLeft], body.Joints[JointType.KneeLeft]);
            canvas.DrawLine(body.Joints[JointType.HipRight], body.Joints[JointType.KneeRight]);
            canvas.DrawLine(body.Joints[JointType.KneeLeft], body.Joints[JointType.AnkleLeft]);
            canvas.DrawLine(body.Joints[JointType.KneeRight], body.Joints[JointType.AnkleRight]);
            canvas.DrawLine(body.Joints[JointType.AnkleLeft], body.Joints[JointType.FootLeft]);
            canvas.DrawLine(body.Joints[JointType.AnkleRight], body.Joints[JointType.FootRight]);
        }

        public static void DrawPoint(this Canvas canvas, Joint joint)
        {
            if (joint.TrackingState == TrackingState.NotTracked) return;

            joint = joint.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Ellipse ellipse = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Colors.LightBlue)
            };

            Canvas.SetLeft(ellipse, joint.Position.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, joint.Position.Y - ellipse.Height / 2);

            canvas.Children.Add(ellipse);
        }

        public static void DrawLine(this Canvas canvas, Joint first, Joint second)
        {
            if (first.TrackingState == TrackingState.NotTracked || second.TrackingState == TrackingState.NotTracked) return;

            first = first.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);
            second = second.ScaleTo(canvas.ActualWidth, canvas.ActualHeight);

            Line line = new Line
            {
                X1 = first.Position.X,
                Y1 = first.Position.Y,
                X2 = second.Position.X,
                Y2 = second.Position.Y,
                StrokeThickness = 8,
                Stroke = new SolidColorBrush(Colors.LightBlue)
            };

            canvas.Children.Add(line);
        }

        #endregion
    }
}

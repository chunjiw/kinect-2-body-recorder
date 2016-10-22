using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace KinectStreams
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Members

        Mode _mode = Mode.Color;

        KinectSensor _sensor;
        MultiSourceFrameReader _reader;
        IList<Body> _bodies;
        SpikeCutter spikeCutter;

        bool _displayBody = true;
        bool _recordBody = false;
        string filePath = "";
        DateTime timeRecordingStarted;
        ulong currentTrackingID = 0;

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Event handlers

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _sensor.Open();

                _reader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body);
                _reader.MultiSourceFrameArrived += Reader_MultiSourceFrameArrived;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            if (_sensor != null)
            {
                _sensor.Close();
            }
        }

        void Reader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            // Color
            using (var frame = reference.ColorFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Color)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Depth
            using (var frame = reference.DepthFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Depth)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Infrared
            using (var frame = reference.InfraredFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    if (_mode == Mode.Infrared)
                    {
                        camera.Source = frame.ToBitmap();
                    }
                }
            }

            // Body
            using (var frame = reference.BodyFrameReference.AcquireFrame())
            {
                if (frame != null)
                {
                    canvas.Children.Clear();

                    _bodies = new Body[frame.BodyFrameSource.BodyCount];

                    frame.GetAndRefreshBodyData(_bodies);

                    TimeSpan relativeTime = DateTime.Now - timeRecordingStarted;
                    string time = relativeTime.TotalSeconds.ToString();

                    //Body body = _bodies[0];

                    foreach (var body in _bodies)
                    {
                        if (body != null)
                        {
                            if (body.IsTracked)
                            {
                                // Draw skeleton.
                                if (_displayBody)
                                {
                                    canvas.DrawSkeleton(body);
                                }
                                // if trackingId not right, continue to next body
                                if (currentTrackingID == 0)
                                {
                                    currentTrackingID = body.TrackingId;                                    
                                }
                                else if (currentTrackingID != body.TrackingId)
                                {
                                    continue;
                                }
                                // write file
                                if (_recordBody) 
                                {
                                    body.WriteSkeleton(filePath, time);
                                    if (spikeCutter.frameNo == 0)
                                    {
                                        spikeCutter.SetPPreBody(body);
                                    } else
                                    if (spikeCutter.frameNo == 1)
                                    {
                                        spikeCutter.SetPreBody(body);
                                    } else { 
                                        spikeCutter.CutSpike(body);
                                        spikeCutter.WriteSkeleton(filePath, time);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Color;
        }

        private void Depth_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Depth;
        }

        private void Infrared_Click(object sender, RoutedEventArgs e)
        {
            _mode = Mode.Infrared;
        }

        private void Body_Click(object sender, RoutedEventArgs e)
        {
            _displayBody = !_displayBody;
        }

        private void Record_Click(object sender, RoutedEventArgs e)
        {
            _recordBody = !_recordBody;
            if (_recordBody == false)
            {
                recordButton.Content = "Record";
                return;
            }
            recordButton.Content = "Stop Recording";
            currentTrackingID = 0;
            // create a csv file and write file header
            string currPath = System.IO.Directory.GetCurrentDirectory();
            string folder = "recordings";
            string recordPath = System.IO.Path.Combine(currPath, folder);
            if (!Directory.Exists(recordPath))
            {
                Directory.CreateDirectory(recordPath);
            }
            timeRecordingStarted = DateTime.Now;
            string filename = timeRecordingStarted.ToString("yyyy-MM-dd HH-mm-ss");
            filename = filename + ".csv";
            filePath = System.IO.Path.Combine(recordPath, filename);
            string writtentext = "time," + "headX," + 
                                    "headY," +
                                    "headZ," +
                                    "headS," +
                                    "neckX," + 
                                    "neckY," +
                                    "neckZ," +
                                    "neckS," +
                                    "spineShoulderX," + 
                                    "spineShoulderY," +
                                    "spineShoulderZ," +
                                    "spineShoulderS," +
                                    "spineMidX," + 
                                    "spineMidY," +
                                    "spineMidZ," +
                                    "spineMidS," +
                                    "spineBaseX," + 
                                    "spineBaseY," +
                                    "spineBaseZ," +
                                    "spineBaseS," +
                                    "shoulderRightX," + 
                                    "shoulderRightY," +
                                    "shoulderRightZ," +
                                    "shoulderRightS," +
                                    "shoulderLeftX," + 
                                    "shoulderLeftY," +
                                    "shoulderLeftZ," +
                                    "shoulderLeftS," +
                                    "elbowRightX," + 
                                    "elbowRightY," +
                                    "elbowRightZ," +
                                    "elbowRightS," +
                                    "elbowLeftX," + 
                                    "elbowLeftY," +
                                    "elbowLeftZ," +
                                    "elbowLeftS," +
                                    "wristRightX," + 
                                    "wristRightY," +
                                    "wristRightZ," +
                                    "wristRightS," +
                                    "wristLeftX," + 
                                    "wristLeftY," +
                                    "wristLeftZ," +
                                    "wristLeftS," +
                                    "handRightX," + 
                                    "handRightY," +
                                    "handRightZ," +
                                    "handRightS," +
                                    "handLeftX," + 
                                    "handLeftY," +
                                    "handLeftZ," +
                                    "handLeftS," +
                                    "handTipRightX," + 
                                    "handTipRightY," +
                                    "handTipRightZ," +
                                    "handTipRightS," +
                                    "handTipLeftX," + 
                                    "handTipLeftY," +
                                    "handTipLeftZ," +
                                    "handTipLeftS," +
                                    // despiked data
                                    "time2," +
                                    "wristRightDespikedX," +
                                    "wristRightDespikedY," +
                                    "wristRightDespikedZ," +
                                    "wristLeftDespikedX," +
                                    "wristLeftDespikedY," +
                                    "wristLeftDespikedZ," +
                                    "elbowRightDespikedX," +
                                    "elbowRightDespikedY," +
                                    "elbowRightDespikedZ," +
                                    "elbowLeftDespikedX," +
                                    "elbowLeftDespikedY," +
                                    "elbowLeftDespikedZ" ;
            File.WriteAllText(filePath, writtentext);
            spikeCutter = new SpikeCutter();
        }

        #endregion
    }

    public enum Mode
    {
        Color,
        Depth,
        Infrared
    }

    public class SpikeCutter
    {
        private static int numJoints = 4;
        private float[] threshold = { 0.1f, 0.1f, 0.05f, 0.05f };
        private int[] flag = { 0, 0, 0, 0 };
        private JointType[] joints = { JointType.WristRight, JointType.WristLeft, JointType.ElbowRight, JointType.ElbowLeft };
        private float[] cbody = Enumerable.Repeat(0f, numJoints * 3).ToArray();
        private float[] pbody = Enumerable.Repeat(0f, numJoints * 3).ToArray();
        private float[] ppbody = Enumerable.Repeat(0f, numJoints * 3).ToArray();
        public int frameNo = 0;

        public SpikeCutter()
        {
            frameNo = 0;
        }

        public void ResetCutter()
        {
            frameNo = 0;
        }

        public void SetPreBody(Body _pbody)
        {
            for (int i = 0; i < numJoints; i++) 
            {
                pbody[3 * i + 0] = _pbody.Joints[joints[i]].Position.X;
                pbody[3 * i + 1] = _pbody.Joints[joints[i]].Position.Y;
                pbody[3 * i + 2] = _pbody.Joints[joints[i]].Position.Z;
            }
            frameNo++;
        }

        public void SetPPreBody(Body _ppbody)
        {
            for (int i = 0; i < numJoints; i++)
            {
                ppbody[3 * i + 0] = _ppbody.Joints[joints[i]].Position.X;
                ppbody[3 * i + 1] = _ppbody.Joints[joints[i]].Position.Y;
                ppbody[3 * i + 2] = _ppbody.Joints[joints[i]].Position.Z;
            }
            frameNo++;
        }

        private void SetCurrentBody(Body _body)
        {
            for (int i = 0; i < numJoints; i++)
            {
                cbody[3 * i + 0] = _body.Joints[joints[i]].Position.X;
                cbody[3 * i + 1] = _body.Joints[joints[i]].Position.Y;
                cbody[3 * i + 2] = _body.Joints[joints[i]].Position.Z;
            }
            frameNo++;
        }

        public void CutSpike(Body _body)
        {
            SetCurrentBody(_body);
            for (int i = 0; i < numJoints; i++)
            {
                if (Norm(cbody[3 * i + 0] - 2 * pbody[3 * i + 0] + ppbody[3 * i + 0],
                        cbody[3 * i + 1] - 2 * pbody[3 * i + 1] + ppbody[3 * i + 1],
                        cbody[3 * i + 2] - 2 * pbody[3 * i + 2] + ppbody[3 * i + 2]) > threshold[i])
                {
                    if (flag[i] == 1)
                    {
                        pbody[3 * i + 0] = cbody[3 * i + 0];
                        pbody[3 * i + 1] = cbody[3 * i + 1];
                        pbody[3 * i + 2] = cbody[3 * i + 2];
                        flag[i] = 0;
                        continue;
                    }
                    float nd1 = Norm(cbody[3 * i + 0] - pbody[3 * i + 0], cbody[3 * i + 1] - pbody[3 * i + 1], cbody[3 * i + 2] - pbody[3 * i + 2]);
                    float nd2 = Norm(cbody[3 * i + 0] - ppbody[3 * i + 0], cbody[3 * i + 1] - ppbody[3 * i + 1], cbody[3 * i + 2] - ppbody[3 * i + 2]);
                    float c12 = CrossNorm(cbody[3 * i + 0] - pbody[3 * i + 0], cbody[3 * i + 1] - pbody[3 * i + 1], cbody[3 * i + 2] - pbody[3 * i + 2],
                        cbody[3 * i + 0] - ppbody[3 * i + 0], cbody[3 * i + 1] - ppbody[3 * i + 1], cbody[3 * i + 2] - ppbody[3 * i + 2]);
                    if (nd2 < nd1 || nd2*nd2 < 2 * c12)
                    {
                        flag[i] = 2;
                        pbody[3 * i + 0] = (ppbody[3 * i + 0] + cbody[3 * i + 0]) / 2;
                        pbody[3 * i + 1] = (ppbody[3 * i + 1] + cbody[3 * i + 1]) / 2;
                        pbody[3 * i + 2] = (ppbody[3 * i + 2] + cbody[3 * i + 2]) / 2;
                    } else {
                        flag[i] = 1;
                        cbody[3 * i + 0] = 2 * pbody[3 * i + 0] - ppbody[3 * i + 0];
                        cbody[3 * i + 1] = 2 * pbody[3 * i + 1] - ppbody[3 * i + 1];
                        cbody[3 * i + 2] = 2 * pbody[3 * i + 2] - ppbody[3 * i + 2];
                    }

                } else
                {
                    flag[i] = 0;
                }
            }
            pbody.CopyTo(ppbody, 0);
            cbody.CopyTo(pbody, 0);
        }

        public void WriteSkeleton(string filePath, string time)
        {
            string despi = "," + time + ",";
            for (int i = 0; i < numJoints*3; i++)
            {
                if (i < numJoints * 3 - 1)
                    despi = despi + ppbody[i].ToString() + ",";
                else
                    despi = despi + ppbody[i].ToString();
            }
            File.AppendAllText(filePath, despi);
        }

        private float Norm(float x, float y, float z)
        {
            return (float)Math.Sqrt(Math.Pow((double)x, 2) + Math.Pow((double)y, 2) + Math.Pow((double)z, 2) );
        }

        private float CrossNorm(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            float x = y1 * z2 - z1 * y2;
            float y = z1 * x2 - x1 * z2;
            float z = x1 * y2 - y1 * x2;
            return Norm(x, y, z);
        }

    }
}

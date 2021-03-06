﻿using Microsoft.Kinect;
using WpfApplication1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Media3D;

//shared with students

namespace WpfApplication1
{
    class KinectClass
    {
        private KinectSensor m_kinectSensor = null;
        private GameClass m_game = null;
        private MainWindow m_window = null;

        private List<Point> m_calibPoints = new List<Point>(); //2d calibration points
        private List<SkeletonPoint> m_skeletonCalibPoints = new List<SkeletonPoint>(); //3d skeleton points

        private Matrix3D m_groundPlaneTransform; //step 2 transform
        private Emgu.CV.Matrix<double> m_transform; //step 3 transform

        private Skeleton[] m_lastSkeletons;

        public KinectClass(GameClass game, MainWindow window)
        {
            m_game = game;
            m_kinectSensor = KinectSensor.KinectSensors[0];
            m_window = window;

            if (null != m_kinectSensor)
            {
                // Start the sensor!
                try
                {
                    m_kinectSensor.Start();

                    // Turn on the skeleton stream to receive skeleton frames
                    m_kinectSensor.SkeletonStream.Enable();

                    // Add an event handler to be called whenever there is new color frame data
                    m_kinectSensor.SkeletonFrameReady += this.SensorCalibrationSkeletonFrameReady;
                }
                catch (IOException e)
                {
                    m_kinectSensor = null;
                }
            }

            m_calibPoints.Add(new Point(0, 0));
            m_calibPoints.Add(new Point(720, 0));
            m_calibPoints.Add(new Point(720, 720));
            m_calibPoints.Add(new Point(0, 720));
        }

        public bool setSkeletonCalibPoint()
        {
            bool skeletonFound = false;

            if (m_lastSkeletons.Length != 0)
            {
                foreach (Skeleton skel in m_lastSkeletons)
                {
                    if (skel.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        m_skeletonCalibPoints.Add(skel.Position);
                        skeletonFound = true;
                    }
                }

                if(skeletonFound)
                {
                    this.calibrate();
                }
            }

            return skeletonFound;
        }

        private void SensorCalibrationSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    m_lastSkeletons = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(m_lastSkeletons);
                }
            }
        }

        private void SensorGameSkeletonFrameReady(object sensor, SkeletonFrameReadyEventArgs e)
        {
            if(m_game.IsFinished())
            {
                return;
            }
            
            bool foundSkeleton = false;

            using (SkeletonFrame frame = e.OpenSkeletonFrame())
            {
                if (frame != null)
                {
                    Skeleton[] skeletonFrames = new Skeleton[frame.SkeletonArrayLength];
                    frame.CopySkeletonDataTo(skeletonFrames);

                    if (skeletonFrames.Length != 0)
                    {
                        foreach (Skeleton skel in skeletonFrames)
                        {
                            if (skel.TrackingState == SkeletonTrackingState.Tracked)
                            {
                                foundSkeleton = true;

                                Point tResult = kinectToProjectionPoint(skel.Position);
                                m_window.moveEllipse(tResult);

                                Gestures gesture = processGesture(skel.Joints);
                                if(gesture != Gestures.NONE)
                                {
                                    Point gridPosition = determineGridPosition(tResult);
                                    if(gridPosition.X != -1 && gridPosition.Y != -1)
                                    {
                                        m_game.PlaceMove(gesture, gridPosition);
                                    }
                                }
                            }
                        }

                        if(!foundSkeleton)
                        {
                            m_window.hideEllipse();
                        } 
                    }
                }
            }
        }

        private Point determineGridPosition(Point position2D)
        {
            Point gridPosition = new Point();
            double boxSize = m_window.Width / 3;

            gridPosition.X = getSingleAxisGridPosition(position2D.X, boxSize);
            gridPosition.Y = getSingleAxisGridPosition(position2D.Y, boxSize);

            return gridPosition;
        }

        private int getSingleAxisGridPosition(double position, double boxSize)
        {
            if (position < boxSize && position > 0)
            {
                return 0;
            }
            else if (position > boxSize && position < boxSize * 2)
            {
                return 1;
            }
            else if (position > boxSize * 2 && position < boxSize * 3)
            {
                return 2;
            } else
            {
                return -1;
            }
        }

        private Gestures processGesture(JointCollection joints)
        {
            Joint leftHand = joints[JointType.HandLeft];
            Joint leftElbow = joints[JointType.ElbowLeft];
            Joint rightHand = joints[JointType.HandRight];
            Joint rightElbow = joints[JointType.ElbowRight];
            Joint shoulderCenter = joints[JointType.ShoulderCenter];
            Joint shoulderLeft = joints[JointType.ShoulderLeft];
            Joint shoulderRight = joints[JointType.ShoulderRight];

            if(leftHand.Position.X > rightHand.Position.X
                && leftElbow.Position.X < rightElbow.Position.X
                && leftElbow.Position.Y < rightHand.Position.Y
                && leftHand.Position.Y > rightElbow.Position.Y)
            {
                return Gestures.X;
            } else if(leftHand.Position.Y > shoulderCenter.Position.Y
                && rightHand.Position.Y > shoulderCenter.Position.Y
                && leftHand.Position.X < rightHand.Position.X
                && leftElbow.Position.X < shoulderLeft.Position.X
                && rightElbow.Position.X > shoulderRight.Position.X)
            {
                return Gestures.O;
            } else
            {
                return Gestures.NONE;
            }
        }

        public void changeSensorEventFunction()
        {
            m_kinectSensor.SkeletonFrameReady -= this.SensorCalibrationSkeletonFrameReady;
            m_kinectSensor.SkeletonFrameReady += this.SensorGameSkeletonFrameReady;
        }

        private void calibrate()
        {
            if (m_skeletonCalibPoints.Count == m_calibPoints.Count)
            {
                //seketon 3D positions --> 3d positions in depth camera
                Point3D p0 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[0]);
                Point3D p1 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[1]);
                Point3D p2 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[2]);
                Point3D p3 = conertSkeletonPointToDepthPoint(m_skeletonCalibPoints[3]);

                //3d positions depth camera --> positions on a 2D plane
                Vector3D v1 = p1 - p0;
                v1.Normalize();

                Vector3D v2 = p2 - p0;
                v2.Normalize();

                Vector3D planeNormalVec = Vector3D.CrossProduct(v1, v2);
                planeNormalVec.Normalize();

                Vector3D resultingPlaneNormal = new Vector3D(0, 0, 1);
                m_groundPlaneTransform = Util.make_align_axis_matrix(resultingPlaneNormal, planeNormalVec);

                Point3D p0OnPlane = m_groundPlaneTransform.Transform(p0);
                Point3D p1OnPlane = m_groundPlaneTransform.Transform(p1);
                Point3D p2OnPlane = m_groundPlaneTransform.Transform(p2);
                Point3D p3OnPlane = m_groundPlaneTransform.Transform(p3);

                //2d plane positions --> exact 2d square on screen (using perspective transform)
                System.Drawing.PointF[] src = new System.Drawing.PointF[4];
                src[0] = new System.Drawing.PointF((float)p0OnPlane.X, (float)p0OnPlane.Y);
                src[1] = new System.Drawing.PointF((float)p1OnPlane.X, (float)p1OnPlane.Y);
                src[2] = new System.Drawing.PointF((float)p2OnPlane.X, (float)p2OnPlane.Y);
                src[3] = new System.Drawing.PointF((float)p3OnPlane.X, (float)p3OnPlane.Y);

                System.Drawing.PointF[] dest = new System.Drawing.PointF[4];
                dest[0] = new System.Drawing.PointF((float)m_calibPoints[0].X, (float)m_calibPoints[0].Y);
                dest[1] = new System.Drawing.PointF((float)m_calibPoints[1].X, (float)m_calibPoints[1].Y);
                dest[2] = new System.Drawing.PointF((float)m_calibPoints[2].X, (float)m_calibPoints[2].Y);
                dest[3] = new System.Drawing.PointF((float)m_calibPoints[3].X, (float)m_calibPoints[3].Y);

                Emgu.CV.Mat transform = Emgu.CV.CvInvoke.GetPerspectiveTransform(src, dest);
               
                m_transform = new Emgu.CV.Matrix<double>(transform.Rows, transform.Cols, transform.NumberOfChannels);
                transform.CopyTo(m_transform);

                //test to see if resulting perspective transform is correct
                //tResultx should be same as points in m_calibPoints
                Point tResult0 = kinectToProjectionPoint(m_skeletonCalibPoints[0]);
                Point tResult1 = kinectToProjectionPoint(m_skeletonCalibPoints[1]);
                Point tResult2 = kinectToProjectionPoint(m_skeletonCalibPoints[2]);
                Point tResult3 = kinectToProjectionPoint(m_skeletonCalibPoints[3]);
            }
        }

        private Point3D conertSkeletonPointToDepthPoint(SkeletonPoint skeletonPoint)
        {
            DepthImagePoint imgPt = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skeletonPoint, DepthImageFormat.Resolution640x480Fps30);

            return new Point3D(imgPt.X, imgPt.Y, imgPt.Depth);
        }

        private Point kinectToProjectionPoint(SkeletonPoint point)
        {
            DepthImagePoint depthP = m_kinectSensor.CoordinateMapper.MapSkeletonPointToDepthPoint(point, DepthImageFormat.Resolution640x480Fps30);
            Point3D p = new Point3D(depthP.X, depthP.Y, depthP.Depth);

            Point3D pOnGroundPlane = m_groundPlaneTransform.Transform(p);

            System.Drawing.PointF[] testPoint = new System.Drawing.PointF[1];
            testPoint[0] = new System.Drawing.PointF((float)pOnGroundPlane.X, (float)pOnGroundPlane.Y);

            System.Drawing.PointF[] resultPoint = Emgu.CV.CvInvoke.PerspectiveTransform(testPoint, m_transform);

            return new Point(resultPoint[0].X, resultPoint[0].Y);
        }

        /*
        private MainWindow getMainWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() == typeof(MainWindow))
                {
                    return (window as MainWindow);
                }
            }

            return null;
        }
        */
    }
}

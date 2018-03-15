using System;
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
using WpfApplication1;

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectClass m_kinect = null;
        private GameClass m_game = null;

        private int m_clicked = 0;

        public MainWindow()
        {
            InitializeComponent();

            m_game = new GameClass();
            m_kinect = new KinectClass(m_game);

            Line line1 = new Line();
            line1.Stroke = System.Windows.Media.Brushes.Black;
            line1.X1 = 240;
            line1.X2 = 240;
            line1.Y1 = 0;
            line1.Y2 = 720;
            line1.StrokeThickness = 5;

            Line line2 = new Line();
            line2.Stroke = System.Windows.Media.Brushes.Black;
            line2.X1 = 480;
            line2.X2 = 480;
            line2.Y1 = 0;
            line2.Y2 = 720;
            line2.StrokeThickness = 5;

            Line line3 = new Line();
            line3.Stroke = System.Windows.Media.Brushes.Black;
            line3.X1 = 0;
            line3.X2 = 720;
            line3.Y1 = 240;
            line3.Y2 = 240;
            line3.StrokeThickness = 5;

            Line line4 = new Line();
            line4.Stroke = System.Windows.Media.Brushes.Black;
            line4.X1 = 0;
            line4.X2 = 720;
            line4.Y1 = 480;
            line4.Y2 = 480;
            line4.StrokeThickness = 5;

            game.Children.Add(line1);
            game.Children.Add(line2);
            game.Children.Add(line3);
            game.Children.Add(line4);
        }

        private void InitiateCalibration(object sender, RoutedEventArgs e)
        {
            start.Visibility = Visibility.Hidden;
            calibration.Visibility = Visibility.Visible;
        }

        private void StartCalibration(object sender, RoutedEventArgs e)
        {
            bool skeletonFound = m_kinect.setSkeletonCalibPoint();

            if(skeletonFound)
            {
                switch (m_clicked)
                {
                    case 0:
                        topleft.Visibility = Visibility.Hidden;
                        topright.Visibility = Visibility.Visible;
                        break;
                    case 1:
                        topright.Visibility = Visibility.Hidden;
                        bottumright.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        bottumright.Visibility = Visibility.Hidden;
                        bottumleft.Visibility = Visibility.Visible;
                        break;
                }

                if (m_clicked == 3)
                {
                    Calibrate.Visibility = Visibility.Hidden;
                    StartAfterCalibrate.Visibility = Visibility.Visible;
                    bottumleft.Visibility = Visibility.Hidden;
                }

                m_clicked++;
            }
        }

        private void StartButton(object sender, RoutedEventArgs e)
        {
            start.Visibility = Visibility.Hidden;
            calibration.Visibility = Visibility.Hidden;
            game.Visibility = Visibility.Visible;

            m_kinect.changeSensorEventFunction();
        }

        public void moveEllipse(Point point)
        {
            currentPosition.Visibility = Visibility.Visible;
            currentPosition.Margin = new Thickness(point.X - (currentPosition.Width / 2), point.Y - (currentPosition.Height / 2), 0, 0);
        }

        public void hideEllipse()
        {
            currentPosition.Visibility = Visibility.Hidden;
        }
    }
}

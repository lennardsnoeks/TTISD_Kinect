using Microsoft.Samples.Kinect.ControlsBasics;
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

namespace WpfApplication1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CalibrationClass m_calibration;

        private int m_clicked = 0;

        public MainWindow()
        {
            InitializeComponent();

            m_calibration = new CalibrationClass();
        }

        private void StartButton(object sender, RoutedEventArgs e)
        {
            m_calibration.setSkeletonCalibPoint();

            switch(m_clicked)
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

            m_clicked++;
        }
    }
}

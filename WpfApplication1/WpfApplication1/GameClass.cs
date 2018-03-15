using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApplication1;

namespace WpfApplication1
{
    public enum Gestures
    {
        NONE, X, O
    }

    class GameClass
    {
        private MainWindow m_window = null;

        public GameClass(MainWindow window)
        {
            m_window = window;
        }

        public void placeMove(Gestures gesture, Point position)
        {
            if(gesture == Gestures.X)
            {
                m_window.drawX(position);
            } else
            {
                m_window.drawO(position);
            }
        }
    }
}

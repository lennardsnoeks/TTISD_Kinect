﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using WpfApplication1;
using MessageBox = System.Windows.Forms.MessageBox;

namespace WpfApplication1
{
    public enum Gestures
    {
        NONE, X, O
    }

    class GameClass
    {
        private MainWindow m_window = null;
        private const int BOARD_SIZE = 3;

        int[,] matrix = new int[BOARD_SIZE, BOARD_SIZE];

        public GameClass(MainWindow window)
        {
            m_window = window;

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    matrix[i, j] = -1;
                }
            }
        }

        private void Reset()
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    matrix[i, j] = -1;
                }
            }
        }

        private bool Set(Gestures gesture, Point position)
        {
            int x = (int)position.X;
            int y = (int)position.Y;

            if (matrix[x, y] != -1)
            {
                matrix[x, y] = (int)gesture;

                if(gesture == Gestures.X)
                {
                    m_window.drawX(position);
                }
                else
                {
                    m_window.drawO(position);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private Gestures CheckWon(Gestures gesture)
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (matrix[i, 0] == (int)gesture && matrix[i, 1] == (int)gesture && matrix[i, 2] == (int)gesture)
                {
                    return gesture;
                }
            }

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                if (matrix[0, i] == (int)gesture && matrix[1, i] == (int)gesture && matrix[2, i] == (int)gesture)
                {
                    return gesture;
                }
            }

            if (matrix[0, 0] == (int)gesture && matrix[1, 1] == (int)gesture && matrix[2, 2] == (int)gesture)
            {
                return gesture;
            }

            if (matrix[2, 0] == (int)gesture && matrix[1, 1] == (int)gesture && matrix[0, 2] == (int)gesture)
            {
                return gesture;
            }

            return Gestures.NONE;
        }

        private bool CheckLock()
        {
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                for (int j = 0; j < BOARD_SIZE; j++)
                {
                    if (matrix[i, j] == -1)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void PlaceMove(Gestures gesture, Point position)
        {
            if (Set(gesture, position))
            {
                if (CheckWon(gesture) == Gestures.X)
                {
                    // Show message mainwindow X won
                    DialogResult dresult = MessageBox.Show("X won the game! The board will be reset.", "Alert"
                              , MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dresult == DialogResult.OK)
                    {
                        // Do something
                        Reset();
                    }
                }
                else if (CheckWon(gesture) == Gestures.O)
                {
                    // Show message mainwindow O won
                    DialogResult dresult = MessageBox.Show("Y won the game! The board will be reset.", "Alert"
                              , MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                    if (dresult == DialogResult.OK)
                    {
                        // Do something
                        Reset();
                    }
                }
                else
                {
                    if (CheckLock())
                    {
                        // Show message mainwindow lock
                        DialogResult dresult = MessageBox.Show("Nobody wins, a lock is reached! The board will be reset.", "Alert"
                              , MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                        if (dresult == DialogResult.OK)
                        {
                            // Do something
                            Reset();
                        }
                    }
                }
            }
        }
    }
}

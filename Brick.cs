using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// INotifyPropertyChanged
using System.ComponentModel;

// Brushes
using System.Windows.Media;
using System.Drawing;

/* Sam (Jian Xin) Zhou and Sam Ansell
 * 4/1/2019
 * Project 3 
 */

namespace BouncingBall
{
    public class Brick : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double _brickHeight;
        public double BrickHeight
        {
            get { return _brickHeight; }
            set
            {
                _brickHeight = value;
                OnPropertyChanged("BrickHeight");
            }
        }

        private double _brickWidth;
        public double BrickWidth
        {
            get { return _brickWidth; }
            set
            {
                _brickWidth = value;
                OnPropertyChanged("BrickWidth");
            }
        }

        private double _brickCanvasTop;
        public double BrickCanvasTop
        {
            get { return _brickCanvasTop; }
            set
            {
                _brickCanvasTop = value;
                OnPropertyChanged("BrickCanvasTop");
            }
        }

        private double _brickCanvasLeft;
        public double BrickCanvasLeft
        {
            get { return _brickCanvasLeft; }
            set
            {
                _brickCanvasLeft = value;
                OnPropertyChanged("BrickCanvasLeft");
            }
        }

        private System.Windows.Media.Brush _brickFill;
        public System.Windows.Media.Brush BrickFill
        {
            get { return _brickFill; }
            set
            {
                _brickFill = value;
                OnPropertyChanged("BrickFill");
            }
        }

        private System.Windows.Media.Brush _brickStroke;
        public System.Windows.Media.Brush BrickStroke
        {
            get { return _brickStroke; }
            set
            {
                _brickStroke = value;
                OnPropertyChanged("BrickStroke");
            }
        }

        private System.Windows.Visibility _brickVisible;
        public System.Windows.Visibility BrickVisible
        {
            get { return _brickVisible; }
            set
            {
                _brickVisible = value;
                OnPropertyChanged("BrickVisible");
            }
        }

        private Rectangle _brickRectangle;
        public Rectangle BrickRectangle
        {
            get { return _brickRectangle; }
            set
            {
                _brickRectangle = value;
                OnPropertyChanged("BrickRectangle");
            }
        }

        private string _brickName;
        public string BrickName
        {
            get { return _brickName; }
            set
            {
                _brickName = value;
                OnPropertyChanged("BrickName");
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// observable collections
using System.Collections.ObjectModel;

// debug output
using System.Diagnostics;

// timer, sleep
using System.Threading;


using System.Windows.Media.Imaging;
using System.Windows;

// hi res timer
using PrecisionTimers;

// Rectangle
// Must update References manually
using System.Drawing;

// Brushes
//using System.Windows.Media;

// INotifyPropertyChanged
using System.ComponentModel;
using System.Drawing.Printing;
using System.Drawing.Drawing2D;
using System.Windows.Threading;


/* Sam (Jian Xin) Zhou and Sam Ansell
 * 4/1/2019
 * Project 3 
 */

namespace BouncingBall
{
    public partial class Model : INotifyPropertyChanged
    {

        #region
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public ObservableCollection<Brick> BrickCollection;
        private static UInt32 _numBricks = 75;
        private Rectangle[] _brickRectangles = new Rectangle[75];
        private UInt32[] _buttonPresses = new UInt32[_numBricks];
        private static UInt32 _numBalls = 1;
        Random _randomNumber = new Random();
        private TimerQueueTimer.WaitOrTimerDelegate _ballTimerCallbackDelegate;
        private TimerQueueTimer.WaitOrTimerDelegate _paddleTimerCallbackDelegate;
        private TimerQueueTimer _ballHiResTimer;
        private TimerQueueTimer _paddleHiResTimer;
        private double _ballXMove = 0.20;
        private double _ballYMove = 0.20;
        System.Drawing.Rectangle _ballRectangle;
        System.Drawing.Rectangle _paddleRectangle;
        Stopwatch stopWatch;
        bool _netDispatchTimerRunning = false;
        double _brickHeight = 15;
        double _brickWidth = 50;
        bool _movepaddleLeft = false;
        bool _movepaddleRight = false;

        private int _point = 0;
        public int Point
        {
            get { return _point; }
            set {
                _point = value;
                OnPropertyChanged("Point");
            }

        }

        private long _elapsedTimePeriod;
        public long ElapsedTimePeriod
        {
            get { return _elapsedTimePeriod; }
            set
            {
                _elapsedTimePeriod = value;
                OnPropertyChanged("ElapsedTimePeriod");
            }

        }

        private bool _moveBall = false;
        public bool MoveBall
        {
            get { return _moveBall; }
            set { _moveBall = value; }
        }

        private double _windowHeight = 100;
        public double WindowHeight
        {
            get { return _windowHeight; }
            set { _windowHeight = value; }
        }

        private double _windowWidth = 100;
        public double WindowWidth
        {
            get { return _windowWidth; }
            set { _windowWidth = value; }
        }

        private string _gameOver;
        public string GameOver
        {
            get { return _gameOver; }
            set {
                _gameOver = value;
                OnPropertyChanged("GameOver");
            }
        }

        public Model()
        {

            BrickCollection = new ObservableCollection<Brick>();
            for (int i = 0; i < _numBricks; i++)
            {
                BrickCollection.Add(new Brick()
                {
                    BrickHeight = _brickHeight,
                    BrickWidth = _brickWidth,
                    BrickFill = System.Windows.Media.Brushes.Red,
                    BrickStroke = System.Windows.Media.Brushes.Black,
                    BrickVisible = System.Windows.Visibility.Visible,
                    BrickName = i.ToString(),
                });

                BrickCollection[i].BrickCanvasLeft = (i % 15) * _brickWidth;
            }

            for (int j = 0; j < _numBricks; j++)
            {
                if (j >= 0 && j <= 15)
                {
                    BrickCollection[j].BrickCanvasTop = _brickHeight;
                }

                else if (j > 15 && j <= 30)
                {
                    BrickCollection[j].BrickCanvasTop = _brickHeight * 2;
                }

                else if (j > 30 && j <= 45)
                {
                    BrickCollection[j].BrickCanvasTop = _brickHeight * 3;
                }

                else if (j > 45 && j <= 60)
                {
                    BrickCollection[j].BrickCanvasTop = _brickHeight * 4;
                }

                else if (j > 60 && j <= 75)
                {
                    BrickCollection[j].BrickCanvasTop = _brickHeight * 5;
                }
            }

            UpdateRects();

            stopWatch = new Stopwatch();
            stopWatch.Start();
           // StartTimer();


        }

        public void InitModel()
        {


            // this delegate is needed for the multi media timer defined 
            // in the TimerQueueTimer class.
            _ballTimerCallbackDelegate = new TimerQueueTimer.WaitOrTimerDelegate(BallMMTimerCallback);
            _paddleTimerCallbackDelegate = new TimerQueueTimer.WaitOrTimerDelegate(paddleMMTimerCallback);
            //_brickTimerCallbackDelegate = new TimerQueueTimer.WaitOrTimerDelegate(brickMMTimerCallback);

            // create our multi-media timers
            _ballHiResTimer = new TimerQueueTimer();
            try
            {
                // create a Multi Media Hi Res timer.
                _ballHiResTimer.Create(1, 1, _ballTimerCallbackDelegate);
            }
            catch (QueueTimerException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed to create Ball timer. Error from GetLastError = {0}", ex.Error);
            }

            _paddleHiResTimer = new TimerQueueTimer();
            try
            {
                // create a Multi Media Hi Res timer.
                _paddleHiResTimer.Create(2, 2, _paddleTimerCallbackDelegate);
            }
            catch (QueueTimerException ex)
            {
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Failed to create paddle timer. Error from GetLastError = {0}", ex.Error);
            }

            UpdateRects();

            for (int i = 0; i < _numBricks; i++)
            {
                Console.WriteLine(BrickCollection[i].BrickRectangle.Location);

            }

            StartTimer();


        }

        private void UpdateRects()
        {
            _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)BallWidth, (int)BallHeight);
            for (int brick = 0; brick < _numBricks; brick++)
                BrickCollection[brick].BrickRectangle = new System.Drawing.Rectangle((int)BrickCollection[brick].BrickCanvasLeft,
                    (int)BrickCollection[brick].BrickCanvasTop, (int)BrickCollection[brick].BrickWidth, (int)BrickCollection[brick].BrickHeight);
        }


        public void CleanUp()
        {
            _ballHiResTimer.Delete();
            _paddleHiResTimer.Delete();
        }


        public void Reset()
        {
            for (int brick = 0; brick < _numBricks; brick++)
            {
                BrickCollection[brick].BrickVisible = Visibility.Visible;
            }

            Point = 0;
            ElapsedTimePeriod = 0;
            NETDispatchTimerTotalTime = 0;
            GameOver = "";


        }

        public void Pause()
        {
            MoveBall = !MoveBall;
            dotNetDispatchTimer.Start();
        }

        private void CheckTouch()
        {
            for (int brick = 0; brick < _numBricks; brick++)
            {
                if (BrickCollection[brick].BrickVisible != Visibility.Visible) continue;

                InterectSide whichSide = IntersectsAt(BrickCollection[brick].BrickRectangle, _ballRectangle);
                switch (whichSide)
                {
                    case InterectSide.NONE:
                        break;

                    case InterectSide.TOP:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballYMove = -_ballYMove;
                        ballCanvasTop += _randomNumber.Next(2) * _ballYMove;
                        Point += 1;

                        break;

                    case InterectSide.BOTTOM:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballYMove = -_ballYMove;
                        ballCanvasTop += _randomNumber.Next(2) * _ballYMove;
                        Point += 1;


                        break;

                    case InterectSide.LEFT:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballXMove = -_ballXMove;
                        ballCanvasLeft += _randomNumber.Next(1) * _ballXMove;
                        Point += 1;

                        break;

                    case InterectSide.RIGHT:
                        BrickCollection[brick].BrickVisible = Visibility.Hidden;
                        _ballXMove = -_ballXMove;
                        ballCanvasLeft += _randomNumber.Next(2) * _ballXMove;
                        Point += 1;

                        break;
                }
            }
        }

       

        public void SetStartPosition()
        {
                      
            BallHeight = 25;
            BallWidth = 25;
            paddleWidth = 80;
            paddleHeight = 10;

            ballCanvasLeft = _windowWidth/2 - BallWidth/2;
            ballCanvasTop = _windowHeight/4;
           
            _moveBall = false;

            paddleCanvasLeft = _windowWidth / 2 - paddleWidth / 2;
            paddleCanvasTop = _windowHeight - paddleHeight;
            _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);

            UpdateRects();

        }

        public void MoveLeft(bool move)
        {
            _movepaddleLeft = move;
        }

        public void MoveRight(bool move)
        {
            _movepaddleRight = move;
        }


        private void BallMMTimerCallback(IntPtr pWhat, bool success)
        {
            var uiContext = SynchronizationContext.Current;

            if (!_moveBall)
                return;

            if (!_ballHiResTimer.ExecutingCallback())
            {
                Console.WriteLine("Aborting timer callback.");
                return;
            }

           
            ballCanvasLeft += _ballXMove;
            ballCanvasTop += _ballYMove;

            // check to see if ball has it the left or right side of the drawing element
            if ((ballCanvasLeft + BallWidth >= _windowWidth) ||
                (ballCanvasLeft <= 0))
                _ballXMove = -_ballXMove;

            // check to see if ball has it the top of the drawing element
            if ( ballCanvasTop <= 0) 
                _ballYMove = -_ballYMove;

            if (ballCanvasTop + BallWidth >= _windowHeight)
            {
                // we hit bottom. stop moving the ball
                _moveBall = false;
                GameOver = "Game Over";
                dotNetDispatchTimer.Stop();



            }

            // see if we hit the paddle
            _ballRectangle = new System.Drawing.Rectangle((int)ballCanvasLeft, (int)ballCanvasTop, (int)BallWidth, (int)BallHeight);
            if (_ballRectangle.IntersectsWith(_paddleRectangle))
            {
                // hit paddle. reverse direction in Y direction
                _ballYMove = -_ballYMove;

                // move the ball away from the paddle so we don't intersect next time around and
                // get stick in a loop where the ball is bouncing repeatedly on the paddle
                ballCanvasTop += 2*_ballYMove;

                // add move the ball in X some small random value so that ball is not traveling in the same 
                // pattern
                ballCanvasLeft += _randomNumber.Next(5);
            }

            // see if we hit the brick
            CheckTouch();

                // done in callback. OK to delete timer
                _ballHiResTimer.DoneExecutingCallback();
        }

        enum InterectSide { NONE, LEFT, RIGHT, TOP, BOTTOM };

        private bool CheckTouch(Brick brick, Rectangle ball)
        {
            if(IntersectsAt(brick.BrickRectangle, ball) != InterectSide.NONE)
            {
                brick.BrickVisible = Visibility.Hidden;
                Console.WriteLine("TOUCH RECORDED!");
                return true;
            }

            return false;
        }

        private InterectSide IntersectsAt(Rectangle brick, Rectangle ball)
        {
            if (brick.IntersectsWith(ball) == false)
                return InterectSide.NONE;

            Rectangle r = Rectangle.Intersect(brick, ball);

            // did we hit the top of the brick
            if (ball.Top + ball.Height - 1 == r.Top &&
                r.Height == 1)
                return InterectSide.TOP;

            if (ball.Top == r.Top &&
                r.Height == 1)
                return InterectSide.BOTTOM;

            if (ball.Left == r.Left &&
                r.Width == 1)
                return InterectSide.RIGHT;

            if (ball.Left + ball.Width - 1 == r.Left &&
                r.Width == 1)
                return InterectSide.LEFT;

            return InterectSide.NONE;
        }



        private void paddleMMTimerCallback(IntPtr pWhat, bool success)
        {

            // start executing callback. this ensures we are synched correctly
            // if the form is abruptly closed
            // if this function returns false, we should exit the callback immediately
            // this means we did not get the mutex, and the timer is being deleted.
            if (!_paddleHiResTimer.ExecutingCallback())
            {
                Console.WriteLine("Aborting timer callback.");
                return;
            }

            if (_movepaddleLeft && paddleCanvasLeft > 0)
                paddleCanvasLeft -= 2;
            else if (_movepaddleRight && paddleCanvasLeft < _windowWidth - paddleWidth)
                paddleCanvasLeft += 2;
            
            _paddleRectangle = new System.Drawing.Rectangle((int)paddleCanvasLeft, (int)paddleCanvasTop, (int)paddleWidth, (int)paddleHeight);


            // done in callback. OK to delete timer
            _paddleHiResTimer.DoneExecutingCallback();
        }


        public void StartTimer()
        {
            NETDispatchTimerStart(true);
        }


        long NETDispatchTimerTotalTime = 0;
        long NETDispatchTimerPreviousTime;

        DispatcherTimer dotNetDispatchTimer;

        public void NETDispatchTimerStart(bool startStop)
        {
            if (startStop == true && _netDispatchTimerRunning == false)
            {
                // reset counters for timing
                NETDispatchTimerTotalTime = 0;
                NETDispatchTimerPreviousTime = stopWatch.ElapsedMilliseconds;

                // set timer interval from GUI and start timer
                dotNetDispatchTimer = new DispatcherTimer();
                dotNetDispatchTimer.Tick += new EventHandler(NETDispatchTimerOnTick);
                dotNetDispatchTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000);
                dotNetDispatchTimer.Start();

                _netDispatchTimerRunning = true;
            }
            else if (startStop == true && _netDispatchTimerRunning == true)
            {
                dotNetDispatchTimer.Stop();
                _netDispatchTimerRunning = false;
            }

        }

        public void NETDispatchTimerOnTick(object obj, EventArgs ea)
        {
            // add time elapsed since previous callback to our total time
            NETDispatchTimerTotalTime += stopWatch.ElapsedMilliseconds - NETDispatchTimerPreviousTime;

            // resent previous time to current time
            NETDispatchTimerPreviousTime = stopWatch.ElapsedMilliseconds;

            ElapsedTimePeriod = (NETDispatchTimerTotalTime / 1000);


 
            // increment the number of times the callback was called over the time period


            
        }


    }


}

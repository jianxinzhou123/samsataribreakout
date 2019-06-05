using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

/* Sam (Jian Xin) Zhou and Sam Ansell
 * 4/1/2019
 * Project 3 
 */


namespace BouncingBall
{
    

    public partial class MainWindow : Window
    {
        private Model _model;



        public MainWindow()
        {
            InitializeComponent();

            // make it so the user cannot resize the window
            this.ResizeMode = ResizeMode.NoResize;

           
        }

   

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            // create an instance of our Model
            _model = new Model();
            _model.WindowHeight = BallCanvas.RenderSize.Height;
            _model.WindowWidth = BallCanvas.RenderSize.Width;
            this.DataContext = _model;
            _model.InitModel();
            _model.SetStartPosition();

            // create an observable collection. this collection
            // contains the tiles we placed in the ItemsControl
            // the data in the Tile Colleciton will be bound to 
            // each of the UI elements on the display
             MyItemsControl.ItemsSource = _model.BrickCollection;
        }

        private void KeypadDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                _model.MoveLeft(true);
            else if (e.Key == Key.Right)
                _model.MoveRight(true);
            else if (e.Key == Key.S)
            {
                _model.Pause();

            }
            else if (e.Key == Key.R)
            {
                _model.Reset();
                _model.SetStartPosition();

            }
            else if (e.Key == Key.E)
            {
                System.Environment.Exit(1);
            }



        }



        private void KeypadUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                _model.MoveLeft(false);
            else if (e.Key == Key.Right)
                _model.MoveRight(false);
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _model.CleanUp();
        }

    }
}

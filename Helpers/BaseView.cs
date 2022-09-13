using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SNSelfOrder.Helpers
{
    public enum WindowDirection { None, Left, Right, Top, Bottom }

    public partial class BaseView : UserControl
    {
        public WindowDirection Direction { get; set; }

        public BaseView()
        {
            this.Loaded += new RoutedEventHandler(ControlLoaded);
        }

        public void ShowControl()
        {
            int animationLength = 300;

            // start setting up the animation we will use to slide the control into position
            Duration duration = new Duration(TimeSpan.FromMilliseconds(animationLength));
            DoubleAnimation anim = new DoubleAnimation(0, duration);

            // the animations will depend on the direction
            switch (Direction)
            {
                case WindowDirection.Top:
                    this.RenderTransform = new TranslateTransform(0, Application.Current.MainWindow.ActualHeight * -1);
                    this.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
                    //       startupTimer.Start();
                    break;
                case WindowDirection.Bottom:
                    this.RenderTransform = new TranslateTransform(0, Application.Current.MainWindow.ActualHeight);
                    this.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
                    //       startupTimer.Start();
                    break;
                case WindowDirection.Left:
                    this.RenderTransform = new TranslateTransform(Application.Current.MainWindow.ActualWidth * -1, 0);
                    this.RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                    //        startupTimer.Start();
                    break;
                case WindowDirection.Right:
                    this.RenderTransform = new TranslateTransform(Application.Current.MainWindow.ActualWidth, 0);
                    this.RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                    //       startupTimer.Start();
                    break;
            }
        }

        public void HideControl()
        {
            int animationLength = 300;

            // start setting up the animation we will use to slide the control out of position
            Duration duration = new Duration(TimeSpan.FromMilliseconds(animationLength));
            DoubleAnimation anim = null;

            var startupTimer = new System.Windows.Threading.DispatcherTimer();
            startupTimer.Tick += TimerFinished;
            startupTimer.Interval = new TimeSpan(0, 0, 0, 0, animationLength);

            this.RenderTransform = new TranslateTransform(0, 0);
            switch (Direction)
            {
                case WindowDirection.Top:
                    anim = new DoubleAnimation(Application.Current.MainWindow.ActualHeight * -1, duration);
                    this.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
                    break;
                case WindowDirection.Bottom:
                    anim = new DoubleAnimation(Application.Current.MainWindow.ActualHeight, duration);
                    this.RenderTransform.BeginAnimation(TranslateTransform.YProperty, anim);
                    break;
                case WindowDirection.Left:
                    anim = new DoubleAnimation(Application.Current.MainWindow.ActualWidth * -1, duration);
                    this.RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                    break;
                case WindowDirection.Right:
                    anim = new DoubleAnimation(Application.Current.MainWindow.ActualWidth, duration);
                    this.RenderTransform.BeginAnimation(TranslateTransform.XProperty, anim);
                    break;
            }
            startupTimer.Start();

        }

        private void ControlLoaded(object sender, RoutedEventArgs e)
        {
            // event handler is no longer required
            this.Loaded -= ControlLoaded;

            this.ShowControl();
        }

        public static Grid MainGrid;

        private void TimerFinished(object sender, EventArgs e)
        {
            ((System.Windows.Threading.DispatcherTimer)sender).Stop();
            // pop the controls
            //SRWindowsHelper.RemoveFromMainGrid(this);
            MainGrid.Children.Remove(this);

            // remove the associated shadow if any.
            var shadowControl = MainGrid.Children[MainGrid.Children.Count - 1];
            if (shadowControl is Border)
            {
                MainGrid.Children.Remove(shadowControl);
            }
        }
    }
}

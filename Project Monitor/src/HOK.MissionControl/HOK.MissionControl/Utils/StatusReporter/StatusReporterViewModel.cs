using System;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using HOK.MissionControl.Tools.Communicator;

namespace HOK.MissionControl.Utils.StatusReporter
{
    public class StatusReporterViewModel : ViewModelBase
    {
        public RelayCommand<Window> WindowLoaded { get; set; }
        private Storyboard FadeInStoryboard { get; set; }
        private Storyboard FadeOutStoryboard { get; set; }
        private DispatcherTimer Timer { get; set; }
        private Window Win { get; set; }

        public StatusReporterViewModel(Status status, string message)
        {
            Status = status;
            Message = message;

            WindowLoaded = new RelayCommand<Window>(OnWindowLoaded);
        }

        private void OnWindowLoaded(Window obj)
        {
            Win = obj;

            var desktopWorkingArea = SystemParameters.WorkArea;
            obj.Left = desktopWorkingArea.Right - obj.Width;
            obj.Top = desktopWorkingArea.Bottom - obj.Height;

            StartTimer();

            // Create the fade in storyboard
            FadeInStoryboard = new Storyboard();
            var fadeInAnimation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.45)));
            Storyboard.SetTarget(fadeInAnimation, obj);
            Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));
            FadeInStoryboard.Children.Add(fadeInAnimation);

            // Create the fade out storyboard
            FadeOutStoryboard = new Storyboard();
            FadeOutStoryboard.Completed += FadeOutStoryboard_Completed;
            var fadeOutAnimation = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromSeconds(0.45)));
            Storyboard.SetTarget(fadeOutAnimation, obj);
            Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
            FadeOutStoryboard.Children.Add(fadeOutAnimation);

            FadeIn();
        }

        private void StartTimer()
        {
            Timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            Timer.Tick += TimerElapsed;
            Timer.Start();
        }

        public void FadeIn()
        {
            // Begin fade in animation
            Win.Dispatcher.BeginInvoke(new Action(FadeInStoryboard.Begin), DispatcherPriority.Render, null);
        }

        public void FadeOut()
        {
            // Begin fade out animation
            Win.Dispatcher.BeginInvoke(new Action(FadeOutStoryboard.Begin), DispatcherPriority.Render, null);
        }

        /// <summary>
        /// Handler for end of Timer. This triggers the Fade out animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TimerElapsed(object sender, EventArgs e)
        {
            Timer.Stop();
            FadeOut();
        }

        /// <summary>
        /// Handler for Fade Out animation end. We don't want to close the window until animation is complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FadeOutStoryboard_Completed(object sender, EventArgs e)
        {
            Win.Close();
        }

        private Status _status;
        public Status Status {
            get { return _status; }
            set { _status = value; RaisePropertyChanged(() => Status); }
        }

        private string _message;
        public string Message {
            get { return _message; }
            set { _message = value; RaisePropertyChanged(() => Message); }
        }
    }
}

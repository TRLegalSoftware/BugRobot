using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows;

namespace BugRobot.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer timer;
        private ObservableCollection<BugRobot.Lib.BugLog> bLogs;
        bool isGetBugsRunning = false;
        public MainWindow()
        {
            InitializeComponent();
            this.bLogs = new ObservableCollection<BugRobot.Lib.BugLog>();

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";

            //Set a default time interval
            interval.Text = "5";

            bugLogs.ItemsSource = bLogs;

        }

        private void RunBotButton(object sender, RoutedEventArgs e)
        { 
            //Only starts if its not running
            if (state.Text.Equals("Not running"))
            {
                startBot();
            }
            else
            {
                stopBot();
            }
        }

        
        private void startBot()
        {            

            //Change the state to starting
            state.Text = "Starting...";
            stateGetBugsFromTFS.Text = "Waiting...";
            startButton.Content = "Stop bot";

            //If you want to run (inteval) times
            if (!runOnce.IsChecked.Value)
            {
                //Setup the timer and starts it, calling the "runEachTime" function
                timer = new Timer();
                timer.Interval = TimeSpan.FromSeconds(Int32.Parse(interval.Text)).TotalMilliseconds;
                timer.AutoReset = true;
                timer.Elapsed += runEachTime;
                timer.Enabled = true;
                timer.Start();
            }

            state.Text = "Running";

            //Call it the first time
            getBugsFromTFS();

        }

        private void stopBot()
        {
            //Stops the timer and then changes the state text
            timer.Stop();
            timer.Enabled = false;
            state.Text = "Not running";
            stateGetBugsFromTFS.Text = "Stopped";
            startButton.Content = "Run bot";
        }

        private void runEachTime(object sender, EventArgs e)
        {
            //Call the actual get bugs functions
            //This run each time is only used for timing purposes
            if(!isGetBugsRunning) getBugsFromTFS();
        }

        private void getBugsFromTFS()
        {
            this.isGetBugsRunning = true;
            stateGetBugsFromTFS.Text = "Connecting...";

            //Get the robot
            var bugRobot = new BugRobot.Lib.BugRobot(query.Text, username.Text, autoAssign.IsChecked.Value, onlyUnassignedBugs.IsChecked.Value, "");

            stateGetBugsFromTFS.Text = "Checking";

            //Run it
            BugRobot.Lib.BugRobot.Message result;

            try
            {
                result = bugRobot.Run();
                //If there's something
                if (result.Success)
                {
                    stateGetBugsFromTFS.Text = "Success!";
                    new Control.NotificationControl(result.Icon).callNotification("Bug " + result.Ids, result.Title, result.Url);
                    bLogs.Add(new BugRobot.Lib.BugLog() { Hour = DateTime.Now.ToString("HH:mm:ss"), Id = result.Ids, Title = result.Title });
                }
            }
            catch (Exception ex)
            {
                stateGetBugsFromTFS.Text = "Failed.";
                MessageBox.Show("Erro:\n\n" + ex.Message+ "\n\nDetalhes:\n" + ex.InnerException);
            }

            
           this.isGetBugsRunning = false;
           stateGetBugsFromTFS.Text = "Waiting...";
        }
    }
}

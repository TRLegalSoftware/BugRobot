using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BugRobot.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer timer;
        private List<BugRobot.Lib.BugLog> bLogs = new List<BugRobot.Lib.BugLog>();
        public MainWindow()
        {
            InitializeComponent();

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
            startButton.Content = "Stop bot";
            
            //Setup the timer and starts it, calling the "runEachTime" function
            timer = new Timer();
            timer.Interval = TimeSpan.FromSeconds(Int32.Parse(interval.Text)).TotalMilliseconds;
            timer.AutoReset = true;
            timer.Elapsed += runEachTime;
            timer.Enabled = true;
            timer.Start();
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
            startButton.Content = "Run bot";
        }

        private void runEachTime(object sender, EventArgs e)
        {
            //Call the actual get bugs functions
            //This run each time is only used for timing purposes
            getBugsFromTFS();
        }

        private void getBugsFromTFS()
        {
            //Get the robot
            var bugRobot = new BugRobot.Lib.BugRobot(query.Text, username.Text, autoAssign.IsChecked.Value, onlyUnassignedBugs.IsChecked.Value, "");

            //Run it
            BugRobot.Lib.BugRobot.Message result = bugRobot.Run();

            //If there's something
            if (result.Success) { 
                new Control.NotificationControl(result.Icon).callNotification("Bug " + result.Ids, result.Title, result.Url);
                bLogs.Add(new BugRobot.Lib.BugLog() { Hour = DateTime.Now.ToString("HH:mm:ss"),  Id=result.Ids, Title = result.Title});
            }
            
        }
    }
}

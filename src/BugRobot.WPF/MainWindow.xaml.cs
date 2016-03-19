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
        public MainWindow()
        {
            InitializeComponent();

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";

            //Set a default time interval
            interval.Text = "5";

            //Setup the timer
            timer = new Timer();
            timer.AutoReset = true;
            timer.Elapsed += runEachTime;
            
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


            new Control.NotificationControl().callNotification("Testee", "Test");
            //Setup the timer and starts it, calling the "runEachTime" function
            timer.Interval = TimeSpan.FromSeconds(Int32.Parse(interval.Text)).TotalMilliseconds;
            timer.Enabled = true;
            timer.Start();

        }

        private void stopBot()
        {
            state.Text = "Not running";
            startButton.Content = "Run bot";
            timer.Stop();
            timer.Enabled = false;

        }

        private void runEachTime(object sender, EventArgs e)
        {
            new Control.NotificationControl().callNotification("Testee", "Test");
            //getBugsFromTFS();
        }

        private void getBugsFromTFS()
        {
            var bugRobot = new BugRobot.Lib.BugRobot(query.Text, username.Text, autoAssign.IsChecked.Value, onlyUnassignedBugs.IsChecked.Value, "");

            BugRobot.Lib.BugRobot.Message result = bugRobot.Run();
            
        }
    }
}

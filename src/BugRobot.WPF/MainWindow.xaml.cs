using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using TFSRobot;
using static TFSRobot.WorkItemRobot;

namespace BugRobot.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Creates new WorkItemRobots for each function needed
        WorkItemRobot bugRobot, assignToMeRobot;

        //Create the log list
        private static ObservableCollection<WorkItemLog> logList;
        private bool isBugRobotRunning = false;
        private bool isGetWorkItemsRobotRunning = false;
        private CancellationTokenSource runBugsToken, getWorkItemsToken;

        public MainWindow()
        {
            logList = new ObservableCollection<WorkItemLog>();
            InitializeComponent();
            this.DataContext = this;

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";
            //Set a default time interval in seconds
            interval.Text = "60";

            //Start a robot only for getting bugs
            bugRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Set the username if want to autoAssign
                username.Text,
                //Set if want to autoAssign value
                bugRobotAutoAssign.IsChecked.Value,
                //Gives it the notification system
                new Notification(Properties.Resources.BugIcon)
            );

            assignToMeRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Set the username if want to autoAssign
                username.Text,
                //Set if want to autoAssign value
                false,
                //Gives it the notification system
                new Notification(Properties.Resources.TFSIcon)
            );

            //Set the WPF table list to the log list
            bugLogsTable.ItemsSource = logList;
        }

        private void RunBugRobotButton(object sender, RoutedEventArgs e)
        {
            //If bug robot is not running    
            if (!isBugRobotRunning)
            {
                startBot();
                bugRobotStartButton.Content = "Stop Bot";
            }
            else
            {
                stopBot();
                bugRobotStartButton.Content = "Run Bot";
            }
        }

        private void startBot()
        {
            bugRobotState.Text = "Running";
            isBugRobotRunning = true;
            try
            {
                //Start a async task to loop each (interval) seconds
                runBugsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;

                var taskGetBugs = Task.Factory.StartNew(async a =>
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bugRobotStateGetBugs.Text = "Connecting.";
                    }));
                    runBot(bugRobot, runBugsToken, bug =>
                        bug.Type.Name.ToLower() == "bug" &&
                        bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                        bug.Fields["Assigned To"].Value.Equals(string.Empty)
                    );
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        bugRobotStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                    }));
                    await Task.Delay(intervalMs, runBugsToken.Token);
                }, runBugsToken.Token, TaskCreationOptions.LongRunning);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
                bugRobotState.Text = "Failed";
            }
        }

        private void stopBot()
        {
            runBugsToken.Cancel();
            bugRobotState.Text = "Not Running";
            bugRobotStateGetBugs.Text = "Stopped";
            isBugRobotRunning = false;
        }


        private void RunWIBotButton(object sender, RoutedEventArgs e)
        {
            if (!username.Text.Equals(""))
            {
                //If bug robot is not running    
                if (!isGetWorkItemsRobotRunning)
                {
                    startWiBot();
                    assignToMeStartButton.Content = "Stop Bot";
                }
                else
                {
                    stopWiBot();
                    assignToMeStartButton.Content = "Run Bot";
                }
            }
            else
            {
                MessageBox.Show("Username cannot be empty");
            }
        }

        private void startWiBot()
        {
            assignToMeState.Text = "Running";
            assignToMeStateGetBugs.Text = "Starting...";
            isGetWorkItemsRobotRunning = true;
            try
            {
                //Start a async task to loop each (interval) seconds
                getWorkItemsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;
                var user = username.Text;

                var taskGetWorkItems = Task.Factory.StartNew(async a =>
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        assignToMeStateGetBugs.Text = "Connecting.";
                    }));
                    runBot(assignToMeRobot, getWorkItemsToken, bug =>
                        bug.Type.Name.ToLower() == "bug" &&
                        bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                        bug.Fields["Assigned To"].Value.ToString().ToLower().Equals(user.ToLower())
                    );
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        assignToMeStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                    }));
                    await Task.Delay(intervalMs, getWorkItemsToken.Token);
                }, getWorkItemsToken.Token, TaskCreationOptions.LongRunning);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
                assignToMeState.Text = "Failed";
            }
        }

        private void stopWiBot()
        {
            getWorkItemsToken.Cancel();
            assignToMeState.Text = "Not Running";
            assignToMeStateGetBugs.Text = "Stopped";
            isGetWorkItemsRobotRunning = false;
        }

        private static void addToGrid(WorkItemLog workItemLog)
        {
            logList.Add(workItemLog);
        }

        private static void runBot(WorkItemRobot robot, CancellationTokenSource cancelToken, Func<WorkItem, bool> filter)
        {
            try
            {
                //While didn't cancel
                while (cancelToken.IsCancellationRequested == false)
                {
                    //Run the bot and gets its list
                    var buglist = robot.Run(filter);

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        //Foreach item on that list, add it to the table
                        foreach (var bug in buglist)
                        {
                            addToGrid(bug);
                        }
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }
        }
    }
}
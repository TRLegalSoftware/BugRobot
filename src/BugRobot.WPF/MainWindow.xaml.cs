using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Deployment.Application;
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
        private static List<WorkItemLog> oldList;
        private bool isBugRobotRunning = false;
        private bool isGetWorkItemsRobotRunning = false;
        private CancellationTokenSource runBugsToken, getWorkItemsToken;
        private Notification notify { get; set; }

        public MainWindow()
        {
            logList = new ObservableCollection<WorkItemLog>();
            oldList = new List<WorkItemLog>();
            InitializeComponent();

            notify = new Notification(Properties.Resources.BugIcon);

            notify.notificationSystem.DoubleClick += delegate (object sender, EventArgs args)
            {
                maximizeWindow();
            };

            //First instatiate the menu for the right click action
            var contextMenu = new System.Windows.Forms.ContextMenu();

            //Creates a menu item to insert on menu
            var rightClickCloseMenuItem = new System.Windows.Forms.MenuItem();
            rightClickCloseMenuItem.Index = 1;
            rightClickCloseMenuItem.Text = "Close";
            rightClickCloseMenuItem.Click += menuItemClose_Click;

            var maximizeMenuItem = new System.Windows.Forms.MenuItem();
            maximizeMenuItem.Index = 0;
            maximizeMenuItem.Text = "Maximize";
            maximizeMenuItem.Click += menuItemMaximize_Click;

            var updateMenuItem = new System.Windows.Forms.MenuItem();
            updateMenuItem.Index = 2;
            updateMenuItem.Text = "Check for updates";
            updateMenuItem.Click += menuItemUpdate_Click;

            contextMenu.MenuItems.Add(updateMenuItem);
            contextMenu.MenuItems.Add(maximizeMenuItem);
            contextMenu.MenuItems.Add(rightClickCloseMenuItem);

            //Set the menu to the right click action
            notify.notificationSystem.ContextMenu = contextMenu;

        }

        private void RunBugRobotButton(object sender, RoutedEventArgs e)
        {
            if (username.Text.Equals("") && bugRobotAutoAssign.IsChecked.Value)
            {
                MessageBox.Show("Username cannot be empty because you want to Auto Assign to a user");
            }
            else
            {   
                //If bug robot is not running    
                if (!isBugRobotRunning)
                {
                    startBot();
                }
                else
                {
                    stopBot();
                }
            }
        }

        private void startBot()
        {

            bugRobotStartButton.Content = "Stop Bot";
            bugRobotState.Text = "Running";
            isBugRobotRunning = true;
            try
            {
                //Start a async task to loop each (interval) seconds
                runBugsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;
                if (bugRobotRunOnce.IsChecked.Value)
                {
                    runOnceBot(bugRobot, runBugsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.Equals(string.Empty)
                        );
                    stopBot();
                }
                else
                {
                    var taskGetBugs = Task.Factory.StartNew(async a =>
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            bugRobotStateGetBugs.Text = "Connecting.";
                        }));
                        runBot(intervalMs, bugRobot, runBugsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.Equals(string.Empty)
                        );
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            bugRobotStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                        }));
                    }, runBugsToken.Token, TaskCreationOptions.LongRunning);

                }
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

            bugRobotStartButton.Content = "Run Bot";
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
                }
                else
                {
                    stopWiBot();
                }
            }
            else
            {
                MessageBox.Show("Username cannot be empty");
            }
        }

        private void startWiBot()
        {
            assignToMeStartButton.Content = "Stop Bot";
            assignToMeState.Text = "Running";
            assignToMeStateGetBugs.Text = "Starting...";
            isGetWorkItemsRobotRunning = true;
            try
            {
                getWorkItemsToken = new CancellationTokenSource();
                var intervalMs = Convert.ToInt32(interval.Text) * 1000;
                var user = username.Text;

                if (assignToMeRunOnce.IsChecked.Value)
                {
                    runOnceBot(assignToMeRobot, getWorkItemsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.ToString().ToLower().Equals(user.ToLower())
                        );
                    stopWiBot();
                }
                else
                {
                    //Start a async task to loop each (interval) seconds 
                    var taskGetWorkItems = Task.Factory.StartNew(async a =>
                    {
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            assignToMeStateGetBugs.Text = "Connecting.";
                        }));
                        runBot(intervalMs, assignToMeRobot, getWorkItemsToken, bug =>
                            bug.Type.Name.ToLower() == "bug" &&
                            bug.Fields["BugType"].Value.ToString().ToLower() == "desenvolvimento" &&
                            bug.Fields["Assigned To"].Value.ToString().ToLower().Equals(user.ToLower())
                        );
                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            assignToMeStateGetBugs.Text = string.Format("Waiting the next {0} seconds.", interval.Text);
                        }));
                    }, getWorkItemsToken.Token, TaskCreationOptions.LongRunning);
                }
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
            assignToMeStartButton.Content = "Run Bot";
            isGetWorkItemsRobotRunning = false;
        }

        private static void addToGrid(WorkItemLog workItemLog)
        {
            //Just add to list if this bug wasn't a recent bug and wasn't cleared before
            if (!logList.Any(l => l.Id == workItemLog.Id) && !oldList.Any(l => l.Id == workItemLog.Id))
            {
                logList.Add(workItemLog);
            }
        }

        private static async void runBot(int interval, WorkItemRobot robot, CancellationTokenSource cancelToken, Func<WorkItem, bool> filter)
        {
            try
            {
                //While didn't cancel
                while (cancelToken.IsCancellationRequested == false)
                {
                    //Run the bot and gets its list
                    var buglist = robot.Run(filter);

                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                     {
                         //Foreach item on that list, add it to the table
                         foreach (var bug in buglist)
                         {
                             addToGrid(bug);
                         }
                     }));
                    await Task.Delay(interval, cancelToken.Token);
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals("A task was canceled."))
                    MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }
        }

        private static void runOnceBot(WorkItemRobot robot, CancellationTokenSource cancelToken, Func<WorkItem, bool> filter)
        {
            try
            {
                //Run the bot and gets its list
                var buglist = robot.Run(filter);


                //Foreach item on that list, add it to the table
                foreach (var bug in buglist)
                {
                    addToGrid(bug);
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Equals("A task was canceled."))
                    MessageBox.Show("Erro:\n\n" + ex.Message + "\n\nDetalhes:\n" + ex.InnerException);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //If you're debugging, there's not a current version to get and it throws a exception
            try
            {
                versionBox.Content = string.Format("Version: {0}", ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString());
            }
            catch (Exception ex)
            {
                versionBox.Content = "Version: Debug";
            }
            this.DataContext = this;

            //Set the query text to the default one
            query.Text = "http://plk-tfs2013/tfs/Fabrica_Collection/Fabrica/_workitems#path=Shared+Queries%2FSustenta%C3%A7%C3%A3o+LegalOne%2FRobo+-+Sustenta%C3%A7%C3%A3o+-+Bugs+de+clientes&_a=query&fullScreen=false";
            //Set a default time interval in seconds
            interval.Text = "60";

            //Start a robot only for getting bugs
            bugRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Context string
                "em aberto",
                //Gives it the notification system
                notify,
                //Set the bot name
                "Bug",
                //Set the username if want to autoAssign
                username.Text,
                //Set if want to autoAssign value
                bugRobotAutoAssign.IsChecked.Value
            );

            //Start a robot only for getting bugs
            assignToMeRobot = new WorkItemRobot(
                //Set the query to find its WorkItems
                query.Text,
                //Context string
                "adicionado(s) ao meu nome",
                //Gives it the notification system
                notify,
                //Set the bot name
                "Add"
            );

            //Set the WPF table list to the log list
            bugLogsTable.ItemsSource = logList;

            //This is called to check for updates every 2 hours
            Task.Factory.StartNew(new Action(async () =>
            {
                while (true)
                {
                    //Check for updates
                    InstallUpdateSyncWithInfo();
                    //Check for updates each 2 hours
                    await Task.Delay((60000 * (60 * 2)));
                }
            }), TaskCreationOptions.LongRunning);
        }

        //If the state of the ENTIRE window has changed
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        //Menu "CLOSE" click
        private void menuItemClose_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            Application.Current.Shutdown();
        }

        //Handle the double click on a listItem
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                try
                {
                    var clickedItem = ((TFSRobot.WorkItemLog)item.Content).Url;
                    if (clickedItem != null)
                        System.Diagnostics.Process.Start(clickedItem);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    if (ex.ErrorCode == -2147467259)
                        MessageBox.Show(ex.Message);
                }
                catch (System.Exception other)
                {
                    MessageBox.Show(other.Message);
                }
            }
        }

        private void clean_Click(object sender, RoutedEventArgs e)
        {
            oldList.AddRange(logList);
            logList.Clear();
        }

        //Menu "Maximize" click
        private void menuItemMaximize_Click(object Sender, EventArgs e)
        {
            // Close the form, which closes the application.
            maximizeWindow();
        }

        //Menu "CLOSE" click
        private void menuItemUpdate_Click(object Sender, EventArgs e)
        {
            // Call the uptade function
            InstallUpdateSyncWithInfo();
        }

        //Maximize window function
        private void maximizeWindow()
        {
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        //This checks for updates
        private void InstallUpdateSyncWithInfo()
        {
            UpdateCheckInfo info = null;

            if (ApplicationDeployment.IsNetworkDeployed)
            {
                ApplicationDeployment ad = ApplicationDeployment.CurrentDeployment;

                try
                {
                    info = ad.CheckForDetailedUpdate();

                }
                catch (DeploymentDownloadException dde)
                {
                    MessageBox.Show("The new version of the application cannot be downloaded at this time. \n\nPlease check your network connection, or try again later. Error: " + dde.Message);
                    return;
                }
                catch (InvalidDeploymentException ide)
                {
                    MessageBox.Show("Cannot check for a new version of the application. The ClickOnce deployment is corrupt. Please redeploy the application and try again. Error: " + ide.Message);
                    return;
                }
                catch (InvalidOperationException ioe)
                {
                    MessageBox.Show("This application cannot be updated. It is likely not a ClickOnce application. Error: " + ioe.Message);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Unknown error: " + ex.Message);
                    return;
                }

                if (info.UpdateAvailable)
                {
                    Boolean doUpdate = true;

                    if (!info.IsUpdateRequired)
                    {
                        var messageResult = MessageBox.Show("An update is available. Would you like to update the application now?", "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                        if (messageResult.Equals(MessageBoxResult.No))
                        {
                            doUpdate = false;
                        }
                    }
                    else
                    {
                        // Display a message that the app MUST reboot. Display the minimum required version.
                        MessageBox.Show("This application has detected a mandatory update from your current " +
                            "version to version " + info.MinimumRequiredVersion.ToString() +
                            ". The application will now install the update and restart.",
                            "Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    if (doUpdate)
                    {
                        try
                        {
                            ad.Update();
                            MessageBox.Show("The application has been upgraded, and will now restart.");

                            System.Windows.Forms.Application.Restart();
                            Application.Current.Shutdown();
                        }
                        catch (DeploymentDownloadException dde)
                        {
                            MessageBox.Show("Cannot install the latest version of the application. \n\nPlease check your network connection, or try again later. Error: " + dde);
                            return;
                        }
                    }
                }
            }
        }



    }
}
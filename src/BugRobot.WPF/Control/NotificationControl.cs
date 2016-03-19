using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BugRobot.WPF.Control
{
    public class NotificationControl
    {
        private NotifyIcon notificationSystem;

        //Creates a url out of the scope to use between methods
        private String url;
        public NotificationControl()
        {
            this.notificationSystem = new NotifyIcon();
            this.notificationSystem.Icon = System.Drawing.SystemIcons.Warning;
            this.notificationSystem.Visible = true;
        }

        public NotificationControl(string img) : this()
        {
            //Tests if there is really a image path
            //since this class is allways called
            //Passing an icon
            if(img.Length>3){
                var bitmap = new Bitmap(img);
                var iconHandle = bitmap.GetHicon();
                var icon = System.Drawing.Icon.FromHandle(iconHandle);
                this.notificationSystem.Icon = icon;
            }
        }

        private void setupNotification(string title, string text, string url)
        {
            this.notificationSystem.BalloonTipTitle = title;
            this.notificationSystem.BalloonTipText = text;

            this.url = url;
            if(url.Length>3)
            this.notificationSystem.BalloonTipClicked += new EventHandler(redirectToLink);              
        }

        private void redirectToLink(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.url);
            }
            catch(System.ComponentModel.Win32Exception ex)
            {
                if (ex.ErrorCode == -2147467259)
                    MessageBox.Show(ex.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

    
        public void callNotification(string title, string text, string url) {
            this.setupNotification(title, text, url);
            this.notificationSystem.ShowBalloonTip(0);
        }

        public void callNotification(string title, string text, string url, int timeout) {
            this.setupNotification(title, text, url);
            this.notificationSystem.ShowBalloonTip(timeout);
        }
    }
}

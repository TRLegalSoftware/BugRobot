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

            //Click on baloon to redirect(work in progress)
            //this.notificationSystem.BalloonTipClicked += new System.EventHandler(redirectToLink(url));                
        }

        private EventHandler redirectToLink(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
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

            return null;
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

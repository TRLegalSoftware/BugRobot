using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFSRobot
{
    public class NotificationContent
    {   
        //Notification's title
        public string Title { get; set; }

        //Link to the notification's content
        public string Url { get; set; }

        //The content of notification
        public string Content { get; set; }
    }
}

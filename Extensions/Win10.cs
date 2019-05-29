using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Extensions
{
    public class LiveTiles
    {
        public LiveTiles()
        { }

        public static void UpdateBadge(string Value)
        {
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", Value);
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
        }

        public static void LiveTileUpdater(string LiveTileXML)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(LiveTileXML);
            TileNotification tileNotification = new TileNotification(xmlDoc);
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
        }
    }

    public class Notifications
    {
        public Notifications()
        {

        }

        public static void SendNotification(string[] Text, ToastTemplateType Template, string ImageSource = null)
        {
            ToastTemplateType ToastTemplate = Template;
            XmlDocument ToastXml = ToastNotificationManager.GetTemplateContent(ToastTemplate);
            XmlNodeList ToastElements;

     /*       if (ImageSource != null)
            {
                ToastElements = ToastXml.GetElementsByTagName("image");

                try
                {
                    ToastElements[0].AppendChild(ToastXml.CreateTextNode(ImageSource));
                }
                catch { }
            }*/

            ToastElements = ToastXml.GetElementsByTagName("text");

            for (int i = 0; i < ToastElements.Length; i++)
            {
                ToastElements[i].AppendChild(ToastXml.CreateTextNode(Text[i]));
            }

            ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(ToastXml));
        }
    }
}

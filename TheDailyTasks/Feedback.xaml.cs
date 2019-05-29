using System;
using Windows.ApplicationModel.Email;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TheDailyTasks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Feedback : Page
    {
        public Feedback()
        {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += Add_BackRequested;
        }

        private async void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                //Windows.ApplicationModel.Email.EmailManager pok = new Windows.ApplicationModel.Email.EmailManager();
           /*     EmailMessage pok = new EmailMessage();
                pok.Body = Body.Text;
                pok.Subject = "The Daily Tasks - Feedback";
                EmailRecipient po = new EmailRecipient(Mail.Text);
                pok.Sender = po;
                await EmailManager.ShowComposeNewEmailAsync(pok); */

              /*  System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
                message.To.Add("frantanov8k@gmail.com");
                message.Subject = "Zlepšení programu DVData";
                message.From = new System.Net.Mail.MailAddress("neznamej@je.tu");
                message.Body = "E_mail: " + Upgrade_Mail.Text +
                    "\n\nNávrh na zlepšení programu: " + new TextRange(Upgrade_Message.Document.ContentStart, Upgrade_Message.Document.ContentEnd).Text;


                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Timeout = 10000;
                smtp.Credentials = new System.Net.NetworkCredential("frantanov8k@gmail.com", "asdfghjkloiuytrewq");
                smtp.Send(message);

                MessageBox.Show("Zpráva odeslána. \nDěkujeme za Váš názor", "Děkujeme", MessageBoxButton.OK, MessageBoxImage.Information);
                stav_prog("Děkujeme za vaše nápady");
                Upgrade_Mail.Text = "";
                message.Dispose();*/
            }
            catch (Exception ex)
            {
              //  MessageBox.Show(ex.ToString() + "\n\nAkci zopakujte za pár minut.", "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
                //stav_prog("Akce se nezdařila");
            }
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack == true)
            {
                Frame.GoBack();
            }
        }

        private void Add_BackRequested(object sender, BackRequestedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= Add_BackRequested;
            Frame.GoBack();
        }
    }
}

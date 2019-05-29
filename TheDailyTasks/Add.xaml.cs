using System;
using System.Collections.ObjectModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TheDailyTasks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Add : Page
    {
        Tasks Task;

        public Add()
        {
            this.InitializeComponent();

            //Frame pok = Window.Current.Content as Frame;
            //if (pok.ActualWidth >= 720)
            //    pok = App.DataModel.DesiredFrame;
        }
        
        public Add(Tasks _Task)
        {
            this.InitializeComponent();

            //Frame pok = Window.Current.Content as Frame;
            //if (pok.ActualWidth >= 720)
            //    pok = App.DataModel.DesiredFrame;

            //pok.Navigate(typeof(Add), App.DataModel.ExaminedTask);


            App.DataModel.DesiredFrame.Navigate(typeof(Add), App.DataModel.ExaminedTask);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.DataModel.IsAdditionalPanelDisplayed)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += Add_BackRequested;
            }

            StartDate.Date = DateTime.Today;
            EndDate.Date = DateTime.Today;

            Task = App.DataModel.ExaminedTask;

            if (Task != null)
            {
                Title.Text = "Edit Activity";
                Description.Text = Task.Description;
                StartDate.Date = Task.Start;
                EndDate.Date = Task.End;
                Title.Tag = "Edit";
                Neverending.IsChecked = Task.Neverend;
                //Confirm.Content = "Edit";
                Notify.IsChecked = Task.Notify;
                notifyTime.Time = new TimeSpan(Task.WhenNotify.Hour, Task.WhenNotify.Minute, Task.WhenNotify.Second);
                ActivityName.Text = Task.Name;
            }
            else
            {
                AllDays.IsChecked = true;
            }

            App.DataModel.AdditionalPanelStyle = Title.Tag.ToString();
        }

        private void Add_BackRequested(object sender, BackRequestedEventArgs e)
        {
            CancelBtn_Click(null, null);
        }

        private void ConfirmBtn_Click(object sender, RoutedEventArgs e)
        {
            Tasks NewTask = new Tasks();
            if (Task != null)
            {
               // App.DataModel.Delete(Task);

                NewTask.ID = Task.ID;
                NewTask.Dates = Task.Dates;

                /*App.DataModel.AddTask(ActivityName.Text, Description.Text, new DateTime(StartDate.Date.Value.Year, 
                    StartDate.Date.Value.Month, StartDate.Date.Value.Day), new DateTime(EndDate.Date.Value.Year, 
                    EndDate.Date.Value.Month, EndDate.Date.Value.Day), (bool)Neverending.IsChecked, (bool)Notify.IsChecked, 
                    (new DateTime(2015, 1, 1)) + notifyTime.Time, Task.Dates);*/
            }
            else
            {
                NewTask.ID = -1;
            }

            NewTask.Name = ActivityName.Text;
            NewTask.Description = Description.Text;
            NewTask.Start = new DateTime(StartDate.Date.Value.Year, StartDate.Date.Value.Month, StartDate.Date.Value.Day);
            NewTask.End = new DateTime(EndDate.Date.Value.Year, EndDate.Date.Value.Month, EndDate.Date.Value.Day);
            NewTask.Neverend = (bool)Neverending.IsChecked;
            NewTask.Notify = (bool)Notify.IsChecked;
            NewTask.WhenNotify = (new DateTime(2015, 1, 1)) + notifyTime.Time;
            NewTask.NotifyDays = GetDays();

            App.DataModel.AddTask(NewTask);

            Add_BackRequested(sender, null);
        }

        private ObservableCollection<DayOfWeek> GetDays()
        {
            var Days = new ObservableCollection<DayOfWeek>();

            if (AllDays.IsChecked == true)
            {
                Days.Add(DayOfWeek.Monday);
                Days.Add(DayOfWeek.Tuesday);
                Days.Add(DayOfWeek.Wednesday);
                Days.Add(DayOfWeek.Thursday);
                Days.Add(DayOfWeek.Friday);
                Days.Add(DayOfWeek.Saturday);
                Days.Add(DayOfWeek.Sunday);
                return Days;
            }

            if (Monday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Monday);
            }

            if (Tuesday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Tuesday);
            }

            if (Wednesday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Wednesday);
            }

            if (Thursday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Thursday);
            }

            if (Friday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Friday);
            }

            if (Saturday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Saturday);
            }

            if (Sunday.IsChecked == true)
            {
                Days.Add(DayOfWeek.Sunday);
            }

            return Days;
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= Add_BackRequested;
            App.DataModel.IsAdditionalPanelDisplayed = false;
            App.DataModel.AdditionalPanelContent = "";

            if (App.DataModel.FrameMain.ActualWidth < 720)
                Frame?.Navigate(typeof(MainPage));

            /*  Add_BackRequested(sender, null);*/

            //XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

            //XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            //for (int i = 0; i < stringElements.Length; i++)
            //{
            //    stringElements[i].AppendChild(toastXml.CreateTextNode("Line " + i));
            //}

            //// Specify the absolute path to an image
            //XmlNodeList imageElements = toastXml.GetElementsByTagName("image");


            //var toast = new ToastNotification(toastXml);
            //ToastNotificationManager.CreateToastNotifier().Show(toast);

        }

        private void StartCheckDate(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if ((StartDate.Date < DateTime.Today) && (Task == null))
                StartDate.Date = DateTime.Today;

            EndCheckDate(null, null);
        }

        private void EndCheckDate(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if ((EndDate.Date < StartDate.Date) && (Task == null))
                EndDate.Date = StartDate.Date;
        }

        private void Add_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            App.DataModel.WindowWidth = e.NewSize.Width;
            if (e.NewSize.Width >= 720 && Frame == Window.Current.Content as Frame)
            {
                Frame?.Navigate(typeof(MainPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= Add_BackRequested;
            }
            else if (Frame == Window.Current.Content as Frame)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += Add_BackRequested;
            }
        }

        private void DaysChecked(object sender, RoutedEventArgs e)
        {
            CheckBox Check = sender as CheckBox;

            if (Check.Name == "AllDays" && AllDays.IsChecked == true)
            {
                Monday.IsChecked = false;
                Tuesday.IsChecked = false;
                Wednesday.IsChecked = false;
                Thursday.IsChecked = false;
                Friday.IsChecked = false;
                Saturday.IsChecked = false;
                Sunday.IsChecked = false;
            }
            else
            {
                AllDays.IsChecked = false;
            }

            if (Monday.IsChecked == false && Tuesday.IsChecked == false && Wednesday.IsChecked == false && Thursday.IsChecked == false && Friday.IsChecked == false && Saturday.IsChecked == false && Sunday.IsChecked == false)
            {
                AllDays.Unchecked -= DaysChecked;
                AllDays.Checked -= DaysChecked;
                AllDays.IsChecked = true;
                AllDays.Unchecked += DaysChecked;
                AllDays.Checked += DaysChecked;
            }

        }

        private void CheckButtonLoaded(object sender, RoutedEventArgs e)
        {
            if (Task != null)
                SetDays();
            else
                AllDays.IsChecked = true;
        }

        private void SetDays()
        {
            if (Task.NotifyDays.Count == 7)
            {
                AllDays.IsChecked = true;
                return;
            }

            if (Task.NotifyDays.Contains(DayOfWeek.Monday))
                Monday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Tuesday))
                Tuesday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Wednesday))
                Wednesday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Thursday))
                Thursday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Friday))
                Friday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Saturday))
                Saturday.IsChecked = true;

            if (Task.NotifyDays.Contains(DayOfWeek.Sunday))
                Sunday.IsChecked = true;
        }

        private void ActivityName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (App.DataModel.GetListOfTasks().Contains(ActivityName.Text))
                ConfirmBtn.IsEnabled = false;
            else
                ConfirmBtn.IsEnabled = true;
        }
    }
}

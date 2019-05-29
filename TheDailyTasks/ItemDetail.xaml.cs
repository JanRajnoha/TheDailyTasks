using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using TaskManager;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TheDailyTasks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ItemDetail : Page, INotifyPropertyChanged
    {
        public ItemDetail()
        {
            this.InitializeComponent();
        }

        public ItemDetail(bool Navigate, Tasks Task)
        {
            this.InitializeComponent();

            //Frame pok = Window.Current.Content as Frame;
            //if (pok.ActualWidth >= 720)
            //    pok = App.DataModel.DesiredFrame;        
                
            App.DataModel.DesiredFrame.Navigate(typeof (ItemDetail), Task);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double NewCalendarWidth
        {
            get { return CalendarWidth; }
            set
            {
                CalendarWidth = value - 20;
                OnPropertyChanged();
            }
        }

        private double CalendarWidth;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.DataModel.IsAdditionalPanelDisplayed)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                     AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += ItemDetail_BackRequested;
            }

            var CompleteDay = new CompleteDays();
            var Task = App.DataModel.ExaminedTask;
            CompleteDay.CompleteDaysInit(Task.Start, Task.End, Task?.Dates, Task.Neverend, Task.NotifyDays);
            Resources["CompleteDiagram"] = CompleteDay;
            Calendar.CellTemplateSelector = CompleteDay;

            ActivityName.Text = Task.Name;
            StartDay.Text = Task.Start.Date.ToString("d");
            EndDay.Text = Task.End.Date.ToString("d");
            Description.Text = Task.Description;
            
            Functions fce = new Functions();

            if (Task.Start <= DateTime.Today)
            {
                var DatesToToday = DateTime.Today - Task.Start.AddDays(-1);
                var Days = fce.GetNumberOfSpecificDays(DatesToToday, Task.NotifyDays, Task.Start);
                int CompleteDaysToToday = Task.Dates.Select(x => x.Date <= DateTime.Today).Count();

                if (DateTime.Today > Task.End)
                {
                    CompleteDays.Text = "Activity has been completed";
                }
                else
                {
                    CompleteDays.Text = ((int)((double)CompleteDaysToToday / Days * 100)).ToString() + "%";
                }
            }
            
            if (!Task.Neverend)
            {
                var Dates = Task.End - Task.Start.AddDays(-1);
                int Days = fce.GetNumberOfSpecificDays(Dates, Task.NotifyDays, Task.Start, Task.End);
                int CompleteDates = Task.Dates.Count();

                CompleteDaysSummary.Text = ((int)((double)CompleteDates / Days * 100)).ToString() + "% (" + CompleteDates + " of " + Days + ")";

                if ((DateTime.Today > Task.End) || (DateTime.Today >= Task.End && Task.Dates.Contains(DateTime.Today)))
                {
                    Status.Text = "Complete";
                }
                else
                {
                    Status.Text = "Uncomplete";
                }
            }
            else
            {
                CompleteDaysSummary.Text = "Neverending";
                Status.Text = "Neverending";
            }
        }

        private void ItemDetail_BackRequested(object sender, BackRequestedEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().BackRequested -= ItemDetail_BackRequested;
            App.DataModel.IsAdditionalPanelDisplayed = false;
            App.DataModel.AdditionalPanelContent = "";

            if (App.DataModel.FrameMain.ActualWidth < 720)
                Frame?.Navigate(typeof(MainPage));

 //           if (Frame?.CanGoBack ?? false)
   //         {
     //           Frame.GoBack();// Navigate(typeof(MainPage));
       //         App.DataModel.IsAdditionalPanelDisplayed = false;
         //   }
        }

        private void ItemDetail_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            App.DataModel.WindowWidth = e.NewSize.Width;
            if (e.NewSize.Width >= 720 && Frame == Window.Current.Content as Frame)
            {
                Frame.Navigate(typeof(MainPage));
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                SystemNavigationManager.GetForCurrentView().BackRequested -= ItemDetail_BackRequested;
            }
            else if (Frame == Window.Current.Content as Frame)
            {
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                SystemNavigationManager.GetForCurrentView().BackRequested += ItemDetail_BackRequested;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ItemDetail_BackRequested(null, null);
        }
    }

    public class CompleteDays : DataTemplateSelector
    {
        private ResourceDictionary dictionary;

        private DateTime Start { get; set; }
        private DateTime End { get; set; }
        private ObservableCollection<DateTime> Days { get; set; }
        private bool Neverend { get; set; }
        private ObservableCollection<DayOfWeek> AcceptedDays { get; set; }

        public void CompleteDaysInit(DateTimeOffset _Start, DateTimeOffset _End, ObservableCollection<DateTime> _Days, bool _Neverend, ObservableCollection<DayOfWeek> _AcceptedDays)
        {
            Start = new DateTime(_Start.Year, _Start.Month, _Start.Day);
            End = new DateTime(_End.Year, _End.Month, _End.Day);
            Days = _Days;
            Neverend = _Neverend;
            AcceptedDays = _AcceptedDays;
        }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (dictionary == null)
            {
                dictionary = new ResourceDictionary();
                dictionary.Source = new Uri("ms-appx:///TemplateCompleteDays.xaml", UriKind.RelativeOrAbsolute);
            }

            var Date = (DateTime)item;

            if (((Date < Start) || ((Date > End) && !Neverend) || !AcceptedDays.Contains(Date.DayOfWeek)))
            {
                return dictionary["Default"] as DataTemplate;
            }
            else
            {
                if (Days.Contains(Date))
                {
                    return dictionary["Complete"] as DataTemplate;
                }
                else
                {
                    return dictionary["Uncomplete"] as DataTemplate;
                }
            }
        }
    }
}

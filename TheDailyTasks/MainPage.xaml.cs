using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.UI.Core;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Microsoft.Advertising.WinRT.UI;
using Windows.UI.Popups;
using Microsoft.AdMediator.Core.Events;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TheDailyTasks
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        InterstitialAd InerAd = new InterstitialAd();
        Timer timer;
        public MainPage()
        {
            this.InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public double OptimalWidth
        {
            get { return pageWidth; }
            set
            {
                pageWidth = value;
                OnPropertyChanged("OptimalWidth");
            }
        }

        private double pageWidth;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //InerAd.AdReady += InerAd_AdReady;
            //InerAd.ErrorOccurred += InerAd_ErrorOccurred;
            // InerAd.RequestAd(AdType.Video, "22f822ca-5b72-4977-9498-ed159ad5bb0d", "11605153");



            SecondaryFrame.DataContext = App.DataModel;
            App.DataModel.DesiredFrame = SecondaryFrame;

            SecondaryFrame = App.DataModel.DesiredFrame;  // podívat se na tuto část 
            App.DataModel.FrameMain = Frame;

            var TasksList = await App.DataModel.GetTasks();
            CheckNewestXmlFormat(TasksList);
            ShowUncompletedTasksOnTile();
            this.TasksList.ItemsSource = TasksList;
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;

            if (!App.DataModel.IsAdditionalPanelDisplayed)
                return;

            SelectContentForAdditionalPanel(SecondaryFrame);
            //var DetailPage = new ItemDetail(true, App.DataModel.ExaminedTask);
        }

        private void InerAd_ErrorOccurred(object sender, AdErrorEventArgs e)
        {

        }

        private void InerAd_AdReady(object sender, object e)
        {
            InerAd.Show();
        }

        private void ShowUncompletedTasksOnTile()
        {
            int Number = 0;

            if (TasksList.ItemsSource != null)
            {
                Number = TasksList.Items.Count(Uncompleted => ((((Tasks)Uncompleted).Dates.Contains(DateTime.Today)) != true) && ((Tasks)Uncompleted).Start <= DateTime.Today && ((Tasks)Uncompleted).End >= DateTime.Today);

                XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeNumber);
                XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
                badgeElement.SetAttribute("value", Number.ToString());
                BadgeNotification badge = new BadgeNotification(badgeXml);
                BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
            }

            if (Number == 0)
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
        }

        private void SelectContentForAdditionalPanel(Frame TargetFrame)
        {
            switch (App.DataModel.AdditionalPanelContent)
            {
                case "TheDailyTasks.Add":
                    //        if (App.DataModel.ExaminedTask == null)
                    //            DetailPage = new Add();
                    //        else
                    //        //DetailPage = new Add(App.DataModel.ExaminedTask);
                    //        if (TargetFrame != null)
                    //            //TargetFrame.Navigate(typeof(Add), App.DataModel.ExaminedTask);
                    //            TargetFrame.Content = App.DataModel.pok;
                    TargetFrame.Navigate(typeof(Add));
                    break;

                case "TheDailyTasks.ItemDetail":
                    //        /* DetailPage = new ItemDetail(true, App.DataModel.ExaminedTask);
                    //         if (TargetFrame != null)
                    //             TargetFrame.Navigate(typeof(ItemDetail), App.DataModel.ExaminedTask);*/
                    //        if (App.DataModel.ExaminedTask == null)
                    //            DetailPage = new ItemDetail();
                    //        else
                    //        //DetailPage = new Add(App.DataModel.ExaminedTask);
                    //        if (TargetFrame != null)
                    //            //TargetFrame.Navigate(typeof(Add), App.DataModel.ExaminedTask);
                    //            TargetFrame.Content = App.DataModel.pok;

                    TargetFrame.Navigate(typeof(ItemDetail));


                    break;
            }
        }

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            App.DataModel.IsAdditionalPanelDisplayed = true;
            App.DataModel.AdditionalPanelContent = typeof(Add).ToString();
            App.DataModel.ExaminedTask = null;
            if (App.DataModel.FrameMain.ActualWidth < 720)
                App.DataModel.FrameMain.Navigate(typeof(Add));
            else
                App.DataModel.DesiredFrame.Navigate(typeof(Add));
        }

        private void GridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != 0)
            {
                double colmuns = e.NewSize.Width / 320;

                if (colmuns < 2)
                    OptimalWidth = e.NewSize.Width - 5;
                else
                {
                    if (TasksList.Items?.Count + 1 > colmuns)
                    {
                        OptimalWidth = e.NewSize.Width / Math.Floor(colmuns) - 15;
                    }
                    else
                    {
                        OptimalWidth = e.NewSize.Width - 5;
                    }
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ItemDetail));
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Grid_Holding(sender, null);
        }

        private void Feedback_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Feedback));
        }

        private void CheckNewestXmlFormat(ObservableCollection<Tasks> Items)
        {
            if (Items.Count(x => (x.Description == null || x.NotifyDays == null || x.ID == 0 || x.NotifyDays.Count == 0) && (x.ID != -2)) != 0)
            {
                foreach (Tasks Item in Items.Where(x => x.Description == null || x.NotifyDays == null || x.ID == 0 || x.NotifyDays.Count == 0))
                {
                    if (Item.Description == null)
                        Item.Description = "";

                    if (Item.NotifyDays == null || Item.NotifyDays.Count == 0)
                    {
                        Item.NotifyDays = new ObservableCollection<DayOfWeek>();
                        Item.NotifyDays.Add(DayOfWeek.Monday);
                        Item.NotifyDays.Add(DayOfWeek.Tuesday);
                        Item.NotifyDays.Add(DayOfWeek.Wednesday);
                        Item.NotifyDays.Add(DayOfWeek.Thursday);
                        Item.NotifyDays.Add(DayOfWeek.Friday);
                        Item.NotifyDays.Add(DayOfWeek.Saturday);
                        Item.NotifyDays.Add(DayOfWeek.Sunday);
                    }

                    if (Item.ID == 0)
                    {
                        int Index = 1;
                        while (Items.Count(x => x.ID == Index) == 1)
                            Index++;
                        Item.ID = Index;
                    }
                }
                App.DataModel.Convert(Items);
            }
        }

        private void MainPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 720)
            {
                if (!App.DataModel.IsAdditionalPanelDisplayed)
                    return;                

                SelectContentForAdditionalPanel(Frame);
                //1App.DataModel.Data = Frame.Content;
                //Frame.Content = App.DataModel.DesiredFrame.Content;
                //var DetailPage = /*new Add();*/ new ItemDetail(true, App.DataModel.ExaminedTask);
            }
        }

        private void TasksList_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            ShowUncompletedTasksOnTile();
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (App.DataModel.IsAdditionalPanelDisplayed && (sender as Grid).Tag != null)
            {
                if (App.DataModel.ExaminedTask != App.DataModel.GetTasks((int)(sender as Grid).Tag))
                {
                    App.DataModel.AdditionalPanelContent = "TheDailyTasks.ItemDetail";
                    App.DataModel.ShowDetail(App.DataModel.GetTasks((int)(sender as Grid).Tag));
                }
                else
                {
                    App.DataModel.IsAdditionalPanelDisplayed = false;
                    App.DataModel.AdditionalPanelContent = "";
                }
            }
            else if ((sender as Grid).Tag == null)
            {
                if (!App.DataModel.IsAdditionalPanelDisplayed || App.DataModel.AdditionalPanelContent != "TheDailyTasks.Add" || App.DataModel.AdditionalPanelStyle == "Edit")
                    AddTask_Click(null, null);
                else
                {
                    App.DataModel.IsAdditionalPanelDisplayed = false;
                    App.DataModel.AdditionalPanelContent = "";
                }
            }
        }

        private void AdTDT_AdRefreshed(object sender, RoutedEventArgs e)
        {

        }

        private void AdTDT_AdMediatorError(object sender, Microsoft.AdMediator.Core.Events.AdMediatorFailedEventArgs e)
        {

        }
    }
}

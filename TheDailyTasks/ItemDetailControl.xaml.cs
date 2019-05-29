using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheDailyTasks
{
    public sealed partial class ItemDetailControl : UserControl, INotifyPropertyChanged
    {
        public ItemDetailControl()
        {
            this.InitializeComponent();
        }

        public ItemDetailControl(bool Navigate, Tasks Task)
        {
            this.InitializeComponent();
            Frame pok = Window.Current.Content as Frame;
            pok.Navigate(typeof(ItemDetail), Task);
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
    }
}

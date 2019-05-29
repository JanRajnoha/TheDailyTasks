using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using TaskManager;

namespace TheDailyTasks
{
    class EnableWhenNotify : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value == true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class EnableNeverending : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value != true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (((ObservableCollection<DateTime>)value).Contains(DateTime.Today))
            {
                return new SolidColorBrush(Color.FromArgb(255, 120, 255, 99));
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (((ObservableCollection<DateTime>)value).Contains(DateTime.Today))
            {
                return "";
            }
            else
            {
                return " ";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Tasks Task = (Tasks)value;

            if ((DateTime.Today > Task.End) || (DateTime.Today < Task.Start) || !Task.NotifyDays.Contains(DateTime.Today.DayOfWeek)) 
            { 
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteTaskToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var Task = (Tasks)value;

            if (((DateTime.Today == Task.End) && (Task.Dates.Contains(DateTime.Today)) || (DateTime.Today > Task.End)) || (DateTime.Today < ((Tasks)value).Start))
            {
                if (DateTime.Today < ((Tasks)value).Start)
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 162, 255));
                }
                else
                {
                    return new SolidColorBrush(Color.FromArgb(255, 0, 255, 50));
                }
            }
            else
            {
                return new SolidColorBrush(Color.FromArgb(255, 255, 48, 48));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteDayToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var Task = (Tasks)value;

            if (Task.Start <= DateTime.Today && Task.End >= DateTime.Today && Task.Dates.Contains(DateTime.Today))
            {
                if (Task.End == DateTime.Today)
                    return new SolidColorBrush(Color.FromArgb(255, 0, 255, 50));
                else
                    return new SolidColorBrush(Color.FromArgb(0, 0, 255, 0));
            }
            else
            {
                if (Task.Start > DateTime.Today)
                    return new SolidColorBrush(Color.FromArgb(255, 0, 162, 255));
                else if (Task.End < DateTime.Today)
                    return new SolidColorBrush(Color.FromArgb(255, 0, 255, 50));
                else if (Task.NotifyDays.Contains(DateTime.Today.DayOfWeek))
                    return new SolidColorBrush(Color.FromArgb(255, 255, 48, 48));
                else
                    return new SolidColorBrush(Color.FromArgb(0, 0, 255, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (bool)value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return (Visibility)value == Visibility.Visible;
        }
    }

    class GetCompleteDaysToToday : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Tasks Task = (Tasks)value;

            if (Task.Start <= DateTime.Today)
            {
                Functions fce = new Functions();

                var DatesToToday = DateTime.Today - Task.Start.AddDays(-1);
                var Days = fce.GetNumberOfSpecificDays(DatesToToday, Task.NotifyDays, Task.Start);
                int CompleteDaysToToday = Task.Dates.Count;

                if (DateTime.Today > Task.End)
                {
                    return "Activity has been completed";
                }
                else
                {
                    if (Days != 0)
                        return "Complete days to today: " + ((int)((double)CompleteDaysToToday / Days * 100)).ToString() + "%";
                    else
                        return "Task started with day, when isn't notifying";
                }
            }
            else
                return "Not started";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    class IsCompleteDayToForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var Task = (Tasks)value;

            bool White = true;

            if (Task.Start <= DateTime.Today && Task.End >= DateTime.Today && Task.Dates.Contains(DateTime.Today))
            {
                if (Task.End == DateTime.Today)
                    White = false;
            }
            else
            {
                if (Task.Start > DateTime.Today)
                    White = true;
                else if (Task.End < DateTime.Today)
                    White = false;
            }

            if (White)
                return new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            else
                return new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class TaskListTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TaskTemplate { get; set; }

        public DataTemplate AddTaskTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if ((item as Tasks).ID != -2)
                return TaskTemplate;
            else
                return AddTaskTemplate;
        }
    }
}

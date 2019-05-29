using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Storage;
using Windows.UI.Popups;
using System.Xml.Serialization;
using Windows.UI.Xaml.Controls;
using System.Linq;
using Windows.ApplicationModel.VoiceCommands;
using System.Globalization;
using System.Collections.Generic;
using Windows.ApplicationModel;
using TaskManager;

namespace TheDailyTasks
{
    public class Tasks:INotifyPropertyChanged
    {
        private ObservableCollection<DateTime> dates;
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public bool Neverend { get; set; }
        public bool Notify { get; set; }
        public DateTime WhenNotify { get; set; }

        public ObservableCollection<DateTime> Dates
        {
            get { return dates; }
            set
            {
                dates = value;
                NotifyPropertyChanged();
            }
        }

        public ObservableCollection<DayOfWeek> NotifyDays { get; set; }

        [XmlIgnore]
        public ICommand CompleteTask { get; set; }
        [XmlIgnore]
        public ICommand ShowDetail { get; set; }
        [XmlIgnore]
        public ICommand Delete { get; set; }
        [XmlIgnore]
        public ICommand Edit { get; set; }

        public Tasks()
        {
            ShowDetail = new ShowDetailButtonEvent();
            CompleteTask = new CompletedButtonEvent();
            Delete = new DeleteButtonEvent();
            Edit = new EditButtonEvent();
            Dates = new ObservableCollection<DateTime>();
            NotifyDays = new ObservableCollection<DayOfWeek>();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Adds today's date if it is not already added
        /// </summary>
        public void AddDate()
        {
            if (!Dates.Contains(DateTime.Today))
                Dates.Add(DateTime.Today);
            else
                Dates.Remove(DateTime.Today);

            NotifyPropertyChanged("Dates");           // to changed si nyní kolekce Dates zavolá sama
        }

        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TasksManager : INotifyPropertyChanged
    { 
        private string TaskFile = "Tasks.xml";

        public Frame DesiredFrame;
        public Tasks ExaminedTask;
        public object Data = null;
        public bool SizeForAdditionalPanel;
        public string AdditionalPanelStyle = "";

        ObservableCollection<Tasks> _Tasks;
        private bool isAdditionalPanelDisplayed;
        private double windowWidth;
        private string additionalPanelContent;
        public Frame FrameMain;

        /// <summary>
        /// Getting width of window
        /// </summary>
        public double WindowWidth
        {
            get { return windowWidth; }
            set
            {
                windowWidth = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// If the detail page is viwed or hidden
        /// </summary>
        public bool IsAdditionalPanelDisplayed
        {
            get { return isAdditionalPanelDisplayed; }
            set
            {
                isAdditionalPanelDisplayed = value;
                OnPropertyChanged();
            }
        }
        public async void UpdatePhraseList()
        {
            try
            {
                StorageFile VoiceCommandsStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommands.xml");

                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(VoiceCommandsStorageFile);

                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a trip, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                VoiceCommandDefinition commandDefinitions;

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "en-us";
                }

                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("TDTComandSet_" + countryCode, out commandDefinitions))
                {
                    List<string> TaskList = new List<string>();
                    foreach (Tasks Item in _Tasks)
                    {
                        if (Item.ID != -2)
                            TaskList.Add(Item.Name);
                    }

                    await commandDefinitions.SetPhraseListAsync("Tasks", TaskList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Updating Phrase list for VCDs: " + ex.ToString());
            }
        }

        /// <summary>
        /// Which page will be displayed
        /// </summary>
        public string AdditionalPanelContent
        {
            get { return additionalPanelContent; }
            set
            {
                additionalPanelContent = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TasksManager()
        {
            _Tasks = new ObservableCollection<Tasks>();
        }

        public async Task<ObservableCollection<Tasks>> GetTasks()
        {
            _Tasks = await ReadData();
            if (_Tasks.Count(x => x.ID == -2) == 0)
                _Tasks.Add(new Tasks() { ID = -2 });
            return _Tasks;
        }

        public async Task<ObservableCollection<Tasks>> ReadData(int Attempts = 0)
        {
            try
            {
                ObservableCollection<Tasks> ReadedTasks = new ObservableCollection<Tasks>();
                XmlSerializer Serializ = new XmlSerializer(typeof(ObservableCollection<Tasks>));
                Stream Steam = await ApplicationData.Current.LocalFolder.OpenStreamForReadAsync(TaskFile);
                using (Steam)
                {
                    ReadedTasks  = (ObservableCollection<Tasks>)Serializ.Deserialize(Steam);
                }

                if (ReadedTasks  == null)
                    return new ObservableCollection<Tasks>();
                else
                    return ReadedTasks;
            }
            catch (Exception s) when ((s.Message.Contains("denied")) && (Attempts < 10))
            {
                return await ReadData(Attempts + 1);
            }

            catch (Exception s)
            {
                return new ObservableCollection<Tasks>();
            }
        }

        private void UpdateAddItem()
        {
            _Tasks.RemoveAt(IndexOfTask(-2));
            _Tasks.Add(new Tasks() { ID = -2 });
        }

        internal async void AddTask(Tasks Item)
        {
            Tasks NewTask = new Tasks();
            NewTask = Item;

            if (NewTask.ID == -1)
                NewTask.ID = GetID();
            else
            {
                int index = _Tasks.IndexOf(NewTask);
                if (index == -1)
                {
                    index = IndexOfTask(NewTask, _Tasks);
                }
                _Tasks.RemoveAt(index);
            }

            _Tasks.Add(NewTask);

            UpdateAddItem();
            await SaveTasks();

            UpdatePhraseList();
        }

        private int GetID()
        {
            int Index = -1;
            for (int i = 0; i < _Tasks.Count; i++)
            {
                bool ExistID = false;
                for (int j = 0; j < _Tasks.Count; j++)
                {
                    if (_Tasks[j].ID == i + 1)
                    {
                        ExistID = true;
                        break;
                    }
                }

                if (ExistID == false)
                {
                    Index = i + 1;
                    break;
                }
            }

            if (Index == -1)
                Index = _Tasks.Count + 1;

            return Index;
        }

        public async void Convert(ObservableCollection<Tasks> items)
        {
            await SaveTasks();
        }

        private async Task SaveTasks(int Attempts = 0)
        {
            try
            {
                var TasksToSerialize = new ObservableCollection<Tasks>(_Tasks.Where(x => x.ID > 0));

                XmlSerializer Serializ = new XmlSerializer(typeof(ObservableCollection<Tasks>));
                
                using (Stream Steam = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync(TaskFile, CreationCollisionOption.ReplaceExisting))
                {
                    Serializ.Serialize(Steam, TasksToSerialize);
                }
            }

            catch (Exception s) when ((s.Message.Contains("denied")) && (Attempts < 10))
            {
                await SaveTasks(Attempts + 1);
            }

            catch (Exception s)
            {
                MessageDialog msds = new MessageDialog(s.ToString(), "error");
                await msds.ShowAsync();
            }

            await TDTSharedCode.BackgroundTask(false);
        }

        public async void CompleteTask(Tasks CompletedTask)
        {
            var ActualTasks = await ReadData();

            int index = _Tasks.IndexOf(CompletedTask);
            if (index == -1)
            {
                index = IndexOfTask(CompletedTask, _Tasks);
                if (index == -1)
                    return;
            }
            
            _Tasks[index].AddDate();
            await SaveTasks();
        }

        public void ShowDetail(Tasks DetailedTask)
        {
            int index = _Tasks.IndexOf(DetailedTask);
            if (index == -1)
            {
                index = IndexOfTask(DetailedTask, _Tasks);
            }
            ExaminedTask = DetailedTask;
            if (FrameMain.ActualWidth < 720)
                FrameMain.Navigate(typeof(ItemDetail));
            else
                DesiredFrame.Navigate(typeof(ItemDetail));
        }

        public List<string> GetListOfTasks()
        {
            return _Tasks.Select(x => x.Name).ToList();
        }

        public async void Delete(Tasks DetailedTask)
        {
            //await GetTasks();
            int index = _Tasks.IndexOf(DetailedTask);
            if (index == -1)
            {
                index = IndexOfTask(DetailedTask, _Tasks);
            }

            UpdatePhraseList();

            _Tasks.RemoveAt(index);
            await SaveTasks();
        }

        public void Edit(Tasks EditedTask)
        {
            int index = _Tasks.IndexOf(EditedTask);
            ExaminedTask = EditedTask;
            if (index == -1)
            {
                index = IndexOfTask(EditedTask, _Tasks);
            }
            
            if (FrameMain.ActualWidth < 720)
                FrameMain.Navigate(typeof(Add));
            else
                DesiredFrame.Navigate(typeof(Add));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal Tasks GetTasks(int Tag)
        {
            return _Tasks[IndexOfTask(Tag)];
        }

        private int IndexOfTask(int tag)
        {
            int Index = -1;

            foreach (Tasks task in _Tasks)
            {
                bool Result = false;

                Result = task.ID == tag;

                Index++;

                if (Result)
                    return Index;
            }

            return Index;
        }

        public int IndexOfTask(Tasks ExaminedTask, ObservableCollection<Tasks> Source)
        {
            int Index = -1;

            foreach (Tasks task in Source)
            {
                bool Result = false;

                Result = task.ID == ExaminedTask.ID;

                Index++;

                if (Result)
                    return Index;
            }

            return Index;
        }
    }
}

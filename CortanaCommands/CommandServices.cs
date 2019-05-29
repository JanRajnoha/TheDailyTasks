using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TaskManager;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources.Core;
using Windows.ApplicationModel.VoiceCommands;
using System.IO;
using System.Collections.ObjectModel;
using System.Xml.Serialization;
using Windows.Storage;
using Reminder;
using Windows.ApplicationModel;

namespace CortanaCommands
{
    public sealed class CommandServices : IBackgroundTask
    {
        VoiceCommandServiceConnection VoiceServiceConnection;
        BackgroundTaskDeferral ServiceDeferral;
      /*  ResourceMap cortanaResourceMap;
        ResourceContext cortanaContext;
        DateTimeFormatInfo dateFormatInfo;*/

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            ServiceDeferral = taskInstance.GetDeferral();

            taskInstance.Canceled += TaskInstance_Canceled;

            var TriggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (TriggerDetails != null && TriggerDetails.Name == "TDTCortanaCommandServices")
            {

                VoiceServiceConnection = VoiceCommandServiceConnection.FromAppServiceTriggerDetails(TriggerDetails);

                VoiceServiceConnection.VoiceCommandCompleted += OnVoiceCommandCompleted;

                // GetVoiceCommandAsync establishes initial connection to Cortana, and must be called prior to any 
                // messages sent to Cortana. Attempting to use ReportSuccessAsync, ReportProgressAsync, etc
                // prior to calling this will produce undefined behavior.
                VoiceCommand voiceCommand = await VoiceServiceConnection.GetVoiceCommandAsync();

                switch (voiceCommand.CommandName)
                {
                    case "ShowMyTasks":
                        string Style;
                        try
                        {
                            Style = voiceCommand.Properties["Style"][0];
                        }
                        catch
                        {
                            Style = "";
                        }
                        await ShowTasks(Style, voiceCommand.SpeechRecognitionResult.Text.Contains("today"));
                        break;

                    case "CompleteSpecificTask":
                        var CompletedTask = voiceCommand.Properties["Tasks"][0];
                        CompleteTask(CompletedTask);
                        break;

                    case "DeleteSpecificTask":
                        var DeletedTask = voiceCommand.Properties["Tasks"][0];
                        DeleteTask(DeletedTask);
                        break;

                    case "ShowDetailForSpecificTask":
                        var DetailedTask = voiceCommand.Properties["Tasks"][0];
                        ShowDetailsOfTask(DetailedTask);
                        break;
                    default:
                        // As with app activation VCDs, we need to handle the possibility that
                        // an app update may remove a voice command that is still registered.
                        // This can happen if the user hasn't run an app since an update.
                        //LaunchAppInForeground();
                        break;
                }
            }
        }

        private async void ShowDetailsOfTask(string DetailedTask)
        {
            await ShowProgressScreen("Looking for your tasks.");

            List<string> ListOfTasks;
            ListOfTasks = GetListOfTasks().Result.Where(x => x.Name == DetailedTask).Select(x => x.Name).ToList();

            VoiceCommandUserMessage UserMessage = new VoiceCommandUserMessage();
            List<VoiceCommandContentTile> TaskTiles = new List<VoiceCommandContentTile>();
            VoiceCommandResponse Response;

            if (ListOfTasks.Count == 0)
            {
                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Sorry, you don't have any tasks.";

                Response = VoiceCommandResponse.CreateResponse(UserMessage);
                await VoiceServiceConnection.ReportSuccessAsync(Response);
            }
            else
            {
                string Message = "";
                if (ListOfTasks.Count != 1)
                {
                    Message = $"Here are your tasks. Please, select one.";
                    string SelectedTask = await GetSingleTask(ListOfTasks, Message, "Which task do you want to show?");
                    ListOfTasks.Clear();
                    ListOfTasks.Add(SelectedTask);
                }
                await ShowProgressScreen("Showing your task.");

                int ID = GetListOfTasks().Result.Where(x => x.Name == ListOfTasks[0]).Select(x => x.ID).FirstOrDefault();
                ObservableCollection<Tasks> TasksList = await GetListOfTasks();
                Tasks InspectedTask = TasksList.Where(x => x.Name == DetailedTask).FirstOrDefault();

                UserMessage.DisplayMessage = UserMessage.SpokenMessage = Message;

                Functions fce = new Functions();

                var DatesToToday = DateTime.Today - InspectedTask.Start.AddDays(-1);
                int Days = fce.GetNumberOfSpecificDays(DatesToToday, InspectedTask.NotifyDays, InspectedTask.Start);
                int CompleteDaysToToday = InspectedTask.Dates.Select(x => x.Date <= DateTime.Today).Count();

                var TaskTile = new VoiceCommandContentTile();

                TaskTile.ContentTileType = VoiceCommandContentTileType.TitleWithText;

                TaskTile.AppLaunchArgument = InspectedTask.Name;
                TaskTile.Title = "Task name";
                TaskTile.TextLine1 = InspectedTask.Name;
                TaskTiles.Add(TaskTile);

                TaskTile.Title = "State";
                TaskTile.TextLine1 = "Complete days: " + (DateTime.Today > InspectedTask.End ? "Activity has been completed" : ((int)((double)CompleteDaysToToday / Days * 100)).ToString() + "%");
                TaskTiles.Add(TaskTile);

                TaskTile.Title = "Informations";
                TaskTile.TextLine1 = "Start: " + InspectedTask.Start;
                TaskTile.TextLine2 = "End: " + InspectedTask.End;
                TaskTiles.Add(TaskTile);
                TaskTile.TextLine2 = "";

                TaskTile.Title = "Status";
                TaskTile.TextLine1 = DateTime.Today > InspectedTask.End ? "Completed" : DateTime.Today < InspectedTask.Start ? "Not started" : "Uncompleted" ;
                TaskTiles.Add(TaskTile);
            }

            UserMessage.DisplayMessage = UserMessage.SpokenMessage = "Here is your task";
            Response = VoiceCommandResponse.CreateResponse(UserMessage, TaskTiles);
            await VoiceServiceConnection.ReportSuccessAsync(Response);
        }

        private async void DeleteTask(string DeletedTask)
        {
            await ShowProgressScreen("Looking for your tasks.");

            List<string> ListOfTasks;
            ListOfTasks = GetListOfTasks().Result.Where(x => x.Name == DeletedTask).Select(x => x.Name).ToList();

            VoiceCommandUserMessage UserMessage = new VoiceCommandUserMessage();
            List<VoiceCommandContentTile> TaskTiles = new List<VoiceCommandContentTile>();
            VoiceCommandResponse Response;

            if (ListOfTasks.Count == 0)
            {
                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Sorry, you don't have any tasks.";

                Response = VoiceCommandResponse.CreateResponse(UserMessage);
                await VoiceServiceConnection.ReportSuccessAsync(Response);
            }
            else
            {
                string Message = "";
                if (ListOfTasks.Count != 1)
                {
                    Message = $"Here are your tasks. Please, select one.";
                    string SelectedTask = await GetSingleTask(ListOfTasks, Message, "Which task do you want to delete?");
                    ListOfTasks.Clear();
                    ListOfTasks.Add(SelectedTask);
                }

                var UserReMessage = new VoiceCommandUserMessage();

                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Do you want delete this {ListOfTasks[0]}?";
                UserReMessage.DisplayMessage = UserReMessage.SpokenMessage = "Do you want delete it?";

                Response = VoiceCommandResponse.CreateResponseForPrompt(UserMessage, UserReMessage);

                var VoiceCommandConfirmation = await VoiceServiceConnection.RequestConfirmationAsync(Response);

                // If RequestConfirmationAsync returns null, Cortana's UI has likely been dismissed.
                if (VoiceCommandConfirmation != null)
                {
                    if (VoiceCommandConfirmation.Confirmed == true)
                    {
                        await ShowProgressScreen("Deleting your task.");

                        int ID = GetListOfTasks().Result.Where(x => x.Name == ListOfTasks[0]).Select(x => x.ID).FirstOrDefault();
                        ObservableCollection<Tasks> TasksList = await GetListOfTasks();

                        await CompleteTask(ID, TasksList);
                    }
                    else
                    {
                        UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"All right. {ListOfTasks[0]} won't be deleted.";

                        Response = VoiceCommandResponse.CreateResponse(UserMessage);
                        await VoiceServiceConnection.ReportSuccessAsync(Response);

                        return;
                    }
                }
            }

            UserMessage.DisplayMessage = UserMessage.SpokenMessage = "Your task has been deleted";
            Response = VoiceCommandResponse.CreateResponse(UserMessage);
            await VoiceServiceConnection.ReportSuccessAsync(Response);

            try
            {
                StorageFile VoiceCommandsStorageFile = await Package.Current.InstalledLocation.GetFileAsync(@"VoiceCommands.xml");

                await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(VoiceCommandsStorageFile);

                UpdatePhraseList();
            }
            catch (Exception)
            {
                //throw;
            }
        }

        private async Task ShowTasks(string Style, bool AskForToday)
        {
            string Space = Style != "" ? " " : "";
            await ShowProgressScreen($"Showing your {Style}{Space}tasks.");
            bool CheckStyle = Style == " " ? false : true;
            List<string> ListOfTasks;
            switch (Style)
            {
                case "completed":
                    if (AskForToday)
                        ListOfTasks = GetListOfTasks().Result.Where(x => x.Dates.Contains(DateTime.Today)).Select(x => x.Name).ToList();
                    else
                        ListOfTasks = GetListOfTasks().Result.Where(x => x.End < DateTime.Today).Select(x => x.Name).ToList();
                    break;

                case "uncompleted":
                    if (AskForToday)
                        ListOfTasks = GetListOfTasks().Result.Where(x => !x.Dates.Contains(DateTime.Today)).Select(x => x.Name).ToList();
                    else
                        ListOfTasks = GetListOfTasks().Result.Where(x => x.End >= DateTime.Today && x.Start <= DateTime.Today).Select(x => x.Name).ToList();
                    break;

                case "not started":
                    ListOfTasks = GetListOfTasks().Result.Where(x => x.Start > DateTime.Today).Select(x => x.Name).ToList();
                    break;

                default:
                    if (AskForToday)
                        ListOfTasks = GetListOfTasks().Result.Where(x => x.End >= DateTime.Today && x.Start <= DateTime.Today).Select(x => x.Name).ToList();
                    else
                        ListOfTasks = GetListOfTasks().Result.Select(x => x.Name).ToList();
                    break;
            }

            VoiceCommandUserMessage UserMessage = new VoiceCommandUserMessage();
            List<VoiceCommandContentTile> TaskTiles = new List<VoiceCommandContentTile>();
            string Today = AskForToday ? " today." : ".";

            if (ListOfTasks.Count == 0)
            {
                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Sorry, you don't have any {Style}{Space}tasks{Today}";
            }
            else
            {
                string Message;
                if (ListOfTasks.Count == 1)
                {
                    Message = $"Here's your {Style}{Space}task{Today}";
                }
                else
                {
                    Message = $"Here are your {Style}{Space}tasks{Today}";
                }
                UserMessage.DisplayMessage = UserMessage.SpokenMessage = Message;

                int i = 1;
                foreach (string Task in ListOfTasks)
                {
                    if (i <= 10)
                    {
                        var TaskTile = new VoiceCommandContentTile();

                        TaskTile.ContentTileType = VoiceCommandContentTileType.TitleOnly;

                        TaskTile.AppLaunchArgument = Task;
                        TaskTile.Title = Task;

                        TaskTiles.Add(TaskTile);
                        i++;
                    }
                    else
                    {
                        UserMessage.SpokenMessage += " I'm showing only first ten.";
                        break;
                    }
                }
            }

            var Response = VoiceCommandResponse.CreateResponse(UserMessage, TaskTiles);
            Response.AppLaunchArgument = ListOfTasks[0];

            await VoiceServiceConnection.ReportSuccessAsync(Response);
        }

        private async Task ShowProgressScreen(string LoadingScreen)
        {
            var UserProgressMessage = new VoiceCommandUserMessage();
            UserProgressMessage.DisplayMessage = UserProgressMessage.SpokenMessage = LoadingScreen;

            VoiceCommandResponse response = VoiceCommandResponse.CreateResponse(UserProgressMessage);
            await VoiceServiceConnection.ReportProgressAsync(response);
        }

        private async void CompleteTask(string CompletedTask)
        {
            await ShowProgressScreen("Looking for your tasks.");

            List<string> ListOfTasks;
            ListOfTasks = GetListOfTasks().Result.Where(x => !x.Dates.Contains(DateTime.Today) && x.Name == CompletedTask).Select(x => x.Name).ToList();

            VoiceCommandUserMessage UserMessage = new VoiceCommandUserMessage();
            List<VoiceCommandContentTile> TaskTiles = new List<VoiceCommandContentTile>();
            VoiceCommandResponse Response;

            if (ListOfTasks.Count == 0)
            {
                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Sorry, you don't have any uncompleted tasks today.";

                Response = VoiceCommandResponse.CreateResponse(UserMessage);
                await VoiceServiceConnection.ReportSuccessAsync(Response);
            }
            else
            {
                string Message = "";
                if (ListOfTasks.Count != 1)
                {
                    Message = $"Here are your uncompleted tasks. Please, select one.";
                    string SelectedTask = await GetSingleTask(ListOfTasks, Message, "Which task do you want to complete?");
                    ListOfTasks.Clear();
                    ListOfTasks.Add(SelectedTask);
                }
                
                var UserReMessage = new VoiceCommandUserMessage();

                UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"Do you want complete this {ListOfTasks[0]}?";
                UserReMessage.DisplayMessage = UserReMessage.SpokenMessage = "Do you want complete it?";

                Response = VoiceCommandResponse.CreateResponseForPrompt(UserMessage, UserReMessage);

                var VoiceCommandConfirmation = await VoiceServiceConnection.RequestConfirmationAsync(Response);

                // If RequestConfirmationAsync returns null, Cortana's UI has likely been dismissed.
                if (VoiceCommandConfirmation != null)
                {
                    if (VoiceCommandConfirmation.Confirmed == true)
                    {
                        await ShowProgressScreen("Completing your task.");

                        ObservableCollection<Tasks> TasksList = await GetListOfTasks();
                        Tasks SelectedTask = TasksList.Where(x => x.Name == ListOfTasks[0]).FirstOrDefault();
                        int ID = TasksList.IndexOf(SelectedTask);

                        await CompleteTask(ID, TasksList);
                    }
                    else
                    {
                        UserMessage.DisplayMessage = UserMessage.SpokenMessage = $"All right. {ListOfTasks[0]} won't be completed.";

                        Response = VoiceCommandResponse.CreateResponse(UserMessage);
                        await VoiceServiceConnection.ReportSuccessAsync(Response);

                        return;
                    }
                }
            }

            UserMessage.DisplayMessage = UserMessage.SpokenMessage = "Your task has been completed";
            Response = VoiceCommandResponse.CreateResponse(UserMessage);
            await VoiceServiceConnection.ReportSuccessAsync(Response);
        }

        private async Task CompleteTask(int ID, ObservableCollection<Tasks> _Tasks)
        {
            if (!_Tasks[ID].Dates.Contains(DateTime.Today))
                _Tasks[ID].Dates.Add(DateTime.Today);
            IStorageFile StorageFile = await GetStream();
            await SaveTasks(await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(StorageFile), _Tasks);
        }

        private async Task<int> CheckInput(string SelectedTask, string Message, string SecondMessage)
        {
            var UserMessage = new VoiceCommandUserMessage();
            var UserReMessage = new VoiceCommandUserMessage();

            UserMessage.DisplayMessage = UserMessage.SpokenMessage = Message;
            UserReMessage.DisplayMessage = UserReMessage.SpokenMessage = SecondMessage;
            
            var Response = VoiceCommandResponse.CreateResponseForPrompt(UserMessage, UserReMessage);

            var VoiceCommandConfirmation = await VoiceServiceConnection.RequestConfirmationAsync(Response);

            // If RequestConfirmationAsync returns null, Cortana's UI has likely been dismissed.
            if (VoiceCommandConfirmation != null)
            {
                if (VoiceCommandConfirmation.Confirmed == true)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            return -1;
        }

        private async Task<string> GetSingleTask(List<string> ListOfTasks, string Message, string SecondMessage)
        {
            var UserMessage = new VoiceCommandUserMessage();
            UserMessage.DisplayMessage = UserMessage.SpokenMessage = Message;

            var UserReMessage = new VoiceCommandUserMessage();
            UserReMessage.DisplayMessage = UserReMessage.SpokenMessage = SecondMessage;
            string SelectedTask;

            int Result;
            do
            {
                var TaskTiles = new List<VoiceCommandContentTile>();
                int i = 1;
                foreach (string Task in ListOfTasks)
                {
                    if (i <= 10)
                    {
                        var TaskTile = new VoiceCommandContentTile();

                        TaskTile.ContentTileType = VoiceCommandContentTileType.TitleOnly;

                        TaskTile.AppContext = TaskTile;

                        TaskTile.AppLaunchArgument = Task;
                        TaskTile.Title = Task;

                        TaskTiles.Add(TaskTile);
                        i++;
                    }
                    else
                    {
                        UserMessage.SpokenMessage += " I'm showing only first ten.";
                        break;
                    }
                }

                // Cortana will handle re-prompting if the user does not provide a valid response.
                var Response = VoiceCommandResponse.CreateResponseForPrompt(UserMessage, UserReMessage, TaskTiles);

                // If cortana is dismissed in this operation, null will be returned.
                var VoiceCommandDisambiguationResult = await VoiceServiceConnection.RequestDisambiguationAsync(Response);
                if (VoiceCommandDisambiguationResult != null)
                    SelectedTask = (string)VoiceCommandDisambiguationResult.SelectedItem.AppContext;
                else
                    return null;

                Result = await CheckInput(SelectedTask, Message: $"Is {SelectedTask} your task?", SecondMessage: $"Is {SelectedTask} your task?");
                if (Result == -1)
                    return null;    
            }
            while (Result == 0);

            return SelectedTask;
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            if (this.ServiceDeferral != null)
            {
                this.ServiceDeferral.Complete();
            }
        }

        private void TaskInstance_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            throw new NotImplementedException();
        }

        private async Task<ObservableCollection<Tasks>> GetListOfTasks()
        {
            IStorageFile IStorageFileTasks = await GetStream();

            return GetTasks(await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(IStorageFileTasks));
        }

        private static async Task<IStorageFile> GetStream()
        {
            var StorageFileTasks = await ApplicationData.Current.LocalFolder.GetFileAsync("Tasks.xml");

            var IStorageFileTasks = (IStorageFile)StorageFileTasks;
            return IStorageFileTasks;
        }

        private ObservableCollection<Tasks> GetTasks(Stream stream, int Attempts = 0)
        {
            ObservableCollection<Tasks> _Tasks = new ObservableCollection<Tasks>();
            try
            {
                XmlSerializer Serializer = new XmlSerializer(typeof(ObservableCollection<Tasks>));

                using (Stream Steam = stream)
                {
                    _Tasks = (ObservableCollection<Tasks>)Serializer.Deserialize(Steam);
                }

                if (_Tasks == null)
                    return new ObservableCollection<Tasks>();
                else
                    return _Tasks;
            }
            catch (Exception s) when ((s.Message.Contains("denied")) && (Attempts < 10))
            {
                return GetTasks(stream, Attempts + 1);
            }

            catch (Exception s)
            {
                return new ObservableCollection<Tasks>();
            }
        }

        private async Task SaveTasks(Stream Stream, ObservableCollection<Tasks> _Tasks, int Attempts = 0)
        {
            try
            {
                var TasksToSerialize = new ObservableCollection<Tasks>(_Tasks.Where(x => x.ID > 0));

                XmlSerializer Serializ = new XmlSerializer(typeof(ObservableCollection<Tasks>));

                using (Stream Steam = await ApplicationData.Current.LocalFolder.OpenStreamForWriteAsync("Tasks.xml", CreationCollisionOption.ReplaceExisting))
                {
                    Serializ.Serialize(Steam, TasksToSerialize);
                }
            }

            catch (Exception s) when ((s.Message.Contains("denied")) && (Attempts < 10))
            {
                await SaveTasks(Stream, _Tasks, Attempts + 1);
            }

            catch (Exception s)
            { }

            await TDTSharedCode.BackgroundTask();
        }

        public async void UpdatePhraseList()
        {
            try
            {
                // Update the destination phrase list, so that Cortana voice commands can use destinations added by users.
                // When saving a trip, the UI navigates automatically back to this page, so the phrase list will be
                // updated automatically.
                VoiceCommandDefinition commandDefinitions;

                var TasksSource = await GetListOfTasks();

                string countryCode = CultureInfo.CurrentCulture.Name.ToLower();
                if (countryCode.Length == 0)
                {
                    countryCode = "en-us";
                }

                if (VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue("TDTComandSet_" + countryCode, out commandDefinitions))
                {
                    List<string> TaskList = new List<string>();
                    foreach (Tasks Item in TasksSource)
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
    }
}

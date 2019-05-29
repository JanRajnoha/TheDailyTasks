using Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.UI.Notifications;

namespace TaskManager
{
    public class TDTSharedCode
    {
        public static async Task BackgroundTask(bool NotifyUser = true)
        {
            var StorageFileTasks = await ApplicationData.Current.LocalFolder.GetFileAsync("Tasks.xml");

            var IStorageFileTasks = (IStorageFile)StorageFileTasks;

            var TaskItems = GetTasks(await WindowsRuntimeStorageExtensions.OpenStreamForReadAsync(IStorageFileTasks), 0);

            int NotifyQueue = 0;
            ObservableCollection<Tasks> Queue = new ObservableCollection<Tasks>();

            foreach (var TaskItem in TaskItems)
            {
                bool Notify = false;

                int CorrectionOfMinuteRange;

                CorrectionOfMinuteRange = (TaskItem.WhenNotify.Minute - 10 < 0) ? 0 : TaskItem.WhenNotify.Minute - 10;

                if ((TaskItem.WhenNotify.Hour == DateTime.Now.Hour) && (Enumerable.Range(CorrectionOfMinuteRange, 20).Contains(DateTime.Now.Minute)))
                {
                    Notify = true;
                }

                #region OldVersionCheckHour 
                /*
                bool CheckAnotherHour = false;
                                    int Minute = -1;

                                    if ((TaskItem.WhenNotify.Minute >= 50) || (TaskItem.WhenNotify.Minute <= 10))
                                    {
                                        Minute = 60 - TaskItem.WhenNotify.Minute;
                                        CheckAnotherHour = true;
                                    }

                                    if (CheckAnotherHour)
                                    {
                                 */
                #endregion

                if (((TaskItem.WhenNotify.Minute >= 50) && (TaskItem.WhenNotify.Hour + 1 == DateTime.Now.Hour) && (Enumerable.Range(0, 60 - TaskItem.WhenNotify.Minute).Contains(DateTime.Now.Minute))) || ((TaskItem.WhenNotify.Minute <= 10) && (TaskItem.WhenNotify.Hour - 1 == DateTime.Now.Hour) && (Enumerable.Range(50, 60 - TaskItem.WhenNotify.Minute).Contains(DateTime.Now.Minute))))
                {
                    Notify = true;
                }

                if ((!TaskItem.Dates.Contains(DateTime.Today)) && ((TaskItem.End >= DateTime.Today) && (TaskItem.Start <= DateTime.Today)) && Notify && TaskItem.Notify && TaskItem.NotifyDays.Contains(DateTime.Today.DayOfWeek))
                {
                    if (NotifyUser)
                        Notifications.SendNotification(new string[2] { "Uncompleted task", $"Task {TaskItem.Name} isn't completed." }, ToastTemplateType.ToastText02);

                    if (NotifyQueue <= 5)
                    {
                        NotifyQueue++;

                        Queue.Add(TaskItem);
                    }
                }
            }

            SendLiveTiles(TaskItems, Queue);
        }

        private static void SendLiveTiles(ObservableCollection<Tasks> TaskItems, ObservableCollection<Tasks> Queue)
        {
            string[] UncompletedTasks = GetUncompletedTaskInfo(TaskItems);

            if (UncompletedTasks != null)
            {
                LiveTiles.UpdateBadge(UncompletedTasks.Length.ToString());
            }
            else
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            TileUpdateManager.CreateTileUpdaterForApplication().Clear();

            TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

            if (UncompletedTasks != null && Queue.Count == 0 && UncompletedTasks.Length != 0)
            {
                string HeadTile = $@"<tile>
                                        <visual>
                                           <binding template='TileWideText01'>
                                              <text id='1'>Uncompleted tasks</text>
                                              {GetLiveTilesRows(UncompletedTasks, 0, 2, 3)}
                                           </binding>
                                           <binding template='TileSquareText01'>
                                              <text id='1'>Uncompleted</text>
                                              {GetLiveTilesRows(UncompletedTasks, 0, 2, 3)}
                                           </binding>
                                           <binding template='TileSquare310x310Text01'>
                                              <text id='1'>Uncompleted tasks</text>
                                              {GetLiveTilesRows(UncompletedTasks, 0, 2, 9)}
                                           </binding>
                                        </visual>
                                     </tile>";

                LiveTiles.LiveTileUpdater(HeadTile);

                GetTiles(UncompletedTasks);
            }
            else
                foreach (var Item in Queue)
                {
                    string tile = $@"<tile>
                                        <visual>
                                           <binding template='TileWideText09'>
                                              <text id='1'>{Item.Name}</text>
                                              <text id='2'>{Item.Description}</text>
                                           </binding>
                                           <binding template='TileSquareText02'>
                                              <text id='1'>{Item.Name}</text>
                                              <text id='2'>{Item.Description}</text>
                                           </binding>
                                           <binding template='TileSquare310x310TextList03'>
                                              <text id='1'>{Item.Name}</text>
                                              <text id='2'>{Item.Description}</text>
                                              <text id='3'>Days complete</text>
                                              <text id='4'>{Item.Dates.Count.ToString() + " of " + ((Item.End - Item.Start).TotalDays + 1).ToString()}</text>
                                              <text id='5'>Remaining days</text>
                                              <text id='6'>{(Item.End - DateTime.Today).Days + " days"}</text>
                                           </binding>
                                        </visual>
                                     </tile>";

                    LiveTiles.LiveTileUpdater(tile);
                }
        }

        private static void GetTiles(string[] UncompletedTasks)
        {
            int Tiles = 2;
            int RowsMediumWide = 3;
            int RowsLarge = 9;
            string LiveTile = "";

            while (Tiles <= 5)
            {
                string MediumWideTileContent = GetLiveTilesRows(UncompletedTasks, RowsMediumWide, MaxRows: 4);

                if (MediumWideTileContent.Length != 0)
                    LiveTile = $@"<tile>
                                     <visual>
                                        <binding template='TileWideText05'>
                                           {MediumWideTileContent}
                                        </binding>
                                        <binding template='TileSquareText03'>
                                           {MediumWideTileContent}
                                        </binding>";

                string LargeTileContent = GetLiveTilesRows(UncompletedTasks, RowsLarge, MaxRows: 11);

                if (LargeTileContent.Length != 0)
                    LiveTile += $@" <binding template = 'TileSquare310x310Text03'>
                                       {LargeTileContent}
                                    </binding>";


                LiveTile += @"   </visual>
                              </tile> ";

                if (MediumWideTileContent.Length == 0 && LargeTileContent.Length == 0)
                    break;

                LiveTiles.LiveTileUpdater(LiveTile);

                RowsMediumWide += 4;
                RowsLarge += 11;
                Tiles++;
            }
        }

        private static string GetLiveTilesRows(string[] UncompletedTasks, int Rows, int StartedRows = 1, int MaxRows = 0)
        {
            string LiveTileRows = "";
            int IdRow = StartedRows;

            while (Rows <= UncompletedTasks.Length - 1 && ((MaxRows != 0 && IdRow <= MaxRows + StartedRows - 1) || MaxRows == 0))
            {
                LiveTileRows += $@"<text id='{IdRow.ToString()}'>{UncompletedTasks[Rows]}</text>";
                IdRow++;
                Rows++;

            }

            return LiveTileRows;
        }

        private static string[] GetUncompletedTaskInfo(ObservableCollection<Tasks> TaskItems)
        {
            int Number = 0;
            string[] UncompletedTasks;

            if ((TaskItems.Count != 0) || (TaskItems != null))
            {
                UncompletedTasks = new string[TaskItems.Count(Uncompleted => Uncompleted.Dates.Contains(DateTime.Today) != true && Uncompleted.Start <= DateTime.Today && Uncompleted.End >= DateTime.Today && Uncompleted.NotifyDays.Contains(DateTime.Today.DayOfWeek))];
            }
            else
                return null;

            int i = 0;

            foreach (var Task in TaskItems)
            {
                if ((!Task.Dates.Contains(DateTime.Today)) && ((Task.End >= DateTime.Today) && (Task.Start <= DateTime.Today)) && Task.NotifyDays.Contains(DateTime.Today.DayOfWeek))
                {
                    UncompletedTasks[i] = Task.Name;
                    i++;
                }
            }

            return UncompletedTasks;
        }

        private static ObservableCollection<Tasks> GetTasks(Stream stream, int Attempts)
        {
            ObservableCollection<Tasks> _Tasks = new ObservableCollection<Tasks>();
            try
            {
                XmlSerializer pok = new XmlSerializer(typeof(ObservableCollection<Tasks>));

                using (Stream Steam = stream)
                {
                    _Tasks = (ObservableCollection<Tasks>)pok.Deserialize(Steam);
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
    }

    public class Functions
    {
        public Functions()
        { }

        public int GetNumberOfSpecificDays(TimeSpan DatesToToday, ObservableCollection<DayOfWeek> NotifyDays, DateTime Start, DateTime EndDay = new DateTime())
        {
            int SpecificDaysCount = 0;

            if (EndDay == new DateTime())
            {
                EndDay = DateTime.Today;
            }

            var StartDay = (int)(8 - Start.DayOfWeek);
            if (StartDay == 8)
                StartDay = 1;

            var TodayDay = (int)(EndDay.DayOfWeek);
            if (TodayDay == 0)
                TodayDay = 7;

            int FullWeeksDays = (int)DatesToToday.TotalDays - StartDay - TodayDay;

            switch (FullWeeksDays)
            {
                case -7:
                    for (int i = 8 - StartDay; i <= TodayDay; i++)
                    {
                        if (NotifyDays.Contains(i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i))
                        {
                            SpecificDaysCount++;
                        }
                    }
                    break;

                case 0:

                    if (StartDay + TodayDay >= 7)
                    {
                        SpecificDaysCount = NotifyDays.Count;

                        for (int i = 8 - StartDay; i <= TodayDay; i++)
                        {
                            if (NotifyDays.Contains(i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i))
                            {
                                SpecificDaysCount++;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 8 - StartDay; i != TodayDay; i++)
                        {
                            if (NotifyDays.Contains(i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i))
                            {
                                SpecificDaysCount++;
                            }

                            if (i == 7)
                                i -= 7;
                        }

                        if (NotifyDays.Contains((DayOfWeek)TodayDay))
                        {
                            SpecificDaysCount++;
                        }
                    }

                    break;

                default:

                    FullWeeksDays = FullWeeksDays / 7;

                    if (StartDay + TodayDay >= 7)
                        FullWeeksDays++;

                    SpecificDaysCount = FullWeeksDays * NotifyDays.Count;


                    if (StartDay + TodayDay >= 7)
                    {
                        for (int i = 8 - StartDay; i <= TodayDay; i++)
                        {
                            if (NotifyDays.Contains(i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i))
                            {
                                SpecificDaysCount++;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 8 - StartDay; i != TodayDay; i++)
                        {
                            if (NotifyDays.Contains(i == 7 ? DayOfWeek.Sunday : (DayOfWeek)i))
                            {
                                SpecificDaysCount++;
                            }

                            if (i == 7)
                                i -= 7;
                        }

                        if (NotifyDays.Contains((DayOfWeek)TodayDay))
                        {
                            SpecificDaysCount++;
                        }
                    }

                    break;
            }

            return SpecificDaysCount;
        }
    }
}

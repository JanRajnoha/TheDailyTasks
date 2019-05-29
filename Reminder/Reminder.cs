using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaskManager;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Storage;
using Windows.UI.Notifications;
using Extensions;

namespace Reminder
{
    public sealed class Reminder : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();

            #region Nevim co to je, ale asi je to důležité
            // Taskinstance.Canceled += Taskinstance_Canceled;

            /* XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastImageAndText04);

             XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
             for (int i = 0; i < stringElements.Length; i++)
             {
                 stringElements[i].AppendChild(toastXml.CreateTextNode("Line " + i));
             }

             // Specify the absolute path to an image
             XmlNodeList imageElements = toastXml.GetElementsByTagName("image");


             var toast = new ToastNotification(toastXml);
             ToastNotificationManager.CreateToastNotifier(CurrentApp.AppId.ToString()).Show(toast);

             await ShowToastAsync("Hello from background");*/
            #endregion

            try
            {
                await TDTSharedCode.BackgroundTask(); 

            }
            catch (Exception e)
            { }

            defferal.Complete();
        }

        //private void SendLiveTiles(ObservableCollection<Tasks> TaskItems, ObservableCollection<Tasks> Queue)
        //{
        //    string[] UncompletedTasks = GetUncompletedTaskInfo(TaskItems);

        //    if (UncompletedTasks != null)
        //    {
        //        LiveTiles.UpdateBadge(UncompletedTasks.Length.ToString());
        //    }
        //    else
        //        TileUpdateManager.CreateTileUpdaterForApplication().Clear();

        //    TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);

        //    if (UncompletedTasks != null && Queue.Count == 0)
        //    {
        //        string HeadTile = $@"<tile>
        //                                <visual>
        //                                   <binding template='TileWideText01'>
        //                                      <text id='1'>Uncompleted tasks</text>
        //                                      {GetLiveTilesRows(UncompletedTasks, 0, 2, 3)}
        //                                   </binding>
        //                                   <binding template='TileSquareText01'>
        //                                      <text id='1'>Uncompleted</text>
        //                                      {GetLiveTilesRows(UncompletedTasks, 0, 2, 3)}
        //                                   </binding>
        //                                   <binding template='TileSquare310x310Text01'>
        //                                      <text id='1'>Uncompleted tasks</text>
        //                                      {GetLiveTilesRows(UncompletedTasks, 0, 2, 9)}
        //                                   </binding>
        //                                </visual>
        //                             </tile>";

        //        LiveTiles.LiveTileUpdater(HeadTile);

        //        GetTiles(UncompletedTasks);
        //    //    GetLargeTiles(UncompletedTasks);
        //    }
        //    else
        //        foreach (var Item in Queue)
        //        {
        //            string tile = $@"<tile>
        //                                <visual>
        //                                   <binding template='TileWideText09'>
        //                                      <text id='1'>{Item.Name}</text>
        //                                      <text id='2'>{Item.Description}</text>
        //                                   </binding>
        //                                   <binding template='TileSquareText02'>
        //                                      <text id='1'>{Item.Name}</text>
        //                                      <text id='2'>{Item.Description}</text>
        //                                   </binding>
        //                                   <binding template='TileSquare310x310TextList03'>
        //                                      <text id='1'>{Item.Name}</text>
        //                                      <text id='2'>{Item.Description}</text>
        //                                      <text id='3'>Days complete</text>
        //                                      <text id='4'>{Item.Dates.Count.ToString() + " of " + ((Item.End - Item.Start).TotalDays + 1).ToString()}</text>
        //                                      <text id='5'>Remaining days</text>
        //                                      <text id='6'>{(Item.End - DateTime.Today).Days + " days"}</text>
        //                                   </binding>
        //                                </visual>
        //                             </tile>";

        //            LiveTiles.LiveTileUpdater(tile);
        //        }
        //}

        //private void GetTiles(string[] UncompletedTasks)
        //{
        //    int Tiles = 2;
        //    int RowsMediumWide = 3;
        //    int RowsLarge = 9;
        //    string LiveTile = "";

        //    while (Tiles <= 5)
        //    {
        //        string MediumWideTileContent = GetLiveTilesRows(UncompletedTasks, RowsMediumWide, MaxRows: 4);

        //        LiveTile = $@"<tile>
        //                         <visual>
        //                            <binding template='TileWideText05'>
        //                               {MediumWideTileContent}
        //                            </binding>
        //                            <binding template='TileSquareText03'>
        //                               {MediumWideTileContent}
        //                            </binding>";

        //        string LargeTileContent = GetLiveTilesRows(UncompletedTasks, RowsLarge, MaxRows: 11);

        //        if (LargeTileContent.Length != 0)
        //            LiveTile += $@" <binding template = 'TileSquare310x310Text03'>
        //                               {LargeTileContent}
        //                            </binding>";


        //        LiveTile += @"   </visual>
        //                      </tile> ";

        //        LiveTiles.LiveTileUpdater(LiveTile);

        //        RowsMediumWide += 4;
        //        RowsLarge += 11;
        //        Tiles++;
        //    }
        //}

        //private string GetLiveTilesRows(string[] UncompletedTasks, int Rows, int StartedRows = 1, int MaxRows = 0)
        //{
        //    string LiveTileRows = "";
        //    int IdRow = StartedRows;

        //    while (Rows <= UncompletedTasks.Length - 1 && (( MaxRows != 0 && IdRow <= MaxRows + StartedRows - 1) || MaxRows == 0))
        //    {
        //        LiveTileRows += $@"<text id='{IdRow.ToString()}'>{UncompletedTasks[Rows]}</text>";
        //        IdRow++;
        //        Rows++;

        //    }

        //    return LiveTileRows;
        //}

        //private string[] GetUncompletedTaskInfo(ObservableCollection<Tasks> TaskItems)
        //{
        //    int Number = 0;
        //    string[] UncompletedTasks;

        //    if ((TaskItems.Count != 0) || (TaskItems != null))
        //    {
        //        UncompletedTasks = new string[TaskItems.Count(Uncompleted => Uncompleted.Dates.Contains(DateTime.Today) != true && Uncompleted.Start <= DateTime.Today && Uncompleted.End >= DateTime.Today && Uncompleted.NotifyDays.Contains(DateTime.Today.DayOfWeek))];
        //    }
        //    else
        //        return null;

        //    int i = 0;

        //    foreach (var Task in TaskItems)
        //    {
        //        if ((!Task.Dates.Contains(DateTime.Today)) && ((Task.End >= DateTime.Today) && (Task.Start <= DateTime.Today)) && Task.NotifyDays.Contains(DateTime.Today.DayOfWeek))
        //        {
        //            UncompletedTasks[i] = Task.Name;
        //            i++;
        //        }
        //    }

        //    return UncompletedTasks;
        //}

        //private ObservableCollection<Tasks> GetTasks(Stream stream, int Attempts)
        //{
        //    ObservableCollection<Tasks> _Tasks = new ObservableCollection<Tasks>();
        //    try
        //    {
        //        XmlSerializer pok = new XmlSerializer(typeof(ObservableCollection<Tasks>));

        //        using (Stream Steam = stream)
        //        {
        //            _Tasks = (ObservableCollection<Tasks>)pok.Deserialize(Steam);
        //        }

        //        if (_Tasks == null)
        //            return new ObservableCollection<Tasks>();
        //        else
        //            return _Tasks;
        //    }
        //    catch (Exception s) when ((s.Message.Contains("denied")) && (Attempts < 10))
        //    {
        //        return GetTasks(stream, Attempts + 1);
        //    }

        //    catch (Exception s)
        //    {
        //        return new ObservableCollection<Tasks>();
        //    }
        //}
    }
}


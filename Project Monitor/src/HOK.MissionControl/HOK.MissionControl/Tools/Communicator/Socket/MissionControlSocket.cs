using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autodesk.Revit.DB;
using GalaSoft.MvvmLight.Messaging;
using Newtonsoft.Json.Linq;
using Quobject.Collections.Immutable;
using Quobject.EngineIoClientDotNet.Client.Transports;
using Quobject.SocketIoClientDotNet.Client;
using HOK.Core.Utilities;
using HOK.MissionControl.Core.Schemas.Families;
using HOK.MissionControl.Core.Schemas.Sheets;
using HOK.MissionControl.Core.Utils;
using HOK.MissionControl.Tools.Communicator.Messaging;

namespace HOK.MissionControl.Tools.Communicator.Socket
{
    public class MissionControlSocket
    {
        public const string BaseUrlLocal = "http://hok-184vs/";
        //public const string BaseUrlLocal = "http://localhost:8080";

        public void Main(Document doc)
        {
            var centralPath = BasicFileInfo.Extract(doc.PathName).CentralPath;

            try
            {
                var options = new IO.Options
                {
                    IgnoreServerCertificateValidation = true,
                    AutoConnect = true,
                    ForceNew = true,
                    Upgrade = false
                };

                options.Transports = ImmutableList.Create<string>(WebSocket.NAME);

                var socket = IO.Socket(BaseUrlLocal, options);

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Connected to Mission Control socket.");
                });

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_DISCONNECT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Disconnected from Mission Control socket.");
                });

                socket.On(Quobject.SocketIoClientDotNet.Client.Socket.EVENT_CONNECT_TIMEOUT, () =>
                {
                    Log.AppendLog(LogMessageType.INFO, "Mission Control socket connection timed out.");
                });

                #region Family Tasks

                socket.On("task_deleted", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var deletedIds = data["deletedIds"].ToObject<List<string>>();
                        var familyName = data["familyName"].ToObject<string>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (!deletedIds.Any() || string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(collId)) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!IdsMatch(centralPath, collId, CollectionType.Families)) return;

                        Messenger.Default.Send(new FamilyTaskDeletedMessage { FamilyName = familyName, DeletedIds = deletedIds });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "task_deleted: Data was null!");
                    }
                });

                socket.On("familyTask_added", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var familyName = data["familyName"].ToObject<string>();
                        var task = data["task"].ToObject<FamilyTask>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (string.IsNullOrEmpty(familyName) || task == null || string.IsNullOrEmpty(collId)) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.CurrentCultureIgnoreCase) ||
                            !IdsMatch(centralPath, collId, CollectionType.Families)) return;

                        Messenger.Default.Send(new FamilyTaskAddedMessage { FamilyName = familyName, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "familyTask_added: Data was null!");
                    }
                });

                socket.On("familyTask_updated", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var familyName = data["familyName"].ToObject<string>();
                        var collId = data["collectionId"].ToObject<string>();
                        var task = data["task"].ToObject<FamilyTask>();
                        if (task == null || string.IsNullOrEmpty(familyName) || string.IsNullOrEmpty(collId)) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!IdsMatch(centralPath, collId, CollectionType.Families)) return;

                        Messenger.Default.Send(new FamilyTaskUpdatedMessage { FamilyName = familyName, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "familyTask_updated: Data was null!");
                    }
                });

                #endregion

                #region Sheet Tasks

                socket.On("sheetTask_added", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var sheetId = data["sheetId"].ToObject<string>();
                        var taskId = data["taskId"].ToObject<string>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(collId)) return;

                        var sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(x.Id, sheetId, StringComparison.Ordinal));
                        var task = sheet?.tasks.FirstOrDefault(x => string.Equals(x.Id, taskId, StringComparison.Ordinal));

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (task == null
                            || !string.Equals(task.assignedTo.ToLower(), Environment.UserName.ToLower(), StringComparison.Ordinal)
                            || !IdsMatch(centralPath, collId, CollectionType.Sheets)) return;

                        Messenger.Default.Send(new SheetsTaskAddedMessage { Sheet = sheet, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_added: Data was null!");
                    }
                });

                socket.On("sheetTask_updated", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var sheetId = data["sheetId"].ToObject<string>();
                        var taskId = data["taskId"].ToObject<string>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(taskId) || string.IsNullOrEmpty(collId)) return;

                        var sheet = sheetsData.sheets.FirstOrDefault(x => string.Equals(sheetId, x.Id, StringComparison.Ordinal));
                        var task = sheet?.tasks.FirstOrDefault(x => string.Equals(x.Id, taskId, StringComparison.Ordinal));

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // Here we will only filter out tasks that don't belong to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        // Tasks that are assigned to someone else will be dealt with in the handler.
                        if (task == null || !IdsMatch(centralPath, collId, CollectionType.Sheets)) return;

                        Messenger.Default.Send(new SheetsTaskUpdatedMessage { Sheet = sheet, Task = task });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_updated: Data was null!");
                    }
                });

                socket.On("sheetTask_deleted", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetId = data["sheetId"].ToObject<string>();
                        var deletedIds = data["deletedIds"].ToObject<List<string>>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (string.IsNullOrEmpty(sheetId) || !deletedIds.Any() || string.IsNullOrEmpty(collId)) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!IdsMatch(centralPath, collId, CollectionType.Sheets)) return;

                        Messenger.Default.Send(new SheetsTaskDeletedMessage { SheetId = sheetId, DeletedIds = deletedIds });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_deleted: Data was null!");
                    }
                });

                socket.On("sheetTask_sheetsAdded", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetsData = data["body"].ToObject<SheetData>();
                        var sheetsIds = data["sheetIds"].ToObject<List<string>>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (sheetsData == null || string.IsNullOrEmpty(collId) || !sheetsIds.Any()) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!IdsMatch(centralPath, collId, CollectionType.Sheets)) return;

                        Messenger.Default.Send(new SheetTaskSheetsCreatedMessage { Sheets = sheetsData.sheets.Where(x => sheetsIds.Contains(x.Id)).ToList() });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetsAdded: Data was null!");
                    }
                });

                socket.On("sheetTask_sheetDeleted", body =>
                {
                    if (body != null)
                    {
                        var data = JObject.FromObject(body);
                        var sheetId = data["sheetId"].ToObject<string>();
                        var deletedIds = data["deletedIds"].ToObject<List<string>>();
                        var collId = data["collectionId"].ToObject<string>();
                        if (string.IsNullOrEmpty(sheetId) || string.IsNullOrEmpty(collId) || !deletedIds.Any()) return;

                        // (Konrad) We want to filter out tasks that don't belong to us or to this model.
                        // We can use "collectionId" to identify the model since that is tied to centralPath.
                        if (!IdsMatch(centralPath, collId, CollectionType.Sheets)) return;

                        Messenger.Default.Send(new SheetTaskSheetDeletedMessage { SheetId = sheetId, DeletedIds = deletedIds });
                    }
                    else
                    {
                        Log.AppendLog(LogMessageType.ERROR, "sheetTask_sheetDeleted: Data was null!");
                    }
                });

                #endregion

                while (true)
                {
                    Thread.Sleep(100);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// Compares collection id of the model that was used in interaction with MongoDB
        /// to current model that we are in.
        /// </summary>
        /// <param name="centralPath">Central Path to current model.</param>
        /// <param name="eventModelId">Collection Id of the model used by the event.</param>
        /// <param name="type">Type of collection to check against.</param>
        /// <returns>True if models match otherwise False.</returns>
        private static bool IdsMatch(string centralPath, string eventModelId, CollectionType type)
        {
            string currentModelId;
            switch (type)
            {
                case CollectionType.Sheets:
                    currentModelId = MissionControlSetup.SheetsIds.ContainsKey(centralPath) ? MissionControlSetup.SheetsIds[centralPath] : string.Empty;
                    break;
                case CollectionType.Families:
                    currentModelId = MissionControlSetup.FamiliesIds.ContainsKey(centralPath) ? MissionControlSetup.FamiliesIds[centralPath] : string.Empty;
                    break;
                default:
                    return false;
            }
            
            return !string.IsNullOrEmpty(currentModelId) && string.Equals(currentModelId.ToLower(), eventModelId.ToLower(), StringComparison.Ordinal);
        }
    }

    public enum CollectionType
    {
        Sheets,
        Families
    }
}

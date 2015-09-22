using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Tasks.Offline;
using System.Linq;
using Esri.ArcGISRuntime.Tasks;

namespace WpfApplication1
{
    internal class SyncHelper
    {



        public async Task<GeodatabaseStatusInfo> SyncGeodatabase(string url, Geodatabase gdb, bool unregister, bool syncing)
        {
            Console.WriteLine("Starting Sync " + DateTime.Now + " --> " + syncing);
            var syncTask = new GeodatabaseSyncTask(new Uri(url));


            var layerIdsAndName = new Dictionary<int, string>();

            FeatureServiceInfo serviceInfo = await syncTask.GetServiceInfoAsync();
            foreach (LayerServiceInfo layerServiceInfo in serviceInfo.Layers)
            {
                if (!layerIdsAndName.ContainsKey(layerServiceInfo.ID))
                {
                    layerIdsAndName.Add(layerServiceInfo.ID, layerServiceInfo.Name);
                }
            }


            


            var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();
            Action<GeodatabaseStatusInfo, Exception> completionAction = (info, ex) =>
            {
                if (ex != null)
                {
                    tcs.SetException(ex);
                    return;
                }
                tcs.SetResult(info);
            };

            var syncProgress = new Progress<GeodatabaseStatusInfo>();
            syncProgress.ProgressChanged += (sndr, sts) => Console.WriteLine("Status:" + sts.Status.ToString() + "," +" --> " + syncing + "," + sts.SubmissionTime + "," + sts.ResultUri);


            IDictionary<int, LayerSyncInfo> layerSync = null;

            layerSync = new Dictionary<int, LayerSyncInfo>();
            foreach (var id_name in layerIdsAndName)
            {
                    var layerQuery = new LayerSyncInfo { SyncDirection = SyncDirection.Bidirectional };
                    layerSync.Add(id_name.Key, layerQuery);
                
            }
            Action<UploadResult> uploadAction = result => { Console.Write(""); };

            //should we remove sync direction since it should be defined in layers?
            var syncParams = new SyncGeodatabaseParameters(gdb) { RollbackOnFailure = false, UnregisterGeodatabase = unregister, LayerSyncInfos = layerSync, SyncDirection = SyncDirection.Bidirectional };
            Console.WriteLine("Calling sync " + DateTime.Now);
            await syncTask.SyncGeodatabaseAsync(syncParams, gdb,
                completionAction,
                uploadAction,
                TimeSpan.FromSeconds(3),
                syncProgress,
                CancellationToken.None);
            
            

            return await tcs.Task;

            

        }
       
        
    }
}
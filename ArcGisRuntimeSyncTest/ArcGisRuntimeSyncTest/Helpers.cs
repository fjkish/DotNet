// *************************
// Solution: WpfApplication1
// Project: WpfApplication1
// File: Help.cs
// Year: 2014
// Vendor: ESRI
// *************************

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace WpfApplication1
{
    public static class Helpers
    {
        private static CancellationTokenSource _syncCancellationTokenSource;

        public static async Task<List<int>> GetLayersIdList(string url)
        {
            var geodatabaseTask = new GeodatabaseSyncTask(new Uri(url));
            FeatureServiceInfo fsInfo = await geodatabaseTask.GetServiceInfoAsync();
            IReadOnlyList<LayerServiceInfo> layerServiceInfos = fsInfo.Layers;
            return layerServiceInfos.Select(lsi => lsi.ID).ToList();
        }


        public static async void CreateReplicaExtent(Geometry thisExtent, string url, string gdbName, string geodatabasePath, string gdbExt, IProgress<string> prog)
        {
            List<int> layerNumList = await GetLayersIdList(url);
            Console.WriteLine(url);
            Console.WriteLine(layerNumList);
            Console.WriteLine(thisExtent);
            Console.WriteLine(gdbName);
            Console.WriteLine(geodatabasePath);
            Console.WriteLine(gdbExt);

            await CreateReplica(url, layerNumList, thisExtent, gdbName, geodatabasePath, gdbExt, prog);
            Console.WriteLine("Done CreateReplicaExtent");
        }

        public static async void CreateReplicaDataBufferExtent(string url, string gdbName, string geodatabasePath, string gdbExt, IProgress<string> prog)
        {
            var serviceUrl = new Uri(url);
            var geodatabaseTask = new GeodatabaseSyncTask(serviceUrl);
            FeatureServiceInfo serviceInfo = await geodatabaseTask.GetServiceInfoAsync();
            List<int> layerNumList = await GetLayersIdList(url);
            Geometry smallEnvelope = GeometryEngine.Buffer(serviceInfo.FullExtent.GetCenter(), 1000);
            await CreateReplica(url, layerNumList, smallEnvelope, gdbName, geodatabasePath, gdbExt, prog);
            Console.WriteLine("Done CreateReplicaExtent");

        }





        //await CreateReplica(url, layerNumList, thisExtent, gdbName, geodatabasePath, gdbExt,prog);
        private static async Task CreateReplica(string featureServiceUrl, IEnumerable<int> layerNumList, Geometry geometry, string gdbNameNoExt, string geodatabasePath, string gdbExt, IProgress<string> prog)
        {


            try
            {
                DateTime begin = DateTime.UtcNow;

                var generationProgress = new Progress<GeodatabaseStatusInfo>();
                Int64 i = 0;
                generationProgress.ProgressChanged += (sender, s) =>
                {
                    
                    i++;
                };

                //setup parameters
                var geodatabaseSyncTask = new GeodatabaseSyncTask(new Uri(featureServiceUrl));
                FeatureServiceInfo serviceInfo = await geodatabaseSyncTask.GetServiceInfoAsync();

                var parameters = new GenerateGeodatabaseParameters(layerNumList, geometry)
                {
                    GeodatabasePrefixName = gdbNameNoExt,
                    OutSpatialReference = SpatialReferences.WebMercator,

                };

                if (serviceInfo.SyncEnabled)
                {
                    parameters.SyncModel = serviceInfo.SyncCapabilities.SupportsPerLayerSync ? SyncModel.PerLayer : SyncModel.PerGeodatabase;
                }


                //call extension method
                GeodatabaseStatusInfo resultInfo =
                    await geodatabaseSyncTask.ExGenerateGeodatabaseAsync(parameters, new TimeSpan(0, 0, 2), generationProgress);

                // Download geodatabase only if generation was completed without errors. Other statuses that might be checked and handled are
                // GeodatabaseSyncStatus.Failed and GeodatabaseSyncStatus.CompletedWithErrors.
                if (resultInfo.Status != GeodatabaseSyncStatus.Completed)
                {
                    Logger.Report(string.Format("Geodatabase: Generating geodatabase failed. Status = {0}.", resultInfo.Status), prog);
                    return;
                }

                //Download database ... with no buffer

                Logger.Report("Geodatabase: Replica created, starting download.", prog);
                var client = new ArcGISHttpClient();
                HttpResponseMessage gdbStream = await client.GetAsync(resultInfo.ResultUri, HttpCompletionOption.ResponseHeadersRead);


                using (FileStream stream = File.Create(geodatabasePath + "\\" + gdbNameNoExt + gdbExt))
                {
                    await gdbStream.Content.CopyToAsync(stream);
                    await stream.FlushAsync();
                }
                DateTime end = DateTime.UtcNow;
                Logger.Report("Measured time: " + (end - begin).TotalMilliseconds + " ms.", prog);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.ToString());
                Console.WriteLine("CreateReplica Exception" + Environment.NewLine + ex.Message);
            }
        }





     
    }
}

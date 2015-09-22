using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Offline;
using WpfApplication1;

namespace ArcGisRuntimeSyncTest
{
    public partial class MainWindow : Window
    {
        private readonly string _url = "http://localhost:6080/arcgis/rest/services/MyMapService2/FeatureServer";
        private readonly string GDB_PATH = @"D:\geodatabases\New folder\main.db";
        
        private bool _syncing;
        private Geodatabase _gdb;

        public MainWindow()
        {
            InitializeComponent();
            _syncing = false;

         
        }

     

        private void MyMapView_LayerLoaded(object sender, LayerLoadedEventArgs e)
        {
            if (e.LoadError == null)
                return;

            Debug.WriteLine("Error while loading layer : {0} - {1}", e.Layer.ID, e.LoadError.Message);
        }

        private async void GetGdb(object sender, RoutedEventArgs e)
        {
            //Helpers.CreateReplicaExtent(MyMapView.Extent, _url, _gdbName, GdbDownloadFolder, GdbExt, _progress);
            await GenerateGeodatabase();
            Console.WriteLine("GetGdb" + "\n" + "~~58~~");
        }

        private async void AddLocalToMap(object sender, RoutedEventArgs e)
        {
            try {

                _gdb = await Geodatabase.OpenAsync(GDB_PATH);

                foreach (var table in _gdb.FeatureTables)
                {
                    try {
                        var flayer = new FeatureLayer {
                            ID = table.Name,
                            DisplayName = table.Name,
                            FeatureTable = table
                        };


                        MyMapView.Map.Layers.Add(flayer);
                        MyMapView.SetView(flayer.FullExtent);
                    }
                    catch (Exception ex) {
                        Console.WriteLine("Error creating feature layer: " + ex.Message, "Samples");
                    }
                }
            }
            catch (Exception ex) {
                Console.WriteLine("Error creating feature layer: " + ex.Message, "Samples");
            }
        }

        private async void Sync(object sender, RoutedEventArgs e)
        {
            await CallSync();
        }

        private async Task CallSync()
        {
            var syncHelper = new SyncHelper();
            if (_syncing) {
                Console.WriteLine("Still syncing");
                return;
            }
            try {
                _syncing = true;

                await syncHelper.SyncGeodatabase(_url, _gdb, false, _syncing);
            }
            catch (Exception ex) {
                Console.WriteLine("ex: " + ex, "Sync ~~ 80");
            }
            finally {
                _syncing = false;
                Console.WriteLine("CallSync finally");
            }

            
        }

        private async void EditFeature(object sender, RoutedEventArgs e)
        {
            var fl = MyMap.Layers[1] as FeatureLayer;
            var query = new QueryFilter {WhereClause = "ObjectID > 0"};
            var features = await fl.FeatureTable.QueryAsync(query);

            foreach (var feature in features) {
                feature.Attributes["txt"] = DateTime.Now.ToShortDateString();
                await fl.FeatureTable.UpdateAsync(feature);
                break;
            }
            await CallSync();
        }

        private async Task GenerateGeodatabase()
        {
            var syncTask = new GeodatabaseSyncTask(new Uri(_url));
            var serviceInfo = await syncTask.GetServiceInfoAsync();

            var syncModel = serviceInfo.SyncCapabilities.SupportsPerLayerSync
                ? SyncModel.PerLayer
                : SyncModel.PerGeodatabase;


            var layerNumList = await Helpers.GetLayersIdList(_url);

            var options = new GenerateGeodatabaseParameters(layerNumList, serviceInfo.FullExtent) {
                ReturnAttachments = false,
                OutSpatialReference = serviceInfo.SpatialReference,
                SyncModel = syncModel
            };

            var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();
            Action<GeodatabaseStatusInfo, Exception> completionAction = (statusInfo, ex) =>
            {
                try {
                    if (ex != null) {
                        tcs.SetException(ex);
                        return;
                    }
                    tcs.SetResult(statusInfo);
                }
                catch (Exception) {
                }
            };

            var generationProgress = new Progress<GeodatabaseStatusInfo>();

            var result = await syncTask.GenerateGeodatabaseAsync(options, completionAction,
                TimeSpan.FromSeconds(3), generationProgress, CancellationToken.None);


            var statusResult = await tcs.Task;

            if (statusResult.Status == GeodatabaseSyncStatus.Failed)
                throw new ApplicationException("Map Download Failed, try again later.");


            await DownloadGeodatabase(statusResult, GDB_PATH);
        }

        // Download a generated geodatabase file
        private async Task<string> DownloadGeodatabase(GeodatabaseStatusInfo statusResult, string gdbPath)
        {
            var client = new ArcGISHttpClient();
            var gdbStream = client.GetOrPostAsync(statusResult.ResultUri, null);
            var gdbFolder = Path.GetDirectoryName(gdbPath);

            if (!Directory.Exists(gdbFolder))
                Directory.CreateDirectory(gdbFolder);

            await Task.Factory.StartNew(async () =>
            {
                using (var stream = File.Create(gdbPath)) {
                    await gdbStream.Result.Content.CopyToAsync(stream);
                }
            });

            return gdbPath;
        }
    }
}
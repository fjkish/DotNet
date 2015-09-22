// *************************
// Solution: WpfApplication1
// Project: WpfApplication1
// File: GenerateGeodatabaseTaskExtensions.cs
// Year: 2014
// Vendor: ESRI
// *************************

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Tasks.Offline;

namespace WpfApplication1
{
    public static class GenerateGeodatabaseTaskExtensions
    {
        /// <summary>
        /// Extension method to generate new geodatabase from target service.
        /// </summary>
        /// <param name="task">Task used.</param>
        /// <param name="gdbParameters">Instance of <see cref="GenerateGeodatabaseParameters"/> that defines how geodatabase is created.</param>
        /// <param name="statusUpdateInterval">Interval when progress is checked from the server.</param>
        /// <param name="progress">Progress that is invoked every status update interval.</param>
        /// <param name="cancellationToken">Token for task cancellation.</param>
        /// <returns>Returns <see cref="GeodatabaseStatusInfo"/> when geonerating geodatabase is finished.</returns>
        /// <remarks>Use <see cref="GeodatabaseStatusInfo.ResultUri"/> for downloading the generated geodatabase.</remarks>
        public static async Task<GeodatabaseStatusInfo> ExGenerateGeodatabaseAsync(
            this GeodatabaseSyncTask task,
            GenerateGeodatabaseParameters gdbParameters,
            TimeSpan statusUpdateInterval,
            IProgress<GeodatabaseStatusInfo> progress,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();

            await task.GenerateGeodatabaseAsync(gdbParameters, (statusInfo, exception) =>
            {
                if (exception != null)
                    tcs.SetException(exception);

                tcs.SetResult(statusInfo);

            }, statusUpdateInterval, progress, cancellationToken);

            GeodatabaseStatusInfo result = await tcs.Task;
            return result;
        }

        /// <summary>
        /// Downloads and saved geodatabase from given Uri to the device.
        /// </summary>
        /// <param name="task">Task used.</param>
        /// <param name="uriToGeodatabase">Uri to geodabase that is downloaded.</param>
        /// <param name="locationForGeodatabase">Full file path, where geodatabase is downloaded.</param>
        /// <param name="geodatabaseName">Name for the geodatabase. This is the name that is used when it is saved to the device.</param>
        /// <returns>Returns after download is fully completed.</returns>
        /// <remarks>If target folder doesn't exists, it is created.</remarks>
        public static async Task DownloadGeodatabaseAsync(
            this GeodatabaseSyncTask task,
            Uri uriToGeodatabase,
            string locationForGeodatabase,
            string geodatabaseName)
        {

            var client = new ArcGISHttpClient();
            var gdbStream = client.GetOrPostAsync(uriToGeodatabase, null);

            var geodatabasePath = Path.Combine(locationForGeodatabase, geodatabaseName);

            if (!Directory.Exists(locationForGeodatabase))
            {
                Directory.CreateDirectory(locationForGeodatabase);
            }

            await Task.Factory.StartNew(async () =>
            {
                using (var stream = File.Create(geodatabasePath))
                {
                    await gdbStream.Result.Content.CopyToAsync(stream);
                }
            });

        }
    }
}
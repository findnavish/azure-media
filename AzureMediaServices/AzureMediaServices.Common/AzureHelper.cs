using AzureMediaServices.Common.Entities;
using Microsoft.WindowsAzure.MediaServices.Client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AzureMediaServices.Common
{
    /// <summary>
    /// This class is a wrapper to connect Azure and consume Media Service SDK's for out funtionality
    /// The following Manage Nuget Package has been used for this purpose in the project: windowsazure.mediaservices.extensions
    /// </summary>
    public sealed class AzureHelper
    {
        #region Private Variables
        private static volatile AzureHelper azureInstance;
        private static object lockObject = new Object();
        private CloudMediaContext context;
        private MediaServicesCredentials cachedCredentials;
        private CloudBlobClient blobClient;
        private CloudStorageAccount storageAccount;
        private StorageCredentials storageCredentials;

        private string encodingJobNamePrefix = "EncodingJob_";
        private string encodingTaskNamePrefix = "EncodingTask_";
        private string streamAssetNamePrefix = "SA_"; // Streaming Asset (SA)
        private string thumbnailJobNamePrefix = "ThumbnailJob_";
        private string thumbnailTaskNamePrefix = "ThumbnailTask_";
        #endregion

        #region Private Contructor
        private AzureHelper()
        {
            cachedCredentials = new MediaServicesCredentials(ConfigurationManager.AppSettings["azureMediaServiceAccountName"], ConfigurationManager.AppSettings["azureMediaServiceAccountKey"], ConfigurationManager.AppSettings["azureMediaServiceUrn"], ConfigurationManager.AppSettings["acsBaseAddress"]);
            context = new CloudMediaContext(new Uri(ConfigurationManager.AppSettings["govCloudMediaApiServer"], UriKind.Absolute), cachedCredentials);
            storageCredentials = new StorageCredentials(ConfigurationManager.AppSettings["storageAccountName"], ConfigurationManager.AppSettings["storageAccountKey"]);
            storageAccount = new CloudStorageAccount(storageCredentials, ConfigurationManager.AppSettings["govCloudEndPointSuffix"], false);
            blobClient = storageAccount.CreateCloudBlobClient();
        }
        #endregion

        #region Pubic Class Instance
        public static AzureHelper AzureInstance
        {
            get
            {
                if (azureInstance == null)
                {
                    /// Thread safe. Using a lockObject instance to lock on, rather than locking on the type itself to avoid deadlocks
                    lock (lockObject)
                    {
                        if (azureInstance == null)
                        {
                            azureInstance = new AzureHelper();
                        }
                    }
                }

                return azureInstance;
            }
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// This method will upload the video to azure storage account and associate with azure media service instance
        /// </summary>
        /// <param name="fileName">Name of the video file</param>
        /// <param name="videoBytes">Video bytes</param>
        /// <returns>Azure asset id to track the uploaded asset</returns>
        public string Upload(string fileName, byte[] videoBytes)
        {
            IAsset asset = azureInstance.context.Assets.Create(fileName, AssetCreationOptions.None);
            ILocator destination = AssignSasLocator(asset);
            CloudBlobContainer destContainer = blobClient.GetContainerReference(new Uri(destination.Path).Segments[1]);
            CloudBlockBlob destBlob = destContainer.GetBlockBlobReference(fileName);
            using (MemoryStream ms = new MemoryStream(videoBytes))
            {
                destBlob.UploadFromStream(ms);
            }

            destBlob.SetProperties();
            IAssetFile assetFile = asset.AssetFiles.Create(fileName);
            assetFile.Update();
            destination.Delete();

            return asset.Id;
        }

        /// <summary>
        /// This method will queue video encoding job in azure.
        /// </summary>
        /// <param name="assetId">Video Asset id to identify the video asset in azure</param>
        public void Encode(string assetId)
        {
            IAsset assetToEncode = GetAssetById(assetId);
            if (assetToEncode == null)
            {
                throw new ArgumentException("Error: Could not find asset with Id: " + assetId);
            }

            IJob jobEncode = azureInstance.context.Jobs.Create(encodingJobNamePrefix + assetId);
            IMediaProcessor latestWameMediaProcessor = (from p in azureInstance.context.MediaProcessors where p.Name == "Media Encoder Standard" select p).ToList().OrderBy(wame => new Version(wame.Version)).LastOrDefault();
            ITask encodeTask = jobEncode.Tasks.AddNew(encodingTaskNamePrefix + assetId, latestWameMediaProcessor, "H264 Multiple Bitrate 1080p", TaskOptions.None);
            encodeTask.InputAssets.Add(assetToEncode);
            string encodedAssetName = string.Concat(streamAssetNamePrefix,assetToEncode.Name);
            encodeTask.OutputAssets.AddNew(encodedAssetName, AssetCreationOptions.None);
            jobEncode.Submit(); // This will just kick of the video encoding job
        }

        public VideoState TrackEncodeProgress(string assetId)
        {
            ITask encodingTask = GetEncodeTask(assetId);
            VideoState state = new VideoState();
            if (encodingTask.State == JobState.Error)
            {
                state.State = encodingTask.State.ToString();
                state.Message = encodingTask.ErrorDetails[0].Message;
            }
            else
            {
                state.State = encodingTask.State.ToString();
            }

            return state;
        }

        public VideoState GetStreamUrl(string assetId)
        {
            VideoState state = TrackEncodeProgress(assetId);

            if (state.State.Equals("Finished", StringComparison.InvariantCultureIgnoreCase))
            {
                ITask encodingTask = GetEncodeTask(assetId);
                IAsset encodedAsset = encodingTask.OutputAssets.FirstOrDefault();
                string streamUrl = PublishOnDemand(encodedAsset);
                IAsset originalVideo = GetAssetById(assetId);
                originalVideo.DeleteAsync(false);
                state.StreamUrl = streamUrl.Replace("http:", "https:");
            }

            return state;
        }
        
        #endregion

        #region Private Methods
        private ILocator AssignSasLocator(IAsset asset)
        {
            IAccessPolicy myPolicy = azureInstance.context.AccessPolicies.Create("READPOLICYSAS", TimeSpan.FromDays(365 * 20), AccessPermissions.Read);
            ILocator destination = azureInstance.context.Locators.CreateSasLocator(asset, myPolicy);
            return destination;
        }

        private ILocator CreateOnDemandLocator(IAsset asset)
        {
            var readPolicy = azureInstance.context.AccessPolicies.Create("READPOLICYONDEMAND", TimeSpan.FromDays(365 * 20), AccessPermissions.Read);
            var readLocator = azureInstance.context.Locators.CreateLocator(LocatorType.OnDemandOrigin, asset, readPolicy);
            return readLocator;
        }

        /// <summary>
        /// To find an assed instance in azure by providing the asset id
        /// </summary>
        /// <param name="assetId">Assed id of the video in azure</param>
        /// <returns>The asset instance</returns>
        private IAsset GetAssetById(string assetId)
        {
            IAsset asset;
            var assetInstance =
                from a in azureInstance.context.Assets
                where a.Id == assetId
                select a;
            asset = assetInstance.FirstOrDefault();
            return asset;
        }

        private ITask GetEncodeTask(string assetId)
        {
            string encodingJobName = encodingJobNamePrefix + assetId;
            string encodingTaskName = encodingTaskNamePrefix + assetId;
            IJob jobEncode = azureInstance.context.Jobs.Where(j => j.Name == encodingJobName).FirstOrDefault();
            jobEncode.GetExecutionProgressTask(CancellationToken.None); //.Wait();  Commenting wait, for the thread to not wait for encoding to complete
            ITask encodingTask = jobEncode.Tasks.Where(t => t.Name == encodingTaskName).FirstOrDefault();
            return encodingTask;
        }

        private string PublishOnDemand(IAsset asset)
        {
            string smoothStreamingUri = string.Empty;
            ILocator locatorOnDemand;
            if (asset.Locators.Count == 0)
            {
                locatorOnDemand = CreateOnDemandLocator(asset);                
            }
            else
            {
                locatorOnDemand = asset.Locators.Where(l => l.Type.Equals(LocatorType.OnDemandOrigin)).FirstOrDefault();
            }
            var manifestFile = asset.AssetFiles.Where(f => f.Name.ToLower().EndsWith(".ism")).FirstOrDefault();
            smoothStreamingUri = locatorOnDemand.Path + manifestFile.Name + "/manifest";
            return smoothStreamingUri;
        }
        #endregion

    }
}

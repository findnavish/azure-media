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
using System.Threading.Tasks;

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
        #endregion

        #region Private Contructor
        private AzureHelper() { }
        #endregion

        #region Private Properties
        private CloudStorageAccount StorageAccount
        {
            get
            {
                if (azureInstance.storageAccount == null)
                {
                    azureInstance.storageAccount = new CloudStorageAccount(
                        StorageCredentials,
                        ConfigurationManager.AppSettings["govCloudEndPointSuffix"],
                        false);
                }

                return azureInstance.storageAccount;
            }
        }

        private StorageCredentials StorageCredentials
        {
            get
            {
                if (azureInstance.storageCredentials == null)
                {
                    azureInstance.storageCredentials = new StorageCredentials(
                        ConfigurationManager.AppSettings["storageAccountName"],
                        ConfigurationManager.AppSettings["storageAccountKey"]);
                }

                return azureInstance.storageCredentials;
            }
        }

        private CloudBlobClient BlobClient
        {
            get
            {
                if (azureInstance.blobClient == null)
                {
                    azureInstance.blobClient = azureInstance.StorageAccount.CreateCloudBlobClient();
                }

                return azureInstance.blobClient;
            }
        }

        private MediaServicesCredentials CachedCredentials
        {
            get
            {
                if (azureInstance.cachedCredentials == null)
                {
                    azureInstance.cachedCredentials = new MediaServicesCredentials(ConfigurationManager.AppSettings["azureMediaServiceAccountName"], ConfigurationManager.AppSettings["azureMediaServiceAccountKey"], ConfigurationManager.AppSettings["azureMediaServiceUrn"], ConfigurationManager.AppSettings["acsBaseAddress"]);
                }

                return azureInstance.cachedCredentials;
            }
        }


        private CloudMediaContext Context
        {
            get
            {
                if (azureInstance.context == null)
                {
                    azureInstance.context = new CloudMediaContext(new Uri(ConfigurationManager.AppSettings["govCloudMediaApiServer"], UriKind.Absolute), azureInstance.CachedCredentials);
                }

                return azureInstance.context;
            }
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

        #endregion

        #region Private Methods
        private ILocator AssignSasLocator(IAsset asset)
        {
            IAccessPolicy myPolicy = azureInstance.context.AccessPolicies.Create("READPOLICYSAS", TimeSpan.FromDays(365 * 20), AccessPermissions.Read);
            ILocator destination = azureInstance.context.Locators.CreateSasLocator(asset, myPolicy);
            return destination;
        }
        #endregion

    }
}

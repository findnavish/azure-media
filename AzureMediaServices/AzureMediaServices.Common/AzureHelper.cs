using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        private static volatile AzureHelper azureInstance;
        private static object lockObject = new Object();
        private static CloudMediaContext context ;

        private AzureHelper() { }

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

                            if (context == null)
                            {
                                
                                //context = new CloudMediaContext()
                                    
                            }



                        }

                       // string help = ;
                    }
                }

                return azureInstance;
            }
        }

        public string TestMethod()
        {           
            return "Constructor Called";
        }

    }
}

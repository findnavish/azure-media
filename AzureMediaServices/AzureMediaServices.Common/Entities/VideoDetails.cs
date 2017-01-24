using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AzureMediaServices.Common.Entities
{
    [DataContract]
    public class VideoDetails
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Keywords { get; set; }

        [DataMember]
        public bool IsFeatured
        {
            get;
            set;
        }

        [DataMember]
        public byte[] VideoBytes { get; set; }


        [DataMember]
        public string OriginalAssetId
        {
            get;
            set;
        }


        [DataMember]
        public string StreamAssetId
        {
            get;
            set;
        }


        [DataMember]
        public string ThumbnailFileLink
        {
            get;
            set;
        }


        [DataMember]
        public string StreamingLink
        {
            get;
            set;
        }


        [DataMember]
        public byte[] ThumbnailImageBytes
        {
            get;
            set;
        }







        [DataMember]
        public string ThumbnailImageName
        {
            get;
            set;
        }


        [DataMember]
        public string OriginalVideoFileName
        {
            get;
            set;
        }

        [DataMember]
        public bool HasThumbnail { get; set; }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AzureMediaServices.Common.Entities
{
    [DataContract]
    public class VideoState
    {
        [DataMember]
        public string State { get; set; }

        [DataMember]
        public double Progress { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string StreamUrl { get; set; }

    }
}

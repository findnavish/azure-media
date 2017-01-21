using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AzureMediaServices.WebApi.Controllers
{
    public class MediaController : ApiController
    {
        [HttpGet]
        [ActionName("Test")]
        public string Test()
        {         
            return "This is a test controller to test WEB API";
        }
    }
}

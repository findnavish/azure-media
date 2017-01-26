using AzureMediaServices.Common;
using AzureMediaServices.Common.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AzureMediaServices.WebApi.Controllers
{
    public class MediaController : ApiController
    {
        [HttpGet]
        [ActionName("Test")]
        public HttpResponseMessage Test()
        {
            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [HttpPost]
        [ActionName("PostVideo")]
        public async Task<HttpResponseMessage> PostVideo()
        {
            try
            {   
                HttpPostedFile pf = HttpContext.Current.Request.Files[0];
                FileInfo fi = new FileInfo(pf.FileName);               
                Stream fileStream = pf.InputStream;
                byte[] input = new byte[pf.ContentLength];
                //read the file bytes asynchronously
                await fileStream.ReadAsync(input, 0, pf.ContentLength);              
                AzureHelper cc = AzureHelper.AzureInstance;
                string assetId = cc.Upload(fi.Name, input);
                cc.Encode(assetId);
                return Request.CreateResponse(HttpStatusCode.OK, assetId);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        [HttpGet]        
        [ActionName("GetEncodingProgress")]
        public HttpResponseMessage GetEncodingProgress(string assetId)
        {
            try
            {
                AzureHelper cc = AzureHelper.AzureInstance;
                VideoState state = cc.TrackEncodeProgress(assetId);
                return Request.CreateResponse(HttpStatusCode.OK, state);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }

        [HttpGet]
        [ActionName("GetStreamUrl")]
        public HttpResponseMessage GetStreamUrl(string assetId)
        {
            try
            {
                AzureHelper cc = AzureHelper.AzureInstance;
                VideoState state = cc.GetStreamUrl(assetId);
                return Request.CreateResponse(HttpStatusCode.OK, state);
            }
            catch (System.Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e.Message, e);
            }
        }
    }
}

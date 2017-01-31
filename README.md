Project Description

The project is targeted for audiences looking to build video streaming capabilities leveraging Azure Media Services.

The project has below major components:

1. Web Pages

  To async post video file bytes to WEB API
  
  Display the video encode job progress
  
  Return the stream url for the encoded file
  
  Player to steam the video


2. WEB API 

  To upload video files to azure blob account and link the file to azure media services

  Trigger an video encode job in Azure

  Track progress of video encode job

  Returning Stream URL that can be used for streaming videos on client


3. Uses the below manage nuggets:
 
   windowsazure.mediaservices.extensions
 
   bootstrap
  
   jQuery
 

Plugins:
  
  Azure Media Player
  
  
  
The solution is composed of three projects:

1. AzureMediaServices.Website

This project provides the web interface to upload videos to Azure, track progress and play the video.

Before starting the project configure the WEB API URL in the java script file under the path:

Scripts -> custom.js -> AZ.WebApiUrl

Default.html is the entry point for the application and will be that start page. 



2. AzureMediaServices.WebApi

Before starting the project configure the azure configurations in the web.config file. 

Storage Account Name
Storage Account Key
Azure Media Service Account Name
Azure Media Service Account Key
Acs Base Address - > Value for this differ for Government cloud and commercial cloud. 
Cloud Media Api Server - >   Value for this differ for Government cloud and commercial cloud
Cloud End Point Suffix -> Value for this differ for Government cloud and commercial cloud
I have provided the values in web.config for Government cloud and Commercial Azure. Comment and un comment based on usage scenario. The values will differ based on region.

 

3. AzureMediaServices.Common

This is a class library project which provides wrapper on top of azure media sdk. This can reused if required from Desktop, mobile or web frameworks. 

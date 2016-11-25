#load "Queue.cs"

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.MediaServices.Client;
using System.Threading;

private static CloudMediaContext cloudMediaContext = null;

public static void Run(QueueData queueMessage, TraceWriter log)
{    
    string assetId = queueMessage.TargetLocation.Substring(queueMessage.TargetLocation.LastIndexOf("/") + 1).Replace("asset-", "nb:cid:UUID:");
    log.Info($"AssetId: {assetId}");
    ReadMediaAssetAndRunEncoding(assetId, log);
}

public static void ReadMediaAssetAndRunEncoding(string assetId, TraceWriter log)
{    
    string keyIdentifier = ConfigurationManager.AppSettings["MEDIA_ACCOUNT_NAME"];
    string keyValue = ConfigurationManager.AppSettings["MEDIA_ACCOUNT_KEY"];
    
    MediaServicesCredentials _cachedCredentials = new MediaServicesCredentials(keyIdentifier, keyValue);
    cloudMediaContext = new CloudMediaContext(_cachedCredentials);
    
    var assetInstance = from a in cloudMediaContext.Assets where a.Id == assetId select a;
    IAsset asset = assetInstance.FirstOrDefault();

    log.Info($"Asset {asset}");
    log.Info($"Asset Id: {asset.Id}");
    log.Info($"Asset name: {asset.Name}");
    log.Info($"Asset files: ");

    List<string> fileNames = new List<string>();
    foreach (IAssetFile fileItem in asset.AssetFiles)
    {
        log.Info($"    Name: {fileItem.Name}");
        log.Info($"    Size: {fileItem.ContentFileSize}");
        fileNames.Add(fileItem.Name);
    }

    ////submit job
    EncodeToAdaptiveBitrateMP4s(asset, log, fileNames);

    log.Info($"Encoding launched - function done");
}

static public void EncodeToAdaptiveBitrateMP4s(IAsset asset, TraceWriter log, List<string> assetFiles)
{
    if (assetFiles.Count > 1)
    {
        SetFirstFileAsPrimary(asset, Path.GetExtension(assetFiles[0]));
    }
    
    // Prepare a job with a single task to transcode the specified asset
    // into a multi-bitrate asset MP4 720p preset.
    var encodingPreset = "H264 Multiple Bitrate 720p";

    IJob job = cloudMediaContext.Jobs.Create("Encoding " + asset.Name + " to " + encodingPreset);
    
    log.Info($"Job created");
    
    IMediaProcessor mesEncoder = (from p in cloudMediaContext.MediaProcessors where p.Name == "Media Encoder Standard" select p).ToList().OrderBy(mes => new Version(mes.Version)).LastOrDefault();
    
    log.Info($"MES encoder");
    
    ITask encodeTask = job.Tasks.AddNew("Encoding", mesEncoder, encodingPreset, TaskOptions.None);
    encodeTask.InputAssets.Add(asset);
    encodeTask.OutputAssets.AddNew(asset.Name + " as " + encodingPreset, AssetCreationOptions.None);

    log.Info($"Submit job encoder");
    job.Submit();
}


static private void SetFirstFileAsPrimary(IAsset asset, string extension)
{
    var ismAssetFiles = asset.AssetFiles.ToList().Where(f => f.Name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)).ToArray(); 
    if(ismAssetFiles != null) 
    {
        ismAssetFiles.First().IsPrimary = true;
        ismAssetFiles.First().Update();
    }
}

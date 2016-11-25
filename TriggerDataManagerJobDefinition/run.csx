#r "dlls\Microsoft.Azure.Management.HybridData.dll"
#r "dlls\Microsoft.IdentityModel.Clients.ActiveDirectory.dll"
#r "dlls\Microsoft.Internal.Dms.DmsWebJob.dll"
#r "dlls\Microsoft.Rest.ClientRuntime.Azure.Authentication.dll"
#r "dlls\Microsoft.Rest.ClientRuntime.Azure.dll"
#r "dlls\Microsoft.Rest.ClientRuntime.dll"
#r "dlls\Newtonsoft.Json.dll"

using System;
using Microsoft.Azure.Management.HybridData.Models;
using Microsoft.Internal.Dms.DmsWebJob;
using Microsoft.Internal.Dms.DmsWebJob.Contracts;
using System.Threading;

public static void Run(TimerInfo myTimer, TraceWriter log)
{
    log.Info($"C# Timer trigger function executed at: {DateTime.Now}"); 
    var configParams = new ConfigurationParams
    {
        SubscriptionId = "2136cf2e-684f-487b-9fc4-0accc9c0166e",
        TenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
        ClientId = "8ea77d66-00ab-4708-a0ce-d0e947966d34",
        ActiveDirectoryKey = "StorSim1",
        ResourceGroupName = "dmsrg1",
        ResourceName = "haritestresource",
    };

    // Initialize the Data Transformation Job instance.
    DataTransformationJob dataTransformationJob = new DataTransformationJob(configParams);
    
    string jobDefinitionName = "jobDefMedia3";
    DataTransformationInput dataTransformationInput = dataTransformationJob.GetJobDefinitionParameters(jobDefinitionName);


    string deviceName = dataTransformationInput.DeviceName;
    string volumeName = string.Join(",", dataTransformationInput.VolumeNames.ToArray());

    log.Info($"Data manager name: {configParams.ResourceName}");
    log.Info($"Job Definition name: {jobDefinitionName}");

    // Trigger a job, retrieve the jobId and the retry interval for polling.
    int retryAFter;
    string jobId = dataTransformationJob.RunJobAsync(jobDefinitionName, dataTransformationInput, out retryAFter);
    
    log.Info($"Job Id: {jobId}");
}
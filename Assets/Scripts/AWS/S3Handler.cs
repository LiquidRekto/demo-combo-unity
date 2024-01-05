using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using Amazon.S3;
using Amazon.S3.Transfer;
using UnityEngine;
using UnityEngine.Events;


public class S3Handler : MonoBehaviour
{
    private static AmazonS3Client s3Client;

    public static void RegisterClient(S3Info info)
    {
        s3Client = new AmazonS3Client(info.AwsAccessKey, info.AwsSecretKey, info.AwsRegion);
    }


    public static void RetrieveDirectory(S3Info info, string source, string destination)
    {
        TransferUtility utility = new TransferUtility(s3Client);

        TransferUtilityDownloadDirectoryRequest request = new TransferUtilityDownloadDirectoryRequest
        {
            BucketName = info.AwsBucketName,
            S3Directory = source,
            LocalDirectory = destination,
            DownloadFilesConcurrently = true
        };

        utility.DownloadDirectory(request);
    }

    public static IEnumerator RetrieveFile(S3Info info, string source, string destination, UnityAction successCallback, UnityAction failureCallback)
    {
        Debug.Log("OH SHIT");
        Debug.Log(source);
        TransferUtility utility = new TransferUtility(s3Client);

        TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest
        {
            BucketName = info.AwsBucketName,
            Key = source,
            FilePath = destination
        };
        yield return utility.DownloadAsync(request);

        try
        {
            successCallback.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            failureCallback.Invoke();
        }
    }

    public static IEnumerator RetrieveJson(S3Info info, string source, string tempDestination, UnityAction<string> handler)
    {
        Debug.Log(source);
        string contents;

        TransferUtility utility = new TransferUtility(s3Client);

        TransferUtilityDownloadRequest request = new TransferUtilityDownloadRequest
        {
            BucketName = info.AwsBucketName,
            Key = source,
            FilePath = tempDestination
        };
        yield return utility.DownloadAsync(request);
        handler.Invoke(request.FilePath);
    }

    public static void UploadDirectory(S3Info info, string folderPath)
    {
        TransferUtility utility = new TransferUtility(s3Client);

        TransferUtilityUploadDirectoryRequest request = new TransferUtilityUploadDirectoryRequest
        {
            BucketName = info.AwsBucketName,
            Directory = folderPath,
            SearchOption = SearchOption.AllDirectories,
            UploadFilesConcurrently = true
        };

        utility.UploadDirectory(request);
    }
}

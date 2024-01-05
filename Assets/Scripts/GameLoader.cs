using Amazon.GameLift.Model;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class GameLoader : MonoBehaviour
{
#if UNITY_ANDROID
        const string targetFolder = "Android";
#elif UNITY_IOS
        const string targetFolder = "iOS";
#else
    const string targetFolder = "Win64";
#endif

    public static string gameContentDirectory; 

    public static List<string> requiredFiles = new List<string>();

    public UnityAction OnCheckComplete;

    private static S3Info info;

    private void Awake()
    {
        gameContentDirectory = Application.persistentDataPath + "/DownloadedAssets"; // HARDCODED
    }
    public void CheckFiles()
    {
        Debug.Log(gameContentDirectory);
        info = new S3Info();
        info.AwsBucketName = "testunityassetbundle222";
        info.AwsAccessKey = "AKIA2FF2XXEYKXZY4J6L";
        info.AwsSecretKey = "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";
        info.AwsRegion = Amazon.RegionEndpoint.APSoutheast2;
        S3Handler.RegisterClient(info);
        StartCoroutine(S3Handler.RetrieveJson(info, targetFolder + "/filesInfo.json", Application.persistentDataPath + "/temp.json", OnJsonRetrieved));
    }

    public void OnJsonRetrieved(string dst)
    {
        string jsonContent = "";
        Debug.Log("ATTEMPTING TO FETCH FILE...");
        while (jsonContent.Length == 0)
        {
            try
            {
                using (StreamReader reader = new StreamReader(dst, Encoding.UTF8))
                {
                    jsonContent = reader.ReadToEnd();
                }
            }
            catch (System.Exception e)
            {

            }
        }
        File.Delete(dst);
        Debug.Log("EXTRACTING...");
        BundleInfo bundleInfo = JsonUtility.FromJson<BundleInfo>(jsonContent);
        Debug.Log(bundleInfo.filesInfo.Length);
        List<string> availableFiles = bundleInfo.filesInfo.ToList();

        if (Directory.Exists(gameContentDirectory))
        {
            List<string> existedFiles = RecursePrint(gameContentDirectory);
            requiredFiles = availableFiles.Where(x => !existedFiles.Contains(x)).ToList();
        }
        else
        {
            requiredFiles = availableFiles;
        }
        Debug.Log("REQUIRED INFO FETCH COMPLETE!");
        OnCheckComplete.Invoke();
    }

    private static List<string> RecursePrint(string path)
    {
        Queue<string> dirs = new Queue<string>();
        List<string> files = new List<string>();

        foreach (string t in System.IO.Directory.GetFileSystemEntries(path))
        {
            dirs.Enqueue(t);
        }

        while (dirs.Count > 0)
        {
            string target = dirs.Dequeue();
            if (File.GetAttributes(target).HasFlag(FileAttributes.Directory))
            {
                foreach (string t in System.IO.Directory.GetFileSystemEntries(target))
                {
                    dirs.Enqueue(t);
                }
            }
            else
            {
                files.Add(target.Replace("\\", "/").Replace(path + "/", ""));
            }
        }
        return files;
    }

    public void Load(UnityAction OnEachFileSuccessEvent, UnityAction OnFileFailEvent)
    {
        Debug.Log(targetFolder);
        Debug.Log(gameContentDirectory);
        try
        {
            foreach (string file in requiredFiles)
            {
                Debug.Log(file);
                StartCoroutine(S3Handler.RetrieveFile(info, targetFolder + "/" + file, gameContentDirectory + "/" + file, OnEachFileSuccessEvent, OnFileFailEvent));
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            Debug.Log("Loading halted");
        }
        
    }
}

using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.VersionControl;
using UnityEngine;

public enum SERVICE_OPTION
{
    AMAZON_S3 = 0,
    GOOGLE_CLOUD_STORAGE = 1,
    AZURE_BLOB_STORAGE = 2,
    LINODE_OBJECT_STORAGE = 3,
    CUSTOM = 4
}



public class AssetBuildWindow : EditorWindow
{
    // Const
    static string assetBundleDirectoryPath = Application.dataPath + "/../AssetBundles";

    bool win64Enabled, iOSEnabled, androidEnabled, deployEnabled, saveCfgEnabled;
    string deployUrl;
    string configPath = Application.dataPath + "/cabSettings.cfg";
    SERVICE_OPTION serviceType;

    // Amazon S3
    string awsS3BucketName, awsAccessKey, awsSecretKey;

    void RetrieveSavedConfig()
    {
        if (!File.Exists(configPath)) {
            return;
        }
        StreamReader reader = new StreamReader(configPath);
        string line = reader.ReadLine();
        while(line != null )
        {
            try
            {
                if (line.StartsWith("DEPLOY_AWS_S3BUCKETNAME_FIELD="))
                {
                    awsS3BucketName = line.Substring(line.IndexOf("=") + 1);
                }
                if (line.StartsWith("DEPLOY_AWS_ACCESSKEY_FIELD="))
                {
                    awsAccessKey = line.Substring(line.IndexOf("=") + 1);
                }
                if (line.StartsWith("DEPLOY_AWS_SECRETKEY_FIELD="))
                {
                    awsSecretKey = line.Substring(line.IndexOf("=") + 1);
                }
            }
            catch (System.Exception e)
            {

            }
            
            line = reader.ReadLine();
        }
    }

    void SaveConfig()
    {
        StreamWriter writer = new StreamWriter(configPath);
        writer.WriteLine($"DEPLOY_AWS_S3BUCKETNAME_FIELD={awsS3BucketName}");
        writer.WriteLine($"DEPLOY_AWS_ACCESSKEY_FIELD={awsAccessKey}");
        writer.WriteLine($"DEPLOY_AWS_SECRETKEY_FIELD={awsSecretKey}");
        writer.Close();
    }

    void OnGUI()
    {
        RetrieveSavedConfig();
        EditorGUILayout.LabelField("Choose one or more platforms you want to build the assets for:");
        EditorGUILayout.Space();

        // Draw checkboxes
        win64Enabled = EditorGUILayout.Toggle("Windows x64", win64Enabled);
        iOSEnabled = EditorGUILayout.Toggle("iOS", iOSEnabled);
        androidEnabled = EditorGUILayout.Toggle("Android", androidEnabled);

        EditorGUILayout.Space();
        deployEnabled = EditorGUILayout.Toggle("Enable Deployment to Service", deployEnabled);
        // Deploy Area
        EditorGUI.BeginDisabledGroup(!deployEnabled);
        serviceType = (SERVICE_OPTION)EditorGUILayout.EnumPopup("Service:", serviceType);
        switch (serviceType)
        {
            case SERVICE_OPTION.AMAZON_S3:
                {
                    EditorGUILayout.LabelField("S3 Bucket Name:");
                    awsS3BucketName = EditorGUILayout.TextField(awsS3BucketName);
                    EditorGUILayout.LabelField("Access Key:");
                    awsAccessKey = EditorGUILayout.TextField(awsAccessKey);
                    EditorGUILayout.LabelField("Secret Key:");
                    awsSecretKey = EditorGUILayout.TextField(awsSecretKey);
                    break;
                }
            default:
                {
                    EditorGUILayout.LabelField("Deploy URL:");
                    deployUrl = EditorGUILayout.TextField(deployUrl);
                    break;
                }

        }
        
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        saveCfgEnabled = EditorGUILayout.Toggle("Remember Settings", saveCfgEnabled);

        EditorGUILayout.Space();

        // Draw buttons
        if (GUILayout.Button(deployEnabled ? "Build & Deploy" : "Build"))
        {
            if (saveCfgEnabled)
            {
                SaveConfig();
            }

            // If exists, reset
            if (System.IO.Directory.Exists(assetBundleDirectoryPath))
            {
                System.IO.Directory.Delete(assetBundleDirectoryPath, true);
            }
            System.IO.Directory.CreateDirectory(assetBundleDirectoryPath);
            // Building assets

            if (win64Enabled)
            {
                CreateAssetBundles.CreateAssetBundle("/Win64", BuildTarget.StandaloneWindows64);
            }
            if (iOSEnabled)
            {
                CreateAssetBundles.CreateAssetBundle("/iOS", BuildTarget.iOS);
            }
            if (androidEnabled)
            {
                CreateAssetBundles.CreateAssetBundle("/Android", BuildTarget.Android);
            }


            // Deploy
            if (deployEnabled)
            {
                // Do automated deploy jobs
                switch (serviceType)
                {
                    case SERVICE_OPTION.AMAZON_S3:
                        {
                            S3Info info = new S3Info();
                            info.AwsBucketName = awsS3BucketName;
                            info.AwsAccessKey = awsAccessKey;
                            info.AwsSecretKey = awsSecretKey;
                            info.AwsRegion = Amazon.RegionEndpoint.APSoutheast2;
                            S3Handler.RegisterClient(info);
                            S3Handler.UploadDirectory(info, assetBundleDirectoryPath);
                            break;
                        }
 

                }
            }

            //

            Close(); // Close the window

        }
        if (GUILayout.Button("Cancel"))
        {
            Close(); // Close the window
        }
    }

    [MenuItem("Assets/Create Asset Bundles")]
    public static void TestBuildDial()
    {
        if (GetWindow<AssetBuildWindow>() == null)
        {
            GetWindow<AssetBuildWindow>().Show();
        }
    }
}

public class CreateAssetBundles
{
    static string assetBundleDirectoryPath = Application.dataPath + "/../AssetBundles";

    [MenuItem("Assets/TEST Log Script")]
    private static void LogScriptInfo()
    {
        CreateScriptResources("/Scripts");
        
    }

    public static void CreateAssetBundle(string outputPath, BuildTarget target)
    {
        

        // If exists, reset
        if(System.IO.Directory.Exists(assetBundleDirectoryPath + outputPath))
        {
            System.IO.Directory.Delete(assetBundleDirectoryPath + outputPath, true);
        }
        System.IO.Directory.CreateDirectory(assetBundleDirectoryPath + outputPath);
        try
        {
            BuildPipeline.BuildAssetBundles(assetBundleDirectoryPath + outputPath, BuildAssetBundleOptions.None, target);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }

        // Fetch all
        List<string> fileList = RecursePrint(assetBundleDirectoryPath + outputPath);
        
        BundleInfo bundle = new BundleInfo();
        bundle.filesInfo = fileList.ToArray();

        File.WriteAllText(assetBundleDirectoryPath + $"{outputPath}/filesInfo.json", JsonUtility.ToJson(bundle));
    }

    public static void CreateScriptResources(string outputPath)
    {
        string scriptResourceDirectoryPath = Application.dataPath + outputPath;

        Debug.Log(Application.dataPath);
        List<string> fileList = RecursePrint(scriptResourceDirectoryPath);
        foreach (string file in fileList)
        {
            if (!file.EndsWith(".meta") && !file.StartsWith("Editor"))
            {
                string txt = File.ReadAllText(scriptResourceDirectoryPath + "/" + file);
                TextAsset myCodeAsset = new TextAsset(txt);
                if (myCodeAsset != null)
                {
                    Debug.Log(myCodeAsset.text);
                }
                else
                {
                    Debug.LogError("Failed to load text asset!");
                }
            }
        }
    }


    public static List<string> RecursePrint(string path)
    {
        Queue<string> dirs = new Queue<string>();
        List<string> files = new List<string>();

        foreach (string t in System.IO.Directory.GetFileSystemEntries(path))
        {
            dirs.Enqueue(t);
        }

        while(dirs.Count > 0)
        {
            string target = dirs.Dequeue();
            if (File.GetAttributes(target).HasFlag(FileAttributes.Directory)) {
                foreach (string t in System.IO.Directory.GetFileSystemEntries(target))
                {
                    dirs.Enqueue(t);
                }
            } else { 
                files.Add(target.Replace("\\", "/").Replace(path + "/", ""));
            }
        }
        return files;
    }
}

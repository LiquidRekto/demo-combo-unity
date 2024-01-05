using Amazon.S3.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingBar : MonoBehaviour
{
    public GameObject loadingBar;
    GameLoader gameLoader;
    public Image loadingBarBody;
    public GameObject checkText;
    public GameObject startButton;
    int requiredFilesCount;
    int currentRetrievedFiles;
    bool checkFinished = false;
    // Start is called before the first frame update
    private void Awake()
    {
        gameLoader = gameObject.AddComponent<GameLoader>();
    }
    void Start()
    {
        
        currentRetrievedFiles = 0;
        gameLoader.OnCheckComplete += OnCheckComplete;
        gameLoader.CheckFiles();
        
    }

    void OnCheckComplete()
    {
        requiredFilesCount = GameLoader.requiredFiles.Count;
        loadingBar.SetActive(true);
        checkText.SetActive(false);
        gameLoader.Load(OnEachFileSuccess, OnFileFailure);
        checkFinished = true;
    }

    void OnEachFileSuccess()
    {
        currentRetrievedFiles++;
    }

    void OnFileFailure()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (requiredFilesCount > 0)
        {
            loadingBarBody.fillAmount = (float)currentRetrievedFiles / requiredFilesCount;
        }
        
        if (checkFinished && currentRetrievedFiles == requiredFilesCount)
        {
            loadingBar.SetActive(false);
            startButton.SetActive(true);
        }
    }
}

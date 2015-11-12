using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;


public class Test : MonoBehaviour {

  private string applicationPath;
  private string downloadURL = "http://www.neopoly.com/ftp/unity/";
  private string fileName = "cards.7z";

  private bool isFirstTime = true;
  private bool isDownloadFinished = false;
  private bool isDecompressing = false;
  private bool isLargeFile = true;

  private int progress;
  private int res;

  private string logText;

  void Awake() {
    applicationPath = Application.persistentDataPath;
  }

  void OnGUI() {
    GUI.Label(new Rect(0, 0, 250, 30), Time.realtimeSinceStartup.ToString());
    GUI.Label(new Rect(280, 0, 250, 30), logText);
    if(isDownloadFinished) {
      if(isFirstTime) {
        Debug.Log("package downloaded, ready to extract at " + applicationPath);
        isFirstTime = false;
      }
      if(!isDecompressing) {
        if(GUI.Button(new Rect(50, 150, 250, 100), "Extract 7zip Files test")) {
          isDecompressing = true;
          Decompression();
        }
      }
    } else {
      if(GUI.Button(new Rect(50, 50, 250, 100), "Start Download")) {
        if(File.Exists(applicationPath + "/" + fileName)) File.Delete(applicationPath + "/" + fileName);
        StartCoroutine(downloadFile());
      }
    }
  }

  private void Decompression() {
    Thread decompressionThread = new Thread(threadedDecompressionFunction);
    decompressionThread.Start();
    Thread progressThread = new Thread(reportProgress);
    progressThread.Start();
  }

  private void reportProgress() {
    #if UNITY_iOS
      while( lzma.getProgressCount2() >= 0 && lzma.getProgressCount2() < 1000) {
        logText = "Decompression Progression iOS: " + lzma.getProgressCount2();
        Debug.Log("Decompression Progression iOS: " + lzma.getProgressCount2());
      }
    #else
    while(progress >= 0) {
        logText = "Decompression Progression: " + progress.ToString();
        Debug.Log("Decompression Progression: " + progress.ToString());
      }
    #endif
  }

  private void threadedDecompressionFunction() {
    if(!Directory.Exists(applicationPath + "/extractionFolder")) Directory.CreateDirectory(applicationPath + "/extractionFolder");
    #if UNITY_iOS
    lzma.setProgressCount2();
    #endif
    res = lzma.doDecompress7zip(applicationPath + "/" + fileName, applicationPath + "/extractionFolder/", ref progress, isLargeFile);
    Debug.Log("Decompress Status: " + res);
    progress = -1;
  }

  private IEnumerator downloadFile() {
    using(WWW www = new WWW(downloadURL + fileName)) {
      while(!www.isDone) {
        logText = "Download Progress: " + www.progress * 100;
        Debug.Log("Download Progress: " + www.progress *100);
        yield return null;
      }
      if(www.error == null) {
        using(FileStream fs = new FileStream(applicationPath + "/" + fileName, FileMode.Create)) {
          fs.Write(www.bytes, 0, www.bytes.Length);
          fs.Flush();
          fs.Close();
          isDownloadFinished = true;
        }
      } else {
        Debug.Log("Download couldn't be completed: " + www.error);
      }
      logText = "";
    }
  }

}

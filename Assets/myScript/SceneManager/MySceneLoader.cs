using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneLoader : MonoBehaviour
{

    public void SetInitialIndex(int index)
    {
        RandomSceneManager.currentIndex = index;
    }

    public void LoadNextScene()
    {
        LoadBySceneNum(RandomSceneManager.getSceneNum());
        RandomSceneManager.currentIndex += 1;
    }

    public void LoadBySceneNum(int sceneNumber)
    {
        StartCoroutine(LoadAsyncScene(sceneNumber));
    }

    private IEnumerator LoadAsyncScene(int sceneNumber)
    {

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneNumber);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneLoader : MonoBehaviour
{
    public void SetInitialIndex(int index)
    {
        RandomSceneManager.currentIndex = index;
    }
    
    // Main method to load by scene name
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsyncScene(sceneName));
    }

    // Modified to handle scene names
    private IEnumerator LoadAsyncScene(string sceneName)
    {
        // Added safety checks
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty!");
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false; // Prevent immediate scene switch

        // Wait until scene is ready to activate
        while (!asyncLoad.isDone)
        {
            // When loading is almost complete
            if (asyncLoad.progress >= 0.9f)
            {
                // Add a brief delay to prevent mistouch
                yield return new WaitForSeconds(0.5f);
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
    }
}
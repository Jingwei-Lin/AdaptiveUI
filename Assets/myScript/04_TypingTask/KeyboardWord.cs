using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyboardWord : MonoBehaviour
{
    public InputField inputField;
    private bool deleteClicked = false;

    private void Update()
    {
        Logger.enteredSentence = inputField.text;
        LogTutorial.enteredSentence = inputField.text;
        
        if (deleteClicked)
        {
            Logger.fixedError = 1;
            deleteClicked = false;  // Reset the flag after updating the logger
        }
        else
        {
            Logger.fixedError = 0;
        }
    }

    public void OnKeyClicked(string key)
    {
        inputField.text += key;
    }

    public void DeleteClicked()
    {
        if (inputField.text.Length > 0)
        {
            inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            deleteClicked = true;
        }
    }
}
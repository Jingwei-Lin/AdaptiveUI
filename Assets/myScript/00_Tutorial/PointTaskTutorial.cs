using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Threading.Tasks;
using System.Linq;


public class PointTaskTutorial : MonoBehaviour
{
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] private GameObject nextDialog;
    [SerializeField] private GameObject leftController;
    [SerializeField] private GameObject rightController;
    public static Vector3 centerLocation;
    public static Vector3 fingerLocation;
    public static int currentIndex = 0;
    public static int buttonNumber;
    private bool isFirstSelection = true;
    public static float distance = 0.03f;
    public static float scale = 0.06f;

    private InteractableColorVisual.ColorState grayColorState = new InteractableColorVisual.ColorState
    {
        Color = new Color(0.5f, 0.5f, 0.5f, 1),
        ColorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1),
        ColorTime = 0.1f
    };

    private InteractableColorVisual.ColorState blueColorState = new InteractableColorVisual.ColorState
    {
        Color = new Color(0, 0, 1, 0.7f),
        ColorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1),
        ColorTime = 0.1f
    };

    private void Start()
    {
        setButtonPositions(scale, distance);

        for (int i = 1; i < buttons.Count; i++)
        {
            buttons[i].SetActive(false);
        }
        leftController.SetActive(false);
        rightController.SetActive(false);
    }

    public void setSelectColour(GameObject button)
    {
        InteractableColorVisual currentButton = button.GetComponentInChildren<InteractableColorVisual>();
        button.SetActive(false);
        currentButton.InjectOptionalNormalColorState(grayColorState);
        button.SetActive(true);
    }

    public void setUnselectColour(GameObject button)
    {
        InteractableColorVisual currentButton = button.GetComponentInChildren<InteractableColorVisual>();
        button.SetActive(false);
        currentButton.InjectOptionalNormalColorState(blueColorState);
        button.SetActive(true);
    }

    public void onButtonClicked(int buttonIndex)
    {
        buttonNumber = buttonIndex;
        if (isFirstSelection)
        {
            setSelectColour(buttons[currentIndex]);
            isFirstSelection = false;
            buttons[currentIndex].SetActive(false);
            currentIndex += 1;
            for (int i = 1; i < buttons.Count; i++)
            {
                buttons[i].SetActive(true);
            }
            setUnselectColour(buttons[currentIndex]);
        }
        else
        {
            if (buttonIndex == currentIndex)
            {
                if (currentIndex == buttons.Count - 1)
                {
                    setSelectColour(buttons[currentIndex]);
                    // Start the coroutine with a 1-second delay
                    StartCoroutine(DelayCoroutine(1.0f));
                }
                else
                {
                    setSelectColour(buttons[currentIndex]);
                    currentIndex += 1;
                    buttons[currentIndex].SetActive(true);
                    setUnselectColour(buttons[currentIndex]);
                }
            }
        }
    }

    public void setButtonPositions(float scale, float distance)
    {
        List<Vector3> buttonPositions = new List<Vector3>();
        int numberOfButtons = buttons.Count - 1;
        float radius = (distance + scale) * (numberOfButtons - 1) / (2 * Mathf.PI); // Calculate radius based on the distance and number of buttons

        for (int i = 0; i <= numberOfButtons; i++)
        {
            // Calculate angle for each button
            float angle = i * Mathf.PI * 2 / numberOfButtons;
            // Position calculation based on the angle and radius
            Vector3 buttonPos = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            buttonPositions.Add(buttonPos);
        }

        for (int i = 1; i <= numberOfButtons; i += 2)
        {
            if (i == numberOfButtons)
            {
                buttons[i].transform.localPosition = buttonPositions[(i - 1) / 2];
                buttons[i].transform.localScale = new Vector3(scale, scale, scale);
                return;
            }
            buttons[i].transform.localPosition = buttonPositions[(i - 1) / 2];
            buttons[i].transform.localScale = new Vector3(scale, scale, scale);

            buttons[i + 1].transform.localPosition = buttonPositions[(i - 1) / 2 + 6];
            buttons[i + 1].transform.localScale = new Vector3(scale, scale, scale);
        }
    }

    private IEnumerator DelayCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 1; i < buttons.Count; i++)
        {
            buttons[i].SetActive(false);
        }
        nextDialog.SetActive(true);
        leftController.SetActive(false);
        rightController.SetActive(false);
    }

    public void GetCenterLocation(GameObject gameObject)
    {
        centerLocation = gameObject.transform.position;
    }

    public void GetFingerLocation(GameObject gameObject)
    {
        fingerLocation = gameObject.transform.position;
    }
}

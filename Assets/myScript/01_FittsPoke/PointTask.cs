using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Threading.Tasks;
using System.Linq;


public class PointTask : MonoBehaviour
{
    [SerializeField] private List<GameObject> buttons;
    [SerializeField] private GameObject nextDialog;
    public static Vector3 centerLocation = new Vector3(0, 0, 0);
    public static Vector3 fingerLocation;

    public static int currentIndex = 0;
    public static int buttonNumber = 0;
    private bool isFirstSelection = true;
    public static int currentIteration = 0;
    public static List<int> randomList = new List<int> { 0, 1, 2, 3, 4, 5 };
    public static List<float> scales = new List<float> { 0.06f, 0.06f, 0.04f, 0.04f, 0.08f, 0.08f };
    public static List<float> distances = new List<float> { 0.03f, 0.05f, 0.03f, 0.05f, 0.03f, 0.05f };

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

    private InteractableColorVisual.ColorState whiteColorState = new InteractableColorVisual.ColorState
    {
        Color = new Color(1, 1, 1, 0.2f),
        ColorCurve = AnimationCurve.EaseInOut(0, 0, 1, 1),
        ColorTime = 0.1f
    };

    private void Start()
    {
        randomList = randomList.OrderBy(i => Random.value).ToList();
        setButtonPositions(scales[randomList[currentIteration]], distances[randomList[currentIteration]]);
        currentIteration++;

        for (int i = 1; i < buttons.Count; i++)
        {
            buttons[i].SetActive(false);
        }
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

    public void setDefaultColour(GameObject button)
    {
        InteractableColorVisual currentButton = button.GetComponentInChildren<InteractableColorVisual>();
        button.SetActive(false);
        currentButton.InjectOptionalNormalColorState(whiteColorState);
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
                setDefaultColour(buttons[i]);
                buttons[i].SetActive(true);
            }
            setUnselectColour(buttons[currentIndex]);
        }
        else
        {
            if (buttonIndex == currentIndex)
            {
                if (buttonIndex == buttons.Count - 1)
                {
                    setSelectColour(buttons[currentIndex]);

                    // jumpt to next ID
                    if (currentIteration < scales.Count)
                    {
                        buttons[0].SetActive(false);
                        setDefaultColour(buttons[0]);
                        buttons[0].SetActive(true);
                        for (int i = 1; i < buttons.Count; i++)
                        {
                            buttons[i].SetActive(false);
                        }
                        currentIndex = 0; // Reset index
                        isFirstSelection = true; // Reset first selection state
                        // Adjust positions
                        setButtonPositions(scales[randomList[currentIteration]], distances[randomList[currentIteration]]);
                        currentIteration++;
                    }
                    else
                    {
                        for (int i = 1; i < buttons.Count; i++)
                        {
                            buttons[i].SetActive(false);
                        }
                        nextDialog.SetActive(true);
                    }
                }
                else
                {
                    setSelectColour(buttons[currentIndex]);
                    setUnselectColour(buttons[currentIndex + 1]);
                    currentIndex += 1;
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

    public void GetCenterLocation(GameObject gameObject)
    {
        centerLocation = gameObject.transform.position;
    }

    public void GetFingerLocation(GameObject gameObject)
    {
        fingerLocation = gameObject.transform.position;
    }
}

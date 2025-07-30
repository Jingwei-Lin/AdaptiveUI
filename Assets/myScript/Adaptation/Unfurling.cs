using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Unfurling : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private WalkDetector walkDetector;
    [SerializeField] private HandEncumbranceDetector encumbranceDetector;
    
    [SerializeField] private float walkingButtonScale = 0.08f;
    [SerializeField] private float normalButtonScale = 0.06f;
    [SerializeField] private float scalingSpeed = 5f;

    [Header("Color Settings")]
    [SerializeField] private Color walkingColor = Color.green; // Default walking color
    [SerializeField] private Color normalColor = Color.white; // Default normal color

    [Header("Button References")]
    [SerializeField] private List<GameObject> fittsRingButtons = new List<GameObject>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private Dictionary<GameObject, Image> buttonImages = new Dictionary<GameObject, Image>();

    [Header("UI Elements")]
    [SerializeField] private Text debugText;

    void Start()
    {
        CacheOriginalScales();
        CacheButtonComponents();
    }

    // Update is called once per frame
    void Update()
    {
        bool isWalking = walkDetector.IsWalking;
        bool isEncumbered = encumbranceDetector.isEncumbrance;
        if (isWalking || isEncumbered)
        {
            AnimateButtonScale(walkingButtonScale);
            SetButtonColor(walkingColor);
            debugText.text = "Scaler: True";

        }
        else
        {
            AnimateButtonScaleToOriginal();
            ResetButtonColorsToOriginal();
            debugText.text = "Scaler: False";
        }

    }

    private void CacheOriginalScales()
    {
        originalScales.Clear();
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null)
                originalScales[btn] = btn.transform.localScale;
        }
    }

    private void CacheButtonComponents()
    {
        buttonImages.Clear();
        originalColors.Clear();
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null)
            {
                // Cache Image components
                Image img = btn.GetComponent<Image>();
                if (img != null)
                {
                    buttonImages[btn] = img;
                    originalColors[btn] = img.color;
                }
                else
                {
                    Debug.LogError($"Button {btn.name} is missing Image component!");
                }
            }
        }
    }

    private void AnimateButtonScale(float targetScale)
    {
        foreach (GameObject button in fittsRingButtons)
        {
            if (button != null)
            {
                Vector3 target = Vector3.one * targetScale;
                button.transform.localScale = Vector3.Lerp(
                    button.transform.localScale, 
                    target, 
                    Time.deltaTime * scalingSpeed
                );
            }
        }
    }

    private void AnimateButtonScaleToOriginal()
    {
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null && originalScales.ContainsKey(btn))
            {
                btn.transform.localScale = Vector3.Lerp(
                    btn.transform.localScale, 
                    originalScales[btn], 
                    Time.deltaTime * scalingSpeed
                );
            }
        }
    }
    
    private void SetButtonColor(Color color)
    {
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null && buttonImages.ContainsKey(btn))
            {
                buttonImages[btn].color = color;
            }
        }
    }

    private void ResetButtonColorsToOriginal()
    {
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null && originalColors.ContainsKey(btn) && buttonImages.ContainsKey(btn))
            {
                buttonImages[btn].color = originalColors[btn];
            }
        }
    }
}

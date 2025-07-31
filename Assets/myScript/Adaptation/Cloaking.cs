using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

public class Cloaking : MonoBehaviour
{
    [Header("Scaling Settings")]
    [SerializeField] private WalkDetector walkDetector;
    [SerializeField] private HandEncumbranceDetector encumbranceDetector;
    [SerializeField] private float walkingButtonScale = 0.18f;
    [SerializeField] private float scalingSpeed = 5f;

    [Header("Button References")]
    [SerializeField] private List<GameObject> fittsRingButtons = new List<GameObject>();
    private Dictionary<GameObject, Vector3> originalScales = new Dictionary<GameObject, Vector3>();
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();
    private Dictionary<GameObject, Image> buttonImages = new Dictionary<GameObject, Image>();
    private Dictionary<GameObject, bool> isHovered = new Dictionary<GameObject, bool>(); // Track hover state per button

    [Header("UI Elements")]
    [SerializeField] private Text debugText;

    [Header("Ray Caster Settings")]
    [SerializeField] private GameObject RayVisuals;

    private int hoverCount = 0; // Tracks active hover states

    void Start()
    {
        CacheOriginalScales();
        CacheButtonComponents();
        InitializeHoverStates();
    }

    void Update()
    {
        // Animate all buttons in Update for smooth continuous scaling
        foreach (var btn in fittsRingButtons)
        {
            if (btn == null) continue;
            Vector3 targetScale = isHovered[btn]
                ? Vector3.one * walkingButtonScale
                : originalScales[btn];

            btn.transform.localScale = Vector3.Lerp(
                btn.transform.localScale,
                targetScale,
                Time.deltaTime * scalingSpeed
            );

            //if (walkDetector.IsWalking || encumbranceDetector.isEncumbrance)
            {
                Transform surface = transform.Find("Model/Surface");
                if (surface != null)
                {
                    surface.localScale = new Vector3(2f, 2f, 0.001f);
                }
                else
                {
                    
                }
            }
            
        }
    }

    private void InitializeHoverStates()
    {
        foreach (var btn in fittsRingButtons)
        {
            if (btn != null)
            {
                isHovered[btn] = false;
            }
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

    public void RayInvisibility(GameObject Button)
    {
        // /if (walkDetector.IsWalking || encumbranceDetectorleft.isEncumbrance || encumbranceDetectorright.isEncumbrance)
        {
            // Update hover state
            if (isHovered.ContainsKey(Button))
                isHovered[Button] = true;
        
            // Update ray visuals using counter
            hoverCount++;
            if (RayVisuals != null && hoverCount == 1) 
                RayVisuals.SetActive(false);
        }
        
    }

    public void RayVisibility(GameObject Button)
    {
        //if (walkDetector.IsWalking || encumbranceDetectorleft.isEncumbrance || encumbranceDetectorright.isEncumbrance)
        {
            // Update hover state
            if (isHovered.ContainsKey(Button))
                isHovered[Button] = false;
            
            // Update ray visuals using counter
            hoverCount = Mathf.Max(0, hoverCount - 1);
            if (RayVisuals != null && hoverCount == 0) 
                RayVisuals.SetActive(true);
        }
    }
}
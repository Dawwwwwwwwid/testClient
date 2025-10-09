using TMPro;
using UnityEngine;

/// <summary>
/// TextDisplay is an abstract class used to display text over a GameObject.
/// </summary>
public abstract class TextDisplay : MonoBehaviour
{
    // Y offset for text
    [Header("Text Y Offset")]
    [SerializeField] protected Vector3 offset;

    protected Transform observerCamera;

    // UI elements
    protected Transform worldSpaceCanvas;
    protected Entity displayEntity;
    protected TMP_Text displayText;
    protected string textString = string.Empty;

    protected bool hidden = false;

    /// <summary>
    /// Assigns the GameObjects from the scene.
    /// </summary>
    protected virtual void Start()
    {
        // assign gameobjects
        displayEntity = GetComponentInParent<Entity>();
        displayText = GetComponent<TMP_Text>();
        observerCamera = Camera.main.transform;
        worldSpaceCanvas = GameObject.Find("WorldSpaceCanvas").transform;
        transform.SetParent(worldSpaceCanvas);
    }

    /// <summary>
    /// Checks the state of the displayEntity and hides or destroys this TextDisplay instance.
    /// Points this TextDisplay instance to face the camera transform.
    /// </summary>
    protected virtual void Update()
    {
        // if the displayEntity is null, it has been destroyed so this needs to be destroyed as well
        if (displayEntity == null)
        {
            Destroy(gameObject);
            return;
        }

        // if the displayEntity is inactive, it has been hidden and so this needs to be hidden as well
        if (!hidden && !displayEntity.IsVisualsActive)
        {
            displayText.text = string.Empty;
            hidden = true;
        }
        else if (hidden && displayEntity.IsVisualsActive)
        {
            displayText.text = textString;
            hidden = false;
        }

        // point to camera
        if (transform.position - observerCamera.position == Vector3.zero) return;
        transform.SetPositionAndRotation(displayEntity.transform.position + offset, Quaternion.LookRotation(transform.position - observerCamera.position));
    }
}


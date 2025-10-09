/// <summary>
/// EntityDisplay implements the abstract TextDisplay class
/// </summary>
public class EntityDisplay : TextDisplay
{
    /// <summary>
    /// Overrides the base start method, calls the base method and then sets the text of this EntityDisplay instance
    /// </summary>
    protected override void Start()
    {
        base.Start();
        SetText();
    }

    /// <summary>
    /// Sets the text based on the displayEntity set
    /// </summary>
    private void SetText()
    {
        displayText.text = displayEntity.name;
        textString = displayEntity.name;
    }

    /// <summary>
    /// Toggles the larger font size of the text.
    /// </summary>
    /// <param name="toggle">Toggle indicates whether to set the highlight to on or off.</param>
    public void ToggleHighlight(bool toggle)
    {
        if (toggle)
        {
            displayText.fontSize = displayText.fontSizeMax;
        }
        else
        {
            displayText.fontSize = displayText.fontSizeMin;
        }
    }
}
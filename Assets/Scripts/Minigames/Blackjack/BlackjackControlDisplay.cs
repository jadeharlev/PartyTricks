using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackjackControlDisplay : MonoBehaviour {
    public Image image;
    public TMP_Text label;

    public void Hide() {
        var color = image.color;
        color.a = 0;
        image.color = color;

        var labelColor = label.color;
        labelColor.a = 0;
        label.color = labelColor;
    }

    public void Show() {
        var color = image.color;
        color.a = 1;
        image.color = color;
        
        var labelColor = label.color;
        labelColor.a = 1;
        label.color = labelColor;
    }
}

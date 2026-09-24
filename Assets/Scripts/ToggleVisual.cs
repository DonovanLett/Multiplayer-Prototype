using UnityEngine;
using UnityEngine.UI;

public class ToggleVisual : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private Image targetImage;

    [SerializeField] private Color onColor = Color.white;
    [SerializeField] private Color offColor = Color.gray;

    private void Start()
    {
        if(toggle == null)
        {
            toggle = GetComponent<Toggle>();
        }

        if(targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }

        if(toggle != null && targetImage != null)
        {
            UpdateVisual(toggle.isOn);
            toggle.onValueChanged.AddListener(UpdateVisual);
        }
        UpdateVisual(toggle.isOn);
        // toggle.onValueChanged.AddListener(UpdateVisual);
    }

    private void UpdateVisual(bool isOn)
    {
        targetImage.color = isOn ? onColor : offColor;
    }

    private void OnDestroy()
    {
        toggle.onValueChanged.RemoveListener(UpdateVisual);
    }
}

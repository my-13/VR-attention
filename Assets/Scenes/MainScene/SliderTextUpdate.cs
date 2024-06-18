using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderTextUpdate : MonoBehaviour
{

    public string text = "";
    public float value;
    public GameObject sliderText;

    public void UpdateValue()
    {
        value = GetComponent<UnityEngine.UI.Slider>().value;
        // Text mesh pro
        sliderText.GetComponent<TMPro.TextMeshProUGUI>().text = text + " " + value.ToString();
        //sliderText.GetComponent<UnityEngine.UI.Text>().text = text + " " + value.ToString();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateValue();
    }
}

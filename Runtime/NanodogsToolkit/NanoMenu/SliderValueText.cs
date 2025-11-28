using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nanodogs.Toolkit.NanoMenu
{
    public class SliderValueText : MonoBehaviour
    {
        public Slider slider;
        public TMP_Text text;
        
        public void OnSliderValueChanged()
        {
            // if the value is 0.5, the text would say 50%
            // text.text = slider.normalizedValue.ToString(); this would not provide that result

            // gives percentage
            float normalized = (slider.normalizedValue / slider.maxValue) * 100;
            text.text = normalized.ToString("F1") + "%";
        }
    }

}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Nanodogs.UniversalScripts.UI
{
    public class SliderValueText : MonoBehaviour
    {
        public Slider slider;
        public TMP_Text text;

        public bool givePercent = false;
        public string format = "F2";

        public bool doDivide = false;
        public float dividen = 10;

        public void Start()
        {
            UpdateText();
        }

        public void OnSliderValueChanged()
        {
            UpdateText();
        }

        private void UpdateText()
        {
            float value;

            if (givePercent)
            {
                value = slider.normalizedValue * 100f;
            }
            else
            {
                value = slider.value;
            }

            // Apply division if enabled
            if (doDivide && dividen != 0)
            {
                value /= dividen;
            }

            // Apply % suffix if needed
            if (givePercent)
            {
                text.text = value.ToString(format) + "%";
            }
            else
            {
                text.text = value.ToString(format);
            }
        }
    }   
}
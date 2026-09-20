using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Nanodogs.Toolkit.NanoMenu
{
    public class HoverActionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public AudioClip hoverEnterSound;
        public AudioClip hoverExitSound;
        public AudioClip clickSound;

        public UnityEvent PointerEnter;
        public UnityEvent PointerExit;
        public UnityEvent PointerClick;

        public void OnPointerEnter(PointerEventData eventData)
        {
            PointerEnter.Invoke();
            if (hoverEnterSound != null)
            {
                AudioSource.PlayClipAtPoint(hoverEnterSound, Camera.main.transform.position);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PointerExit.Invoke();
            if (hoverExitSound != null)
            {
                AudioSource.PlayClipAtPoint(hoverExitSound, Camera.main.transform.position);
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            PointerClick.Invoke();
            if (clickSound != null)
            {
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            }
        }
    }
}

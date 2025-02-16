using UnityEngine;
using UnityEngine.EventSystems;

public class OverrideGameplayClickWhileHovering : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GooyoController controller;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (this.controller)
        {
            this.controller.disableClicks = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (this.controller)
        {
            this.controller.disableClicks = false;
        }
    }
}

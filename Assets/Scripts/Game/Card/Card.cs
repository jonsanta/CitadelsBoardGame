using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//represents a ingame card
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected float width = 40f;
    protected float height = 65f;
    protected bool selectable = true;

    public void SetCard(float width, float height, bool selectable)
    {
        this.width = width;
        this.height = height;
        SetCardSize(width, height);
        this.selectable = selectable;
    }

    protected void SetCardSize(float width, float height)
    {
        GetComponent<LayoutElement>().preferredWidth = width;
        GetComponent<LayoutElement>().preferredHeight = height;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (selectable)
        {
            if (GetComponentInParent<Animator>() != null) //Animation for cards in hand
                GetComponentInParent<Animator>().SetBool("show", true);
            SetCardSize(100f, 150f);
            GetComponent<Outline>().enabled = true;
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (selectable)
        {
            if (GetComponentInParent<Animator>() != null) //Animation for cards in hand
                GetComponentInParent<Animator>().SetBool("show", false);
            SetCardSize(width, height);
            GetComponent<Outline>().enabled = false;
        }
    }
}

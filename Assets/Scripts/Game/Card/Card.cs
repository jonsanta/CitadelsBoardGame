using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//represents a ingame card
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    protected float width = 40f;
    protected float height = 65f;
    protected bool selectable = true; //Some cards might be clicked or not

    public void SetCard(float width, float height, bool selectable)
    {
        this.width = width;
        this.height = height;
        SetCardSize(width, height);
        this.selectable = selectable;
    }

    protected void SetCardSize(float width, float height) //Adjust card size
    {
        GetComponent<LayoutElement>().preferredWidth = width;
        GetComponent<LayoutElement>().preferredHeight = height;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (selectable)
        {
            if (GetComponentInParent<Animator>() != null) //Show Hand cards
                GetComponentInParent<Animator>().SetBool("show", true);
            SetCardSize(100f, 150f); //Increase pointed card size
            GetComponent<Outline>().enabled = true; //Outline pointed card
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (selectable)
        {
            if (GetComponentInParent<Animator>() != null) //Hide Hand cards
                GetComponentInParent<Animator>().SetBool("show", false);
            SetCardSize(width, height); //Return pointed card to default size
            GetComponent<Outline>().enabled = false; //Disable outline for pointed card
        }
    }
}

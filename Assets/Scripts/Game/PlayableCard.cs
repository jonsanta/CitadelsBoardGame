using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//represents a ingame Playable card
[RequireComponent(typeof(Image))]
public class PlayableCard : Card, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private int id; //card id
    private string cardName; //card name
    private int price; // card price
    private string colour; //card colour

    [SerializeField] private Transform hand = null;
    private GamePlayer playerInstance = null;

    private Vector3 velocity = Vector3.zero;

    public void SetCard(string[] data, bool flag, Transform hand, GamePlayer playerInstance)
    {
        id = int.Parse(data[0]);
        cardName = data[1];
        price = int.Parse(data[2]);
        colour = data[3];
        GetComponent<LayoutElement>().preferredWidth = width;
        GetComponent<LayoutElement>().preferredHeight = height;

        this.hand = hand;
        this.playerInstance = playerInstance;

        foreach (Image image in GetComponentsInChildren<Image>())
            if (image.gameObject.name == "Sprite") image.sprite = Resources.Load<Sprite>($"Ciudadelas/Distritos/{cardName}");
        GetComponent<Button>().enabled = flag;
    }

    public string GetCardName()
    {
        return cardName;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        if (GetComponent<Button>().enabled == true)
        {
            transform.SetParent(hand.parent);
            SetCardSize(width, height);
        }
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        RectTransform rect = transform as RectTransform;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var globalMousePosition) && GetComponent<Button>().enabled == true)
        {
            rect.position = Vector3.SmoothDamp(rect.position, globalMousePosition, ref velocity, .02f); //.01f works fine
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(hand);
        transform.SetSiblingIndex(playerInstance.GetCardIndex(this.gameObject));
    }
}

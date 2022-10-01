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

    private GameObject emptySpace;

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
        transform.SetParent(hand.parent);
        SetCardSize(width, height);
        emptySpace = new GameObject();
        emptySpace.AddComponent<Image>();
        emptySpace.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        emptySpace.AddComponent<LayoutElement>();
        emptySpace.GetComponent<LayoutElement>().preferredWidth = 80f;
        emptySpace.transform.SetParent(hand);
        emptySpace.transform.SetSiblingIndex(playerInstance.GetCardIndex(gameObject));
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        hand.gameObject.GetComponent<Animator>().SetBool("show", true);
        RectTransform rect = transform as RectTransform;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var globalMousePosition))
        {
            rect.position = Vector3.SmoothDamp(rect.position, globalMousePosition, ref velocity, .02f); //.01f works fine
        }
        SwapHandCards();
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        Destroy(emptySpace);
        if (GetComponent<Button>().enabled && Input.mousePosition.y > Screen.height * 0.365f)
        {
            playerInstance.PlayCard(gameObject);
            hand.gameObject.GetComponent<Animator>().SetBool("show", false);
        }
        else
        {
            transform.SetParent(hand);
            SetCardSize(100f, 150f);
            transform.SetSiblingIndex(playerInstance.GetCardIndex(gameObject));
        }
    }

    private void SwapHandCards()
    {
        if(Input.mousePosition.y < Screen.height * 0.365f)
        {
            RectTransform rightCard = playerInstance.GetCardIndex(gameObject) < playerInstance.GetHand().Count - 1 ? playerInstance.GetHand()[playerInstance.GetCardIndex(gameObject) + 1].GetComponent<RectTransform>() : null;
            RectTransform leftCard = playerInstance.GetCardIndex(gameObject) > 0 ? playerInstance.GetHand()[playerInstance.GetCardIndex(gameObject) - 1].GetComponent<RectTransform>() : null;
            if (Input.GetAxis("Mouse X") > 0 && rightCard is not null)
            {
                if (Input.mousePosition.x > rightCard.TransformPoint(rightCard.rect.center).x - 100f)
                {
                    emptySpace.transform.SetParent(hand);
                    emptySpace.SetActive(true);
                    int temp = playerInstance.GetCardIndex(gameObject) + 1;
                    playerInstance.RemoveCard(gameObject);
                    playerInstance.AddCardOnIndex(temp, gameObject);
                    emptySpace.transform.SetSiblingIndex(playerInstance.GetCardIndex(gameObject));
                }
            }
            else if(leftCard is not null)
            {
                if (Input.mousePosition.x < leftCard.TransformPoint(leftCard.rect.center).x + 100f)
                {
                    emptySpace.transform.SetParent(hand);
                    emptySpace.SetActive(true);
                    int temp = playerInstance.GetCardIndex(gameObject) - 1;
                    playerInstance.RemoveCard(gameObject);
                    playerInstance.AddCardOnIndex(temp, gameObject);
                    emptySpace.transform.SetSiblingIndex(playerInstance.GetCardIndex(gameObject));
                }
            }
        }
        else
        {
            emptySpace.transform.SetParent(null);
            emptySpace.SetActive(false);
        }
    }
}

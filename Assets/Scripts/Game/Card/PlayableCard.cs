using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//represents a ingame Playable card
[RequireComponent(typeof(Image))]
public class PlayableCard : Card, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private string[] data;

    private Transform hand = null;
    private GamePlayer player = null;

    private Vector3 velocity = Vector3.zero;

    private GameObject emptySpace; //simulates card position in hand while being dragged

    public void SetCard(string[] data, bool flag, Transform hand, GamePlayer player)
    {
        GetComponent<LayoutElement>().preferredWidth = width;
        GetComponent<LayoutElement>().preferredHeight = height;

        this.data = data;

        this.hand = hand;
        this.player = player;

        foreach (Image image in GetComponentsInChildren<Image>())
            if (image.gameObject.name == "Sprite") image.sprite = Resources.Load<Sprite>($"Ciudadelas/Distritos/{data[1]}");
        GetComponent<Button>().enabled = flag;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        //Increase dragged card size & removes its anchors
        transform.SetParent(hand.parent);
        SetCardSize(width, height);

        //Empty space that remembers the cards hand position
        emptySpace = new GameObject();
        emptySpace.AddComponent<Image>();
        emptySpace.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        emptySpace.AddComponent<LayoutElement>();
        emptySpace.GetComponent<LayoutElement>().preferredWidth = 80f;
        emptySpace.transform.SetParent(hand);
        emptySpace.transform.SetSiblingIndex(player.GetCardIndex(gameObject));
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        hand.gameObject.GetComponent<Animator>().SetBool("show", true); //show hand while dragging card

        //Drag card
        RectTransform rect = transform as RectTransform;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rect, eventData.position, eventData.pressEventCamera, out var globalMousePosition))
        {
            rect.position = Vector3.SmoothDamp(rect.position, globalMousePosition, ref velocity, .015f); //.01f works fine
        }
        SwapHandCards();
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        //Drop card
        Destroy(emptySpace);
        transform.SetParent(hand);
        transform.SetSiblingIndex(player.GetCardIndex(gameObject));

        if (GetComponent<Button>().enabled && Input.mousePosition.y > Screen.height * 0.365f) //Play card
        {
            hand.gameObject.GetComponent<Animator>().SetBool("show", false);
            if (player.PlayCard(data))
            {
                player.RemoveCard(gameObject);
                Destroy(gameObject);
            }
            else Debug.Log("No hay suficiente dinero");
        }
    }

    private void SwapHandCards()
    {
        if(Input.mousePosition.y < Screen.height * 0.365f) //Animations will be shown while mouse is near hand
        {
            //Detect if dragged card has cards to his right
            RectTransform rightCard = player.GetCardIndex(gameObject) < player.GetHand().Count - 1 ? player.GetHand()[player.GetCardIndex(gameObject) + 1].GetComponent<RectTransform>() : null;
            //Detect if dragged card has cards to his left
            RectTransform leftCard = player.GetCardIndex(gameObject) > 0 ? player.GetHand()[player.GetCardIndex(gameObject) - 1].GetComponent<RectTransform>() : null;

            if (Input.GetAxis("Mouse X") > 0 && rightCard is not null) // if mouse is moving to the right & exists card on the right
            {
                if (Input.mousePosition.x > rightCard.TransformPoint(rightCard.rect.center).x - 100f)// Swap card position to his right
                {
                    emptySpace.transform.SetParent(hand);
                    emptySpace.SetActive(true);
                    int temp = player.GetCardIndex(gameObject) + 1;
                    player.RemoveCard(gameObject);
                    player.AddCardOnIndex(temp, gameObject);
                    emptySpace.transform.SetSiblingIndex(player.GetCardIndex(gameObject));
                }
            }
            else if(leftCard is not null) // if mouse is moving to the left & exists card on the left
            {
                if (Input.mousePosition.x < leftCard.TransformPoint(leftCard.rect.center).x + 100f)// Swap card position to his left
                {
                    emptySpace.transform.SetParent(hand);
                    emptySpace.SetActive(true);
                    int temp = player.GetCardIndex(gameObject) - 1;
                    player.RemoveCard(gameObject);
                    player.AddCardOnIndex(temp, gameObject);
                    emptySpace.transform.SetSiblingIndex(player.GetCardIndex(gameObject));
                }
            }
        }
        else //Disable animations
        {
            emptySpace.transform.SetParent(null);
            emptySpace.SetActive(false);
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;
using System;

//Game Logic class (All players)
public class GameLogic : MonoBehaviour
{
    //Photon Message constants
    private const byte PHASE_END = 3;
    private const byte GIVE_TURN = 4;
    private const byte GIVE_CARDS = 5;
    private const byte ASK_CARDS = 6;
    private const byte STEAL = 8;
    private const byte ADD_GOLD = 9;
    private const byte CLEAR_CHARACTERS = 10;
    private const byte RETURN_CARD = 11;

    //UI
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private RectTransform optionSelector; //RecTransform that will be used as button container
    [SerializeField] private RectTransform discarded;
    [SerializeField] private RectTransform hand; //RecTransform that will be used as button container
    [SerializeField] private GameObject cardPrefab; //card prefab

    [SerializeField] private Button showHideButton;
    [SerializeField] private Button endButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Image characterUI;

    private Dictionary<Sprite, string> characters; //Translates sprite ID to Name

    private bool turn = false;

    private bool _byPassCardSelection = false;

    private void Start()
    {
        characters = new(){
            { Resources.Load<Sprite>("Ciudadelas/Personajes/1"), "Asesino" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/2"), "Ladron" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/3"), "Mago" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/4"), "Rey" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/5"), "Obispo" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/6"), "Mercader" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/7"), "Arquitecto" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/8"), "Guerrero" },
            { Resources.Load<Sprite>("Ciudadelas/Personajes/9"), "Artista" },
        };
    }

    /// <summary>
    /// Generates character selection buttons
    /// </summary>
    /// <param name="selected"></param>
    private void showCharacterSelection(string[] selected)
    {
        selectionPanel.SetActive(true);
        optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Selecciona personaje";
        discarded.parent.gameObject.SetActive(true);
        foreach (Sprite sprite in characters.Keys)
        {
            if (!selected.ToList().Contains(sprite.name)) //instantiate only available characters to select
            {
                GameObject g = GenerateCard(80, 115, true, sprite, optionSelector);
                g.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GetComponent<GamePlayer>().character = sprite; //Set as selected character for current round
                    gameObject.AddComponent(Type.GetType(characters[sprite])); //Generate characters skill script

                    characterUI.sprite = Resources.Load<Sprite>($"Ciudadelas/Personajes/Caratulas/{sprite.name}");

                    RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                    PhotonNetwork.RaiseEvent(PHASE_END, new string[] { GetComponent<GamePlayer>().character.name, PhotonNetwork.LocalPlayer.UserId }, raiseEventOptions, SendOptions.SendUnreliable);
                    CleanOptionSelector();
                    selectionPanel.SetActive(false);
                });
            }
        }
    }

    //Clean optionSelector transform
    public void CleanOptionSelector()
    {
        discarded.parent.gameObject.SetActive(false);
        foreach (RectTransform child in optionSelector)
            Destroy(child.gameObject);
        foreach (RectTransform child in discarded)
            Destroy(child.gameObject);
    }

    //Turn Logic Here
    public void StartTurn()
    {
        //ENABLE ALL TURN ACTIVE UI
        endButton.gameObject.SetActive(true);
        selectionPanel.SetActive(false);
        showHideButton.gameObject.SetActive(false);
        foreach (GameObject card in GetComponent<GamePlayer>().GetHand())
            card.GetComponent<Button>().enabled = true;
    }

    public void EndTurn()
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(PHASE_END, "", raiseEventOptions, SendOptions.SendUnreliable);

        //DISABLE ALL TURN ACTIVE UI
        endButton.gameObject.SetActive(false);
        skillButton.gameObject.SetActive(false);
        Destroy(GetComponent<Character>());
        foreach (GameObject card in GetComponent<GamePlayer>().GetHand())
            card.GetComponent<Button>().enabled = false;

        GetComponent<GamePlayer>().EndTurn();
        turn = false;
    }

    public void setTurn(bool turn)
    {
        this.turn = turn;
    }

    private void PerformSkill()
    {
        if (GetComponent<Character>().isPassive())
            GetComponent<Character>().setSkill(optionSelector, selectionPanel, cardPrefab, characters.Keys.ToArray());
        else
        {
            skillButton.gameObject.SetActive(true);
            StartTurn();
        }
    }

    private GameObject GenerateCard(float width, float height, bool selectable, Sprite sprite, Transform parent)
    {
        GameObject g = Instantiate(cardPrefab);
        g.AddComponent<Card>();
        g.GetComponent<Card>().SetCard(width, height, selectable);

        foreach (Image image in g.GetComponentsInChildren<Image>())
            if (image.gameObject.name == "Sprite") {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
        g.transform.SetParent(parent);
        g.transform.localScale = new Vector3(1, 1, 1);
        return g;
    }

    public void ShowHideSelectionPanel()
    {
        selectionPanel.SetActive(!selectionPanel.activeInHierarchy);
        if(selectionPanel.activeInHierarchy)
            showHideButton.GetComponentInChildren<Text>().text = "Ocultar";
        else
            showHideButton.GetComponentInChildren<Text>().text = "Mostrar";
    }

    public void byPassCardSelection()
    {
        _byPassCardSelection = true;
    }

    private void OnEnable()
	{
		PhotonNetwork.NetworkingClient.EventReceived += NetworkingClient_EventReceived;
	}

	private void OnDisable()
	{
		PhotonNetwork.NetworkingClient.EventReceived -= NetworkingClient_EventReceived;
	}

	private void NetworkingClient_EventReceived(EventData obj)
	{
		if (obj.Code == GIVE_TURN)
        {
            object data = obj.CustomData;
            if (((string[])data)[0] == PhotonNetwork.LocalPlayer.UserId){
                string phase = ((string[])data)[1];

                //Generate Character selection
                if (phase == "phase1") {
                    List<string> discardedCharacters = new();
                    for (int x = 2; x < ((string[])data).Length; x++)
                    {
                        discardedCharacters.Add(((string[])data)[x]);
                        if (x == 2) GenerateCard(30, 47, false, Resources.Load<Sprite>("Ciudadelas/Dorso"), discarded);
                        else GenerateCard(30, 47, false, characters.Keys.ToArray()[int.Parse(((string[])data)[x]) - 1], discarded);
                    }

                    showCharacterSelection(discardedCharacters.ToArray());
                }
                else
                {
                    turn = true;
                    selectionPanel.SetActive(true);
                    showHideButton.gameObject.SetActive(true);
                    showHideButton.GetComponentInChildren<Text>().text = "Ocultar";
                    optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Elige una";
                    //GOLD OPTION
                    GameObject gold = GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/gold"), optionSelector);
                    gold.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        CleanOptionSelector();
                        GetComponent<GamePlayer>().AddGold(2);
                        PerformSkill();
                    });

                    //CARDS OPTION
                    GameObject cards = GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/card"), optionSelector);
                    cards.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        CleanOptionSelector();
                        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                        PhotonNetwork.RaiseEvent(ASK_CARDS, new string[] { PhotonNetwork.LocalPlayer.UserId, "2" }, raiseEventOptions, SendOptions.SendUnreliable);
                    });
                }
            }
        }

        if (obj.Code == GIVE_CARDS) //Receive cards
        {
            object[] data = (object[])obj.CustomData;
            if ((string)data[0] == PhotonNetwork.LocalPlayer.UserId)
            {
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                if (turn && !_byPassCardSelection)
                {
                    selectionPanel.SetActive(true);
                    optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Selecciona una carta";

                    GameObject card1 = GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[1])[1]}"), optionSelector);
                    card1.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[1]), turn, hand, GetComponent<GamePlayer>());
                        g.transform.SetParent(hand);
                        g.transform.localScale = new Vector3(1, 1, 1);
                        GetComponent<GamePlayer>().AddCard(g);
                        PhotonNetwork.RaiseEvent(RETURN_CARD, (string)data[2], raiseEventOptions, SendOptions.SendUnreliable);
                        CleanOptionSelector();
                        PerformSkill();
                    });

                    GameObject card2 = GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[2])[1]}"), optionSelector);
                    card2.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[2]), turn, hand, GetComponent<GamePlayer>());
                        g.transform.SetParent(hand);
                        g.transform.localScale = new Vector3(1, 1, 1);
                        GetComponent<GamePlayer>().AddCard(g);
                        PhotonNetwork.RaiseEvent(RETURN_CARD, (string)data[1] , raiseEventOptions, SendOptions.SendUnreliable);
                        CleanOptionSelector();
                        PerformSkill();
                    });
                }
                else
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        //Instantiate cards on hand
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[i]), turn, hand, GetComponent<GamePlayer>());
                        g.transform.SetParent(hand);
                        g.transform.localScale = new Vector3(1, 1, 1);
                        GetComponent<GamePlayer>().AddCard(g);
                    }
                    _byPassCardSelection = false;
                }
            }
        }

        if (obj.Code == STEAL) // Called when player has been stolen
        {
            object[] data = (object[])obj.CustomData;
            if(GetComponent<GamePlayer>().character.name == (string)data[0])
            {
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(ADD_GOLD, new string[] { (string)data[1], GetComponent<GamePlayer>().GetGold().ToString()}, raiseEventOptions, SendOptions.SendUnreliable);
                GetComponent<GamePlayer>().AddGold(-GetComponent<GamePlayer>().GetGold());
            }
        }

        if(obj.Code == ADD_GOLD) // Called when players gold increases
        {
            object[] data = (object[])obj.CustomData;
            if (PhotonNetwork.LocalPlayer.UserId == (string)data[0])
                GetComponent<GamePlayer>().AddGold(int.Parse((string)data[1]));
        }

        if (obj.Code == CLEAR_CHARACTERS)
        {
            if(TryGetComponent(out Character character))
                Destroy(GetComponent<Character>());
            characterUI.sprite = Resources.Load<Sprite>("Ciudadelas/Personajes/Caratulas/empty");
        }

    }
}

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;

public class GameLogicNetworking : MonoBehaviour
{
    private const byte GIVE_TURN = 4;
    private const byte GIVE_CARDS = 5;
    private const byte ASK_CARDS = 6;
    private const byte STEAL = 8;
    private const byte ADD_GOLD = 9;
    private const byte CLEAR_CHARACTERS = 10;
    private const byte RETURN_CARD = 11;

    private GameLogic gameLogic;

    private bool _byPassCardSelection = false;

    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button showHideButton;
    [SerializeField] private RectTransform optionSelector; //RecTransform that will be used as button container
    [SerializeField] private RectTransform discarded;
    [SerializeField] private GameObject cardPrefab; //card prefab
    [SerializeField] private RectTransform hand; //RecTransform that will be used as button container
    [SerializeField] private Image characterUI;

    private void Awake()
    {
        gameLogic = GetComponent<GameLogic>();
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
            if (((string[])data)[0] == PhotonNetwork.LocalPlayer.UserId)
            {
                string phase = ((string[])data)[1];

                //Generate Character selection
                if (phase == "phase1")
                {
                    List<string> discardedCharacters = new();
                    for (int x = 2; x < ((string[])data).Length; x++)
                    {
                        discardedCharacters.Add(((string[])data)[x]);
                        if (x == 2) gameLogic.GenerateCard(30, 47, false, Resources.Load<Sprite>("Ciudadelas/Dorso"), discarded);
                        else gameLogic.GenerateCard(30, 47, false, gameLogic.getCharacters().Keys.ToArray()[int.Parse(((string[])data)[x]) - 1], discarded);
                    }

                    gameLogic.showCharacterSelection(discardedCharacters.ToArray());
                }
                else
                {
                    gameLogic.setTurn(true);
                    selectionPanel.SetActive(true);
                    showHideButton.gameObject.SetActive(true);
                    showHideButton.GetComponentInChildren<Text>().text = "Ocultar";
                    optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Elige una";
                    //GOLD OPTION
                    GameObject gold = gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/gold"), optionSelector);
                    gold.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        gameLogic.CleanOptionSelector();
                        GetComponent<GamePlayer>().AddGold(2);
                        gameLogic.PerformSkill();
                    });

                    //CARDS OPTION
                    GameObject cards = gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/card"), optionSelector);
                    cards.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        gameLogic.CleanOptionSelector();
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
                if (gameLogic.getTurn() && !_byPassCardSelection)
                {
                    selectionPanel.SetActive(true);
                    optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Selecciona una carta";

                    GameObject card1 = gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[1])[1]}"), optionSelector);
                    card1.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[1]), gameLogic.getTurn(), hand, GetComponent<GamePlayer>());
                        g.transform.SetParent(hand);
                        g.transform.localScale = new Vector3(1, 1, 1);
                        GetComponent<GamePlayer>().AddCard(g);
                        PhotonNetwork.RaiseEvent(RETURN_CARD, (string)data[2], raiseEventOptions, SendOptions.SendUnreliable);
                        gameLogic.CleanOptionSelector();
                        gameLogic.PerformSkill();
                    });

                    GameObject card2 = gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[2])[1]}"), optionSelector);
                    card2.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[2]), gameLogic.getTurn(), hand, GetComponent<GamePlayer>());
                        g.transform.SetParent(hand);
                        g.transform.localScale = new Vector3(1, 1, 1);
                        GetComponent<GamePlayer>().AddCard(g);
                        PhotonNetwork.RaiseEvent(RETURN_CARD, (string)data[1], raiseEventOptions, SendOptions.SendUnreliable);
                        gameLogic.CleanOptionSelector();
                        gameLogic.PerformSkill();
                    });
                }
                else
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        //Instantiate cards on hand
                        GameObject g = Instantiate(cardPrefab);
                        g.AddComponent<PlayableCard>();
                        g.GetComponent<PlayableCard>().SetCard(Deck.Search((string)data[i]), gameLogic.getTurn(), hand, GetComponent<GamePlayer>());
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
            if (GetComponent<GamePlayer>().character.name == (string)data[0])
            {
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                PhotonNetwork.RaiseEvent(ADD_GOLD, new string[] { (string)data[1], GetComponent<GamePlayer>().GetGold().ToString() }, raiseEventOptions, SendOptions.SendUnreliable);
                GetComponent<GamePlayer>().AddGold(-GetComponent<GamePlayer>().GetGold());
            }
        }

        if (obj.Code == ADD_GOLD) // Called when players gold increases
        {
            object[] data = (object[])obj.CustomData;
            if (PhotonNetwork.LocalPlayer.UserId == (string)data[0])
                GetComponent<GamePlayer>().AddGold(int.Parse((string)data[1]));
        }

        if (obj.Code == CLEAR_CHARACTERS)
        {
            if (TryGetComponent(out Character character))
                Destroy(GetComponent<Character>());
            characterUI.sprite = Resources.Load<Sprite>("Ciudadelas/Personajes/Caratulas/empty");
        }

    }
}

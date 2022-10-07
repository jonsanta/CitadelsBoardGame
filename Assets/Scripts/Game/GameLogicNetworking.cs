using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;

public class GameLogicNetworking : MonoBehaviour
{
    //Photon Message constants
    private const byte GIVE_TURN = 4;
    private const byte GIVE_CARDS = 5;
    private const byte ASK_CARDS = 6;
    private const byte STEAL = 8;
    private const byte ADD_GOLD = 9;
    private const byte CLEAR_CHARACTERS = 10;

    private bool _byPassCardSelection = false;

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
        GameLogic _gameLogic = GetComponent<GameLogic>();

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
                        if (x == 2) _gameLogic.GenerateCard(30, 47, false, Resources.Load<Sprite>("Ciudadelas/Dorso"), true);
                        else _gameLogic.GenerateCard(30, 47, false, _gameLogic.getCharacters().Keys.ToArray()[int.Parse(((string[])data)[x]) - 1], true);
                    }

                    _gameLogic.showCharacterSelection(discardedCharacters.ToArray());
                }
                else
                {
                    _gameLogic.setTurn(true);
                    _gameLogic.SetUI("Elige una");
                    //GOLD OPTION
                    GameObject gold = _gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/gold"), false);
                    gold.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        _gameLogic.CleanOptionSelector();
                        GetComponent<GamePlayer>().AddGold(2);
                        _gameLogic.PerformSkill();
                    });

                    //CARDS OPTION
                    GameObject cards = _gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/card"), false);
                    cards.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        _gameLogic.CleanOptionSelector();
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
                if (_gameLogic.getTurn() && !_byPassCardSelection)
                {
                    _gameLogic.SetUI("Selecciona una carta");
                    GameObject card1 = _gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[1])[1]}"), false);
                    card1.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = _gameLogic.GeneratePlayableCard(Deck.Search((string)data[1]), true);
                        GetComponent<GamePlayer>().AddCard(g);
                    });

                    GameObject card2 = _gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[2])[1]}"), false);
                    card2.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = _gameLogic.GeneratePlayableCard(Deck.Search((string)data[2]), true);
                        GetComponent<GamePlayer>().AddCard(g);
                    });
                }
                else
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        GameObject g = _gameLogic.GeneratePlayableCard(Deck.Search((string)data[i]), false);
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
            _gameLogic.ClearCharacter();
        }

    }
}

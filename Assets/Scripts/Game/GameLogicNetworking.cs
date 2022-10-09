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
    private const byte RETURN_CARD = 11;

    private bool _byPassCardSelection = false; //True will ask player to select a card - False will give the cards directly to hand.

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

    private void ReturnCard(GameLogic gameLogic, string data) //Returns discarded card to deck
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(RETURN_CARD, data, raiseEventOptions, SendOptions.SendUnreliable);
        gameLogic.CleanOptionSelector();
    }

    private void NetworkingClient_EventReceived(EventData obj)
    {
        GameLogic gameLogic = GetComponent<GameLogic>();

        if (obj.Code == GIVE_TURN)
        {
            object data = obj.CustomData;
            if (((string[])data)[0] == PhotonNetwork.LocalPlayer.UserId)
            {
                string phase = ((string[])data)[1];

                if (phase == "phase1")
                {
                    //GENERATE CHARACTER SELECTION SCREEN
                    gameLogic.SetUI("Elige un personaje", false);
                    List<string> discardedCharacters = new();
                    for (int x = 2; x < ((string[])data).Length; x++)
                    {
                        discardedCharacters.Add(((string[])data)[x]);
                        if (x == 2) gameLogic.GenerateCard(30, 47, false, Resources.Load<Sprite>("Ciudadelas/Dorso"), true);
                        else gameLogic.GenerateCard(30, 47, false, gameLogic.getCharacters().Keys.ToArray()[int.Parse(((string[])data)[x]) - 1], true);
                    }

                    gameLogic.showCharacterSelection(discardedCharacters.ToArray());
                }
                else
                {
                    //GENERATE GOLD OR CARD SELECTION SCREEN
                    gameLogic.setTurn(true);
                    gameLogic.SetUI("Elige una", true);
                    //GOLD OPTION
                    GameObject gold = gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/gold"), false);
                    gold.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        gameLogic.CleanOptionSelector();
                        GetComponent<GamePlayer>().AddGold(2);
                        gameLogic.PerformSkill();
                    });

                    //CARD OPTION
                    GameObject cards = gameLogic.GenerateCard(60, 90, true, Resources.Load<Sprite>("Ciudadelas/card"), false);
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
                if (gameLogic.getTurn() && !_byPassCardSelection) //Show 2 cards to player --> Player will select one card and return the discarded one
                {
                    gameLogic.SetUI("Selecciona una carta", true);
                    GameObject card1 = gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[1])[1]}"), false);
                    card1.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = gameLogic.GeneratePlayableCard(Deck.Search((string)data[1]));
                        GetComponent<GamePlayer>().AddCard(g);
                        ReturnCard(gameLogic, (string)data[2]);
                        gameLogic.PerformSkill();
                    });

                    GameObject card2 = gameLogic.GenerateCard(80, 115, true, Resources.Load<Sprite>($"Ciudadelas/Distritos/{Deck.Search((string)data[2])[1]}"), false);
                    card2.GetComponent<Button>().onClick.AddListener(() =>
                    {
                        GameObject g = gameLogic.GeneratePlayableCard(Deck.Search((string)data[2]));
                        GetComponent<GamePlayer>().AddCard(g);
                        ReturnCard(gameLogic, (string)data[1]);
                        gameLogic.PerformSkill();
                    });
                }
                else //Give cards directly to players hand
                {
                    for (int i = 1; i < data.Length; i++)
                    {
                        GameObject g = gameLogic.GeneratePlayableCard(Deck.Search((string)data[i]));
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

        if (obj.Code == ADD_GOLD) // Called when player receives Gold
        {
            object[] data = (object[])obj.CustomData;
            if (PhotonNetwork.LocalPlayer.UserId == (string)data[0])
                GetComponent<GamePlayer>().AddGold(int.Parse((string)data[1]));
        }

        if (obj.Code == CLEAR_CHARACTERS) //Clean character info
        {
            gameLogic.ClearCharacter();
        }

    }
}

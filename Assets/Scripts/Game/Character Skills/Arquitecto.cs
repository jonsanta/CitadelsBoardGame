using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arquitecto : Character
{
    //Photon Message constants
    private const byte ASK_CARDS = 6;

    //has passive skill?
    private const bool passive = true;

    override public bool isPassive()
    {
        return passive;
    }

    override public void setSkill(Sprite[] sprites)
    {
        GetComponent<GameLogicNetworking>().byPassCardSelection(); //ENABLE GIVE CARDS DIRECTLY TO HAND

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
        PhotonNetwork.RaiseEvent(ASK_CARDS, new string[] { PhotonNetwork.LocalPlayer.UserId, "2" }, raiseEventOptions, SendOptions.SendUnreliable);

        GetComponent<GameLogic>().CleanOptionSelector();
        GetComponent<GameLogic>().StartTurn();
    }
}

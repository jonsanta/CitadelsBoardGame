using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

//Represents Thief character skill
public class Ladron : Character
{
    //Photon Message constants
    private const byte STEAL = 8;

    //has passive skill?
    private const bool passive = true;

    override public bool isPassive()
    {
        return passive;
    }

    /// <summary>
    /// Load Skill
    /// </summary>
    /// <param name="sprites">Character Sprites</param>
    /// 
    override public void setSkill(Sprite[] sprites)
    {
        GetComponent<GameLogic>().SetUI("Selecciona el personaje al que quieres robar", true);
        //Generate every character card excepting Assassin and Thief
        for (int i = 2; i < sprites.Length; i++)
        {
            GameObject g = GetComponent<GameLogic>().GenerateCard(60, 90, true, sprites[i], false);
            g.GetComponent<Button>().onClick.AddListener(() =>
            {
                //Steal selected character's gold
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
                foreach (Image image in g.GetComponentsInChildren<Image>())
                    if (image.gameObject.name == "Sprite")
                        PhotonNetwork.RaiseEvent(STEAL, new string[] { image.sprite.name, PhotonNetwork.LocalPlayer.UserId }, raiseEventOptions, SendOptions.SendUnreliable);
                GetComponent<GameLogic>().CleanOptionSelector();
                GetComponent<GameLogic>().StartTurn();
                Destroy(GetComponent<Ladron>());
            });
        }
    }
}

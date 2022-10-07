using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

//Represents Assassin character skill
public class Asesino : Character
{
    //Photon Message constants
    private const byte KILL = 7;

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
    override public void setSkill(Sprite[] sprites)
    {
        GetComponent<GameLogic>().SetUI("Selecciona el personaje que quieres matar");
        //Generate every character card excepting Assassin
        for (int i = 1; i < sprites.Length; i++)
        {
            GameObject g = GetComponent<GameLogic>().GenerateCard(60, 90, true, sprites[i], false);
            g.GetComponent<Button>().onClick.AddListener(() =>
            {
                //selected character will lose his turn
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                foreach (Image image in g.GetComponentsInChildren<Image>())
                    if (image.gameObject.name == "Sprite")
                        PhotonNetwork.RaiseEvent(KILL, image.sprite.name, raiseEventOptions, SendOptions.SendUnreliable);
                GetComponent<GameLogic>().CleanOptionSelector();
                GetComponent<GameLogic>().StartTurn();
                Destroy(GetComponent<Asesino>());
            });
        }
    }
}

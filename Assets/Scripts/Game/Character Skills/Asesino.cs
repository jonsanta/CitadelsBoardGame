using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

//Represents Assassin character skill
public class Asesino : MonoBehaviour
{
    //Photon Message constants
    private const byte KILL = 7;

    /// <summary>
    /// Load Skill
    /// </summary>
    /// <param name="optionSelector">Content RectTransform where prefabs will be instantiated</param>
    /// <param name="characterSelectPrefab">Character card prefab</param>
    /// <param name="sprites">Character Sprites</param>
    public void setSkill(RectTransform optionSelector, GameObject selectionPanel, GameObject characterSelectPrefab, Sprite[] sprites)
    {
        selectionPanel.SetActive(true);
        optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = "Selecciona el personaje que quieres matar";
        //Generate every character card excepting Assassin
        for (int i = 1; i < sprites.Length; i++)
        {
            GameObject g = Instantiate(characterSelectPrefab);
            g.AddComponent<Card>();
            g.GetComponent<Card>().SetCard(60, 90, true);
            foreach (Image image in g.GetComponentsInChildren<Image>())
                if (image.gameObject.name == "Sprite") image.sprite = sprites[i];
            g.transform.SetParent(optionSelector);
            g.transform.localScale = new Vector3(1, 1, 1);
            g.GetComponent<Button>().onClick.AddListener(() =>
            {
                //selected character will lose his turn
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient };
                foreach(Image image in g.GetComponentsInChildren<Image>())
                    if (image.gameObject.name == "Sprite")
                        PhotonNetwork.RaiseEvent(KILL, image.sprite.name, raiseEventOptions, SendOptions.SendUnreliable);
                GetComponent<GameLogic>().CleanOptionSelector();
                GetComponent<GameLogic>().StartTurn();
                Destroy(GetComponent<Asesino>());
            });
        }
    }
}

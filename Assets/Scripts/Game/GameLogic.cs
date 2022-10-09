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

    //Prefabs
    [SerializeField] private GameObject cardPrefab; //card prefab

    //Transforms
    [SerializeField] private RectTransform optionSelector; //RecTransform that will be used as button container
    [SerializeField] private RectTransform discarded;
    [SerializeField] private RectTransform hand; //RecTransform that will be used as button container

    //UI
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Button showHideButton;
    [SerializeField] private Button endButton;
    [SerializeField] private Button skillButton;
    [SerializeField] private Image characterUI;

    private Dictionary<Sprite, string> characters; //Translates sprite ID to Name

    private bool turn = false;

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

    public bool getTurn()
    {
        return turn;
    }

    public void setTurn(bool turn)
    {
        this.turn = turn;
    }

    /// <summary>
    /// Generates character selection buttons
    /// </summary>
    /// <param name="selected"></param>
    /// Contains unavailable characters
    public void showCharacterSelection(string[] selected)
    {
        foreach (Sprite sprite in characters.Keys)
        {
            if (!selected.ToList().Contains(sprite.name)) //instantiate only available characters to select
            {
                GameObject g = GenerateCard(80, 115, true, sprite, false);
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

    public void PerformSkill()
    {
        if (GetComponent<Character>().isPassive())
            GetComponent<Character>().setSkill(characters.Keys.ToArray());
        else
        {
            skillButton.gameObject.SetActive(true);
            StartTurn();
        }
    }

    /// <summary>
    /// Generates UI card
    /// </summary>
    /// <param name="width"></param> card width size
    /// <param name="height"></param> card height size
    /// <param name="selectable"></param> clicable card?
    /// <param name="sprite"></param> card sprite
    /// <param name="discard"></param> true -> discarded RecTransform, false -> optionSelector RectTransform
    public GameObject GenerateCard(float width, float height, bool selectable, Sprite sprite, bool discard)
    {
        GameObject g = Instantiate(cardPrefab);
        g.AddComponent<Card>();
        g.GetComponent<Card>().SetCard(width, height, selectable);

        foreach (Image image in g.GetComponentsInChildren<Image>())
            if (image.gameObject.name == "Sprite") {
                image.sprite = sprite;
                image.preserveAspect = true;
            }
        g.transform.SetParent(discard == true ? discarded : optionSelector);
        g.transform.localScale = new Vector3(1, 1, 1);
        return g;
    }

    /// <summary>
    /// Generates Hand card
    /// </summary>
    /// <param name="data"></param> Contains data about the card (id, name, price, colour, sprite)
    public GameObject GeneratePlayableCard(string[] data)
    {
        GameObject g = Instantiate(cardPrefab);
        g.AddComponent<PlayableCard>();
        g.GetComponent<PlayableCard>().SetCard(data, turn, hand, GetComponent<GamePlayer>());
        g.transform.SetParent(hand);
        g.transform.localScale = new Vector3(1, 1, 1);
        return g;
    }

    public Dictionary<Sprite, string> getCharacters()
    {
        return characters;
    }

    public void ClearCharacter()
    {
        if (TryGetComponent(out Character character)) //Destroy character script if exists
            Destroy(GetComponent<Character>());
        characterUI.sprite = Resources.Load<Sprite>("Ciudadelas/Personajes/Caratulas/empty"); //Set empty character sprite
    }

    public void ShowHideSelectionPanel()
    {
        selectionPanel.SetActive(!selectionPanel.activeInHierarchy);
        if (selectionPanel.activeInHierarchy)
            showHideButton.GetComponentInChildren<Text>().text = "Ocultar";
        else
            showHideButton.GetComponentInChildren<Text>().text = "Mostrar";
    }

    /// <summary>
    /// Enables Selection panel UI
    /// </summary>
    /// <param name="text"></param> selection panel text --> will be shown to player
    /// <param name="hideOption"></param> //Panel can be hidden or not
    public void SetUI(string text, bool hideOption)
    {
        selectionPanel.SetActive(true);
        optionSelector.parent.parent.gameObject.GetComponentInChildren<Text>().text = text;

        if (hideOption)
        {
            showHideButton.gameObject.SetActive(true);
            showHideButton.GetComponentInChildren<Text>().text = "Ocultar";
        }
        else discarded.parent.gameObject.SetActive(true); //this line might need some rework
    }
}

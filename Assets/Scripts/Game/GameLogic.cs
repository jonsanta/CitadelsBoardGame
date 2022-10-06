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
    public void showCharacterSelection(string[] selected)
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

    public bool getTurn()
    {
        return turn;
    }

    public void PerformSkill()
    {
        if (GetComponent<Character>().isPassive())
            GetComponent<Character>().setSkill(optionSelector, selectionPanel, cardPrefab, characters.Keys.ToArray());
        else
        {
            skillButton.gameObject.SetActive(true);
            StartTurn();
        }
    }

    public GameObject GenerateCard(float width, float height, bool selectable, Sprite sprite, Transform parent)
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

    public Dictionary<Sprite, string> getCharacters()
    {
        return characters;
    }
}

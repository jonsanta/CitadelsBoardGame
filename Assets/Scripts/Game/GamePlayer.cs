using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Saves player data (selected character, cards in hand, current gold)
public class GamePlayer : MonoBehaviour
{
    public Sprite character { get; set;}
    private List<GameObject> hand = new();
    private List<GameObject> table = new();
    private int gold = 2;
    private int playedCards = 0;

    [SerializeField] private GameObject CardPrefab;
    [SerializeField] private GameObject playedCardPrefab;

    [SerializeField] private Text goldUI;

    public void AddCard(GameObject card)
    {
        hand.Add(card);
    }

    public void AddCardOnIndex(int index, GameObject card)
    {
        hand.Insert(index, card);
    }

    public void RemoveCard(GameObject card)
    {
        hand.Remove(card);
    }

    public bool PlayCard(string[] data)
    {
        int limit = character.name == "7" ? 3 : 1;
        //PLAY CARD
        if (gold >= int.Parse(data[2]) && playedCards < limit)
        {
            GameObject card = Instantiate(playedCardPrefab);
            card.GetComponent<PlayedCard>().SetCard(data);
            table.Add(card);
            AddGold(-int.Parse(data[2]));
            playedCards++;
            return true;
        }
        else return false;
    }

    public int GetCardIndex(GameObject card)
    {
        return hand.IndexOf(card);
    }

    public List<GameObject> GetHand()
    {
        return hand;
    }

    public void AddGold(int amount) {
        gold += amount;
        goldUI.text = gold.ToString();
    }

    public int GetGold()
    {
        return gold;
    }

    public void EndTurn()
    {
        playedCards = 0;
    }
}

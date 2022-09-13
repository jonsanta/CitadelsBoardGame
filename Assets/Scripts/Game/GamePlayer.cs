using System.Collections.Generic;
using UnityEngine;

//Saves player data (selected character, cards in hand, current gold)
public class GamePlayer
{
    public Sprite character { get; set;}
    private List<GameObject> cards = new();
    private int gold = 2;

    public void AddCard(GameObject card)
    {
        cards.Add(card);
    }

    public void RemoveCard(GameObject card)
    {
        cards.Remove(card);
    }

    public int GetCardIndex(GameObject card)
    {
        return cards.IndexOf(card);
    }

    public List<GameObject> GetCards()
    {
        return cards;
    }

    public void AddGold(int gold) {
        this.gold += gold;
    }

    public int GetGold()
    {
        return gold;
    }
}

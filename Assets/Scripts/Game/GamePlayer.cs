using System.Collections.Generic;
using UnityEngine;

//Saves player data (selected character, cards in hand, current gold)
public class GamePlayer
{
    public Sprite character { get; set;}
    private List<GameObject> hand = new();
    private List<GameObject> table = new();
    private int gold = 2;

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

    public void PlayCard(GameObject card)
    {
        //PLAY CARD
        hand.Remove(card);
        GameObject g = new GameObject();
        g.name = card.GetComponent<PlayableCard>().GetCardName();
        table.Add(g);
        MonoBehaviour.Destroy(card);
    }

    public int GetCardIndex(GameObject card)
    {
        return hand.IndexOf(card);
    }

    public List<GameObject> GetHand()
    {
        return hand;
    }

    public void AddGold(int gold) {
        this.gold += gold;
    }

    public int GetGold()
    {
        return gold;
    }
}

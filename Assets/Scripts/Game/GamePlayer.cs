using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Saves player data (selected character, cards in hand, current gold)
public class GamePlayer : MonoBehaviour
{
    public Sprite character { get; set;} //Current selected character
    private List<GameObject> hand = new(); //Cards in hand
    private List<GameObject> table = new(); //Cards in table
    private int gold = 2; //Current player gold
    private int playedCards = 0; //Current turn played cards amount

    [SerializeField] private GameObject playedCardPrefab;

    [SerializeField] private Text goldUI;

    public void AddCard(GameObject card) //Add card to hand
    {
        hand.Add(card);
    }

    public void AddCardOnIndex(int index, GameObject card) //Insert card to hand on index
    {
        hand.Insert(index, card);
    }

    public int GetCardIndex(GameObject card)
    {
        return hand.IndexOf(card);
    }

    public void RemoveCard(GameObject card) //Remove card from hand
    {
        hand.Remove(card);
    }

    public bool PlayCard(string[] data)
    {
        int limit = character.name == "7" ? 3 : 1; //Limit playable cards - 1 card per turn excepting Architect (can play 3 cards per turn) 
        //PLAY CARD
        if (gold >= int.Parse(data[2]) && playedCards < limit) //If player has enough gold && limit is not exceded
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

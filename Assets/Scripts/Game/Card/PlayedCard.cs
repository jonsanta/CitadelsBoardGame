using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayedCard : MonoBehaviour
{
    [SerializeField] private int id; //card id
    [SerializeField] private string cardName; //card name
    [SerializeField] private int points; // card price
    [SerializeField] private string colour; //card colour

    public void SetCard(string[] data)
    {
        id = int.Parse(data[0]);
        cardName = data[1];
        points = int.Parse(data[2]);
        colour = data[3];
    }
}

using System.Xml;
using UnityEngine;
using System.Collections.Generic;

public static class Deck
{
    //Generate card ID list
    public static List<string> Get()
    {
        List<string> cards = new();

        XmlDocument xdoc = new XmlDocument();
        xdoc.LoadXml(Resources.Load<TextAsset>("cards").text); //load XML document from TextAssets

        XmlNodeList elements = xdoc.SelectSingleNode("deck").ChildNodes; //Root element childs

        for(int x = 0; x < elements.Count; x++)
            if (elements[x].NodeType == XmlNodeType.Element)
                for (int i = 0; i < int.Parse(elements[x].Attributes["number"].Value); i++) //Iterate card number of times
                    cards.Add(elements[x].Attributes["id"].Value);//Add ID in list

        return cards;
    }

    //Get card data searching with ID
    public static string[] Search(string id)
    {
        string[] values = new string[4];
        values[0] = id;

        XmlDocument xdoc = new XmlDocument();
        xdoc.LoadXml(Resources.Load<TextAsset>("cards").text); //load XML document from TextAssets

        XmlNodeList elements = xdoc.SelectSingleNode("deck").ChildNodes; //Root element childs

        //Get all card data
        for (int x = 0; x < elements.Count; x++)
            if (elements[x].NodeType == XmlNodeType.Element)
                if (elements[x].Attributes["id"].Value == id)
                    for (int i = 0; i < elements[x].ChildNodes.Count; i++)
                        values[i + 1] = elements[x].ChildNodes[i].InnerText;

        return values;
    }
}

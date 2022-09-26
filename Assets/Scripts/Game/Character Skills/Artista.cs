using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Artista : Character
{
    //has passive skill?
    private const bool passive = false;

    override public bool isPassive()
    {
        return passive;
    }

    public override void setSkill(RectTransform optionSelector, GameObject selectionPanel, GameObject characterSelectPrefab, Sprite[] sprites)
    {
        throw new System.NotImplementedException();
    }
}

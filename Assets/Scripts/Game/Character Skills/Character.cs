using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public abstract bool isPassive();
    public abstract void setSkill(RectTransform optionSelector, GameObject selectionPanel, GameObject characterSelectPrefab, Sprite[] sprites);
}

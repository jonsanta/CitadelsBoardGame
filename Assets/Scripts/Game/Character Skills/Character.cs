using UnityEngine;

public abstract class Character : MonoBehaviour
{
    public abstract bool isPassive(); //Determines character skill type
    public abstract void setSkill(Sprite[] sprites); //sets character's skill
}

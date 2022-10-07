
using UnityEngine;

public class Rey : Character
{

    //has passive skill?
    private const bool passive = false;

    override public bool isPassive()
    {
        return passive;
    }

    public override void setSkill(Sprite[] sprites)
    {
        throw new System.NotImplementedException();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class touchController : MonoBehaviour
{
    public bool fire, jump, aim, bomb, knife, reload, inspect;
    void Start()
    {

    }

    // Update is called once per frame
    public void fireDown()
    {
        fire = true;
    }
    public void fireUp()
    {
        fire = false;

    }
    public void setJump()
    {
        jump = true;
    }
    public void setAim()
    {
        if (aim)
            aim = false;
        else
            aim = true;
    }
    public void setBomb()
    {
        bomb = true;
    }
    public void setKnife()
    {
        knife = true;
    }
    public void setReload()
    {
        reload = true;
    }
    public void setInspect()
    {
        inspect = true;
    }
}

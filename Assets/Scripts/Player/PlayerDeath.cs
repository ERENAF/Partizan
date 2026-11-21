
using UnityEngine;

public class PlayerDeath : Death
{
    public override void death()
    {
        Debug.Log("PlayerDeath");
        Application.Quit();
    }
}

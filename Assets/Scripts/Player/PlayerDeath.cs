
using UnityEngine;

public class PlayerDeath : Death
{
    public override void death()
    {
        Application.Quit();
        Debug.Log("PlayerDeath");
    }
}

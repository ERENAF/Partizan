using UnityEngine;
public class EnemyDeath : Death
{
    public override void death()
    {
        Debug.Log("EnemyDeath");
        Destroy(gameObject,3f);
    }
}

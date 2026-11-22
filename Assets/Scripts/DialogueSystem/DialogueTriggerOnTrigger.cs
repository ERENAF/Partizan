using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTriggerOnTrigger : DialogueTrigger
{
    [Header("Trigger")]
    public string playerTag;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag)
        {
            SetDialogue();
        }
    }
}

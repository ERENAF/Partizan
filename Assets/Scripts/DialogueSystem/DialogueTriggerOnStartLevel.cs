using System.Collections;
using UnityEngine;

public class DialogueTriggerOnStartLevel : DialogueTrigger
{
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        SetDialogue();
    }
}

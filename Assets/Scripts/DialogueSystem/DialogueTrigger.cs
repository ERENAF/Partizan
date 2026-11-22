using UnityEngine;

public abstract class DialogueTrigger : MonoBehaviour
{
    [Header("dialogueSettings")]
    public Dialogue dialogue;
    public DialogueSystem dialogueSystem;
    [SerializeField] protected bool triggerOnlyOnce = true;
    private bool hasTriggered;
    protected void SetDialogue()
    {
        if (dialogueSystem == null || dialogue == null)
        {
            Debug.LogWarning(
                "DialogueTrigger: Dialogue или DialogueSystem не назначены!"
            );
            return;
        }
        if (triggerOnlyOnce && hasTriggered)
            return;

        if (dialogueSystem.IsDialogueActive)
            return;
        dialogueSystem.dialogue = dialogue;
        dialogueSystem.StartDialogue();

        hasTriggered = true;
    }

    private bool IsDialogueActive()
    {
        return dialogue==null || dialogueSystem == null;
    }
}

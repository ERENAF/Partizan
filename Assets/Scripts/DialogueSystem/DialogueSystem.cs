using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class Line
{
    public string heroName;
    public Image heroImage;
    [TextArea(3,6)] public string line;
}
[Serializable]
public class DialogueUnit
{
    public Line[] dialogueLines;
    [Min(0)]public int nextDialogueUnit;
}
[Serializable]
public class DialogueChoice
{
    public string choiceText;

    [Min(-1)]
    public int nextDialogueUnit = -1;
}
public class DialogueSystem : MonoBehaviour
{
    public DialogueUnit[] dialogueUnits;
    [Min(0)] public int currentDialogueUnit;

    public bool CheckNextDialogueUnit()
    {
        return dialogueUnits[currentDialogueUnit].nextDialogueUnit >= dialogueUnits.Length;
    }

    public void NextDialogueUnit()
    {
        if (!CheckNextDialogueUnit())
        {
            Debug.LogWarning("Следующий диалоговый блок не существует!");
            return;
        }
        int nextIndex =
            dialogueUnits[currentDialogueUnit].nextDialogueUnit;

        SwitchDialogueUnit(nextIndex);
    }
    public void SwitchDialogueUnit(int index)
    {
        if (dialogueUnits == null ||
            index < 0 ||
            index >= dialogueUnits.Length)
        {
            Debug.LogWarning($"Диалоговый блок {index} не существует!");
            return;
        }

        currentDialogueUnit = index;

        Debug.Log($"Переключение на диалоговый блок {index}");
    }
}

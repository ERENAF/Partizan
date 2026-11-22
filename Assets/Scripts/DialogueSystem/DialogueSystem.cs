using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UI;
[Serializable]
public class Line
{
    public string heroName;
    public Sprite heroImage;
    [TextArea(3,6)] public string line;
}
[Serializable]
public class DialogueUnit
{
    public Line[] dialogueLines;
    [Min(-1)]public int nextDialogueUnit = -1;
}
[Serializable]
public class Dialogue
{
    public DialogueUnit[] dialogueUnits;
    [Min(0)] public int currentDialogueUnit;
    [NonSerialized]public Line currentLine;
    [NonSerialized, Min(0)] public int currentLineIndex;
    public void StartDialogue()
    {
        currentDialogueUnit = 0;
        currentLineIndex = 0;
        currentLine = null;
    }
    public bool NextLine()
    {
        if (dialogueUnits == null ||
            dialogueUnits.Length == 0)
            return false;

        // Защита от некорректных переходов.
        int transitions = 0;

        while (transitions <= dialogueUnits.Length)
        {
            if (currentDialogueUnit < 0 ||
                currentDialogueUnit >= dialogueUnits.Length)
                return false;

            DialogueUnit unit =
                dialogueUnits[currentDialogueUnit];

            if (unit == null)
                return false;

            // Если в блоке ещё есть реплики.
            if (unit.dialogueLines != null &&
                currentLineIndex < unit.dialogueLines.Length)
            {
                currentLine =
                    unit.dialogueLines[currentLineIndex];

                currentLineIndex++;

                return true;
            }

            // Переходим к следующему блоку.
            int nextIndex = unit.nextDialogueUnit;

            if (nextIndex < 0 ||
                nextIndex >= dialogueUnits.Length)
                return false;

            currentDialogueUnit = nextIndex;
            currentLineIndex = 0;

            transitions++;
        }

        Debug.LogWarning(
            "Обнаружен цикл из пустых диалоговых блоков."
        );

        return false;
    }
}

public class DialogueSystem : MonoBehaviour
{
    [Header("DialogueSettings")]
    public Dialogue dialogue;
    [Min(0)]public float speedTyping;
    [Header("UI")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI textDialogueLine;
    public TextMeshProUGUI textHeroName;
    public Image imageHero;
    public Button buttonNextLine;
    public Button buttonSkipDialogue;

    private Coroutine typingCoroutine;
    private bool isTyping;
    private bool isDialogueActive;
    public bool IsDialogueActive => isDialogueActive;
    private string fullLine;

    private void Awake()
    {
        buttonNextLine.onClick.AddListener(NextLine);
        buttonSkipDialogue.onClick.AddListener(FinishDialogue);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
    }

    public void StartDialogue()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        dialogue.StartDialogue();

        isDialogueActive = true;
        isTyping = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);
        GamePauseManager.Instance.SetPause(true);
        ShowNextLine();
    }

    private void NextLine()
    {
        if (!isDialogueActive)
            return;

        // Если текст печатается — показываем целиком.
        if (isTyping)
        {
            CompleteTyping();
            return;
        }

        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (!dialogue.NextLine())
        {
            FinishDialogue();
            return;
        }

        Line line = dialogue.currentLine;

        if (line == null)
        {
            Debug.LogWarning("Реплика диалога не задана.");
            FinishDialogue();
            return;
        }

        textHeroName.text = line.heroName;

        if (imageHero != null)
        {
            imageHero.sprite = line.heroImage;
            imageHero.enabled = line.heroImage != null;
        }

        fullLine = line.line ?? "";

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(
            TypeLine(fullLine)
        );
    }

    private IEnumerator TypeLine(string text)
    {
        isTyping = true;

        // TextMeshPro позволяет открывать текст
        // посимвольно без изменения строки.
        textDialogueLine.text = text;
        textDialogueLine.maxVisibleCharacters = 0;

        textDialogueLine.ForceMeshUpdate();

        int characterCount =
            textDialogueLine.textInfo.characterCount;

        if (speedTyping <= 0f)
        {
            textDialogueLine.maxVisibleCharacters =
                characterCount;

            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        for (int i = 0; i < characterCount; i++)
        {
            textDialogueLine.maxVisibleCharacters = i + 1;

            yield return new WaitForSecondsRealtime(speedTyping);
        }

        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        textDialogueLine.maxVisibleCharacters =
            int.MaxValue;

        isTyping = false;
    }

    private void FinishDialogue()
    {
        if (!isDialogueActive)
            return;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
        isDialogueActive = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        GamePauseManager.Instance.SetPause(false);
        Debug.Log("Диалог завершён.");
    }

    private void OnDestroy()
    {
        if (buttonNextLine != null)
            buttonNextLine.onClick.RemoveListener(NextLine);

        if (buttonSkipDialogue != null)
            buttonSkipDialogue.onClick.RemoveListener(
                FinishDialogue
            );
    }


}

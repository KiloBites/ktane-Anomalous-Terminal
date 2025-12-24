using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public TextMesh ProgressText, ProgramInfoDisplay, Date, ReadyToUnlockText;
    public KMSelectable[] ProgramButtons;


    private Terminal _terminal;

    private bool programOpened;


    void Awake()
    {
        foreach (KMSelectable button in ProgramButtons)
        {
            button.OnInteract += () => { ButtonPress(button); return false; };
            button.OnHighlight += () => { Highlight(button); };
            button.OnHighlightEnded += () => { EndHighlight(); };
        }
    }

    void Update()
    {
        if (programOpened)
            return;

        Date.text = DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm:ss tt");

        if (ReadyToUnlockText.text.Length > 0)
            return;

        ReadyToUnlockText.text = _terminal.AllProgramsCompleted() ? "The Terminal is ready to be unlocked!" : string.Empty;
    }

    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    public void MarkProgramAsClosed() => programOpened = false;

    public void UpdateProgramCompletions(bool[] completed)
    {
        for (int i = 0; i < 4; i++)
            ProgramButtons[i].GetComponentInChildren<TextMesh>().color = completed[i] ? Color.green : Color.white;
    }

    public void ShowProgress() => ProgressText.text = $"{_terminal.ProgressCount()}/4 Programs Completed";

    void Highlight(KMSelectable button)
    {
        string programName;

        _terminal.GetProgramName(Array.IndexOf(ProgramButtons, button), out programName);

        ProgramInfoDisplay.text = programName;
    }

    void EndHighlight() => ProgramInfoDisplay.text = string.Empty;

    void ButtonPress(KMSelectable button)
    {
        button.AddInteractionPunch(0.2f);

        if (_terminal.AllProgramsCompleted() || _terminal.ProgramIndexCompleted(Array.IndexOf(ProgramButtons, button)))
        {
            _terminal.Module.Audio.PlaySoundAtTransform("AlreadyCompleted", button.transform);
            return;
        }
            
        
        _terminal.OpenProgram(Array.IndexOf(ProgramButtons, button));

        EndHighlight();
        programOpened = true;
    }
}

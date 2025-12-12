using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public TextMesh ProgressText, ProgramInfoDisplay, Date;
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
    }

    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    public void MarkProgramAsClosed() => programOpened = false;

    void Highlight(KMSelectable button)
    {
        string programName;

        _terminal.GetProgramName(Array.IndexOf(ProgramButtons, button), out programName);

        ProgramInfoDisplay.text = programName;
    }

    void EndHighlight() => ProgramInfoDisplay.text = string.Empty;

    void ButtonPress(KMSelectable button)
    {
        if (_terminal.AllProgramsCompleted() || programOpened)
            return;


        _terminal.Module.Audio.PlaySoundAtTransform("OpenProgram", transform);
        _terminal.OpenProgram(Array.IndexOf(ProgramButtons, button));

        EndHighlight();
        programOpened = true;
    }
}

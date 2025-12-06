using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public TextMesh ProgressText, ProgramInfoDisplay;
    public KMSelectable[] ProgramButtons;

    private Terminal _terminal;


    void Awake()
    {
        foreach (KMSelectable button in ProgramButtons)
        {
            button.OnInteract += () => { ButtonPress(button); return false; };
            button.OnHighlight += () => { Highlight(button); };
            button.OnHighlightEnded += () => { EndHighlight(); };
        }
    }

    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void Highlight(KMSelectable button)
    {
        string programName;

        _terminal.GetProgramName(Array.IndexOf(ProgramButtons, button), out programName);

        ProgramInfoDisplay.text = programName;
    }

    void EndHighlight() => ProgramInfoDisplay.text = string.Empty;

    void ButtonPress(KMSelectable button)
    {
        if (_terminal.AllProgramsCompleted())
            return;

        _terminal.OpenProgram(Array.IndexOf(ProgramButtons, button));
    }
}

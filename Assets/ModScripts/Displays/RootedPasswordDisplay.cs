using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Random;

public class RootedPasswordDisplay : MonoBehaviour
{
    public KMSelectable[] Keypad;
    public TextMesh PasswordDisplay;

    private readonly List<int> inputtedNumbers = new List<int>();

    private RootedPassword _program;
    private Terminal _terminal;

    public void AssignProgram(RootedPassword program) => _program = program;
    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void Awake()
    {
        foreach (KMSelectable key in Keypad)
            key.OnInteract += () => { KeypadPress(key); return false; };
    }

    void UpdateDisplay() => PasswordDisplay.text = inputtedNumbers.Join("");

    void KeypadPress(KMSelectable key)
    {
        key.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", key.transform);

        if (_program.ProgramComplete)
            return;

        var ix = Array.IndexOf(Keypad, key);

        switch (ix)
        {
            case 10:
                inputtedNumbers.Clear();
                UpdateDisplay();
                break;
            case 11:
                if (_program.CheckInformation(inputtedNumbers))
                {
                    _terminal.Module.DoLog("The password has been inputted correctly. Program completed.");
                    _program.ProgramComplete = true;
                    _terminal.DoCreepyShit(_program.ProgramIndex);
                }
                else
                {
                    _terminal.Module.DoLog($"The password expected is {_program.DisplayInfo}, but inputted {(inputtedNumbers.Count == 0 ? "nothing" : inputtedNumbers.Join(""))}. Strike!");
                    _terminal.Module.Module.HandleStrike();
                    inputtedNumbers.Clear();
                    UpdateDisplay();
                }
                break;
            default:
                if (inputtedNumbers.Count == 6)
                    return;
                inputtedNumbers.Add(ix);
                UpdateDisplay();
                break;
        }
    }
}

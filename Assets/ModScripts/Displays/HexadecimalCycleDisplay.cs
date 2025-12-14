using System;
using UnityEngine;
using static UnityEngine.Random;

public class HexadecimalCycleDisplay : MonoBehaviour
{
    public KMSelectable[] Keypad;

    public TextMesh MainDisplay;

    private HexCycle _program;
    private Terminal _terminal;

    private string input;

    void Awake()
    {
        foreach (KMSelectable key in Keypad)
            key.OnInteract += () => { KeypadPress(key); return false; };
    }

    public void AssignTerminal(Terminal terminal) => _terminal = terminal;
    public void AssignProgram(HexCycle program) => _program = program;

    public void DisplayEncryptedMessage() => MainDisplay.text = _program.DisplayInfo.WordWrap(20).Join("\n");

    void KeypadPress(KMSelectable key)
    {
        key.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", key.transform);

        if (_program.ProgramComplete)
            return;

        var ix = Array.IndexOf(Keypad, key);

        switch (ix)
        {
            case 16:
                input = string.Empty;
                MainDisplay.text = _program.DisplayInfo.WordWrap(20).Join("\n");
                break;
            case 17:
                input += " ";
                MainDisplay.text = input.WordWrap(20).Join("\n");
                break;
            case 18:
                if (_program.CheckInformation(input))
                {
                    _program.ProgramComplete = true;
                    _terminal.Module.DoLog("The expected hexadecimal encryption is correct. Program completed.");
                    _terminal.DoCreepyShit(_program.ProgramIndex);
                }
                else
                {
                    _terminal.Module.DoLog("The expected hexadecimal encryption isn't correct. Strike!");
                    _terminal.Module.Module.HandleStrike();
                    input = string.Empty;
                    MainDisplay.text = _program.DisplayInfo.WordWrap(20).Join("\n");
                }
                break;
            default:
                _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", key.transform);
                input += "0123456789ABCDEF"[ix];
                MainDisplay.text = input.WordWrap(20).Join("\n");
                break;
        }
    }
}
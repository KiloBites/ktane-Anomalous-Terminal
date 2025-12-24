using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Random;

public class SacrificialHouseDisplay : MonoBehaviour
{
    public KMSelectable[] Keyboard;
    public TextMesh MessageDisplay, CommandDisplay;

    [NonSerialized]
    public Coroutine CurrentlyTyping;

    private SacrificialHouse _program;
    private Terminal _terminal;

    private string message, commandInput = string.Empty;

    private const string keyboardLayout = "QWERTYUIOPASDFGHJKLZXCVBNM";

    public static int GetIndexOfLetter(char c) => keyboardLayout.IndexOf(c);

    public void AssignProgram(SacrificialHouse program) => _program = program;
    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void Awake()
    {
        foreach (KMSelectable key in Keyboard)
            key.OnInteract += () => { KeyPress(key); return false; };
    }

    void Update()
    {
        if (_program.ProgramComplete || CurrentlyTyping != null || _program.KilledOrInvalid)
            return;

        if (_terminal.Module.IsModuleFocused)
        {
            for (var ltr = 0; ltr < 26; ltr++)
                if (Input.GetKeyDown(((char)('a' + ltr)).ToString()))
                    Keyboard[GetIndexOfLetter((char)('A' + ltr))].OnInteract();

            if (Input.GetKeyDown(KeyCode.Backspace))
                Keyboard[27].OnInteract();

            if (Input.GetKeyDown(KeyCode.Return))
                Keyboard[28].OnInteract();

            if (Input.GetKeyDown(KeyCode.Space))
                Keyboard[29].OnInteract();
        }
    }

    public void StartProgram()
    {
        message = _program.ObtainMessage().WordWrap(25).Join("\n");
        CurrentlyTyping = StartCoroutine(TypeMessage(false));
    }

    void KeyPress(KMSelectable key)
    {
        key.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", key.transform);

        if (_program.ProgramComplete || CurrentlyTyping != null || _program.KilledOrInvalid)
            return;

        var ix = Array.IndexOf(Keyboard, key);

        switch (ix)
        {
            case 27:
                if (commandInput.Length > 0)
                    commandInput = commandInput.Remove(commandInput.Length - 1);
                CommandDisplay.text = commandInput;
                break;
            case 28:
                _program.CheckCommandInput(commandInput, out message);
                message = message.WordWrap(25).Join("\n");
                CurrentlyTyping = StartCoroutine(TypeMessage(commandInput.ToUpperInvariant() == "SACRIFICE", commandInput.ToUpperInvariant() == "HIDE"));
                commandInput = string.Empty;
                CommandDisplay.text = string.Empty;
                break;
            case 29:
                if (commandInput.Length == 37)
                    return;
                commandInput += ' ';
                CommandDisplay.text = commandInput;

                break;
            default:
                if (commandInput.Length == 37)
                    return;
                commandInput += keyboardLayout[ix];
                CommandDisplay.text = commandInput;
                break;
        }
    }

    IEnumerator TypeMessage(bool check = false, bool hiding = false)
    {
    reset:

        if (MessageDisplay.text.Length != 0)
            MessageDisplay.text = string.Empty;

        foreach (var c in message)
        {
            MessageDisplay.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        if (check || _program.KilledOrInvalid)
        {
            if (_program.KilledOrInvalid)
            {
                _terminal.Module.DoLog("Either the anomaly killed you, or you have invalid/no items sacrificed. Strike!");
                yield return new WaitForSeconds(1);
                _terminal.ToggleObject(_program.ProgramIndex, false);
                _terminal.ErrorPayload(_program.ProgramIndex, _program.Killed());
                yield return new WaitUntil(() => _terminal.CreepyShit == null);
                _terminal.Module.Module.HandleStrike();
                _program.Reset();
                message = _program.ObtainMessage().WordWrap(25).Join("\n");
                _terminal.ToggleObject(_program.ProgramIndex, true);
                check = false;
                _program.Reset();
                message = _program.ObtainMessage().WordWrap(25).Join("\n");
                goto reset;
            }
            else if (!_program.InvalidSacrifice())
            {
                _terminal.Module.DoLog("All items have been sacrificed. Program completed.");
                _program.ProgramComplete = true;
                _terminal.DoCreepyShit(_program.ProgramIndex);
            }
        }
        else if (hiding)
        {
            yield return new WaitForSeconds(1);
            message = _program.ObtainMessage().WordWrap(25).Join("\n");
            hiding = false;
            goto reset;
        }
            CurrentlyTyping = null;
    }
}
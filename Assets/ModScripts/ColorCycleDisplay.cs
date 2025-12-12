using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColorCycleDisplay : MonoBehaviour
{
    public KMSelectable ColorButton;
    public TextMesh CBText;
    public MeshRenderer ColorRender;

    private Coroutine flash;
    private int flashIx = 0;

    private ColorCycle _program;
    private Terminal _terminal;

    private static readonly Color[] colorOutputs =
    {
        Color.red,
        new Color(1, 1, 0),
        Color.green,
        Color.blue
    };

    public void BeginSequence(List<ATColor> colorSeq) => flash = StartCoroutine(StartFlash(colorSeq));
    public void AssignProgram(ColorCycle program) => _program = program;
    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void Awake() => ColorButton.OnInteract += () => { ButtonPress(); return false; };

    void ButtonPress()
    {
        if (flash != null)
            StopCoroutine(flash);

        if (_program.ProgramComplete)
            return;

        var checkInput = _program.GetInput(flashIx);

        flashIx = 0;

        if (_program.CheckInformation(checkInput))
        {
            _terminal.Module.DoLog($"{(checkInput is int ? $"Position {(int)checkInput + 1}" : $"Color {checkInput}")} has been submitted correctly. Program {_program.ProgramIndex + 1} has been completed.");
            _program.ProgramComplete = true;
        }
        else
        {
            _terminal.Module.DoLog($"Submitted {(checkInput is int ? $"position {(int)checkInput + 1}" : $"color {checkInput.ToString().ToLowerInvariant()}")}, but the answer is {_program.GetWrongInput(checkInput)}. Strike!");
            _terminal.Module.Module.HandleStrike();
            _program.StartFlashingSequence();
        }
    }

    IEnumerator StartFlash(List<ATColor> colorSeq)
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            while (flashIx < 6)
            {
                _terminal.Module.Audio.PlaySoundAtTransform(colorSeq[flashIx].ToString(), transform);
                ColorRender.material.color = colorOutputs[(int)colorSeq[flashIx]];
                CBText.text = _program.NeedColorblind.Value ? colorSeq[flashIx].ToString()[0].ToString() : string.Empty;
                CBText.color = colorSeq[flashIx] == ATColor.Yellow ? Color.black : Color.white;
                yield return new WaitForSeconds(0.4f);
                ColorRender.material.color = Color.black;
                CBText.text = string.Empty;
                yield return new WaitForSeconds(0.4f);
                flashIx++;
            }

            flashIx %= 6;
            yield return new WaitForSeconds(1);
        }
    }

}

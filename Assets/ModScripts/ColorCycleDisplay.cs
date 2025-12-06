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

    private bool programComplete;

    private Coroutine flash;
    private int flashIx = 0;

    private ColorCycle _program;
    private AnomalousTerminalScript _module;

    private static readonly Color[] colorOutputs =
    {
        Color.red,
        new Color(1, 1, 0),
        Color.green,
        Color.blue
    };

    public void BeginSequence(List<ATColor> colorSeq) => flash = StartCoroutine(StartFlash(colorSeq));
    public void AssignProgram(ColorCycle program) => _program = program;
    public void AssignModule(AnomalousTerminalScript module) => _module = module; 

    void ButtonPress()
    {
        if (flash != null)
            StopCoroutine(flash);

        if (programComplete)
            return;

        var checkInput = _program.GetInput(flashIx);

        if (_program.CheckInformation(checkInput))
        {
            _module.DoLog($"{(checkInput is int ? $"Position {(int)checkInput + 1}" : $"Color {checkInput}")} has been submitted correctly. Program {_program.ProgramIndex + 1} has been completed.");
            programComplete = true;
            _program.Terminal.MarkProgramAsComplete(_program.ProgramIndex);
        }
        else
        {
            _module.DoLog($"Submitted {(checkInput is int ? $"position {(int)checkInput + 1}" : $"color {checkInput.ToString().ToLowerInvariant()}")}, but the answer is {_program.GetWrongInput(checkInput)}. Strike!");
            _module.Module.HandleStrike();
            _program.StartFlashingSequence();
        }
    }

    IEnumerator StartFlash(List<ATColor> colorSeq)
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {
            // TODO: Make DOS PC speaker related sounds

            while (flashIx < 6)
            {
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

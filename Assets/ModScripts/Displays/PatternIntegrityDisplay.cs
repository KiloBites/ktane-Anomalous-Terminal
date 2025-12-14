using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;

public class PatternIntegrityDisplay : MonoBehaviour
{
    public TextMesh[] Grids;
    public TextMesh RightWindowCB;
    public TextMesh DisplayButtonInfo;
    public MeshRenderer ColorCycle;
    public KMSelectable RightWindow, CheckButton, ResetButton;
    public KMSelectable[] TransformButtons;

    private PatternIntegrity _program;
    private Terminal _terminal;

    private Coroutine rightWindowAnim;

    private Color origColor;

    private bool[] copiedModifiedPattern;

    private static readonly string[] buttonInfo =
    {
        "Flip Horizontal",
        "Flip Vertical",
        "Shift Row",
        "Shift Column"
    };

    [NonSerialized]
    public bool TransformButtonsShown = false;

    public void AssignProgram(PatternIntegrity program) => _program = program;
    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void Awake()
    {
        RightWindow.OnInteract += () => { RightWindowPress(); return false; };
        CheckButton.OnInteract += () => { CheckButtonPress(); return false; };
        ResetButton.OnInteract += () => { ResetPress(); return false; };

        foreach (KMSelectable transformButton in TransformButtons)
        {
            transformButton.OnInteract += () => { TransformButtonPress(transformButton); return false; };
            transformButton.OnHighlight += () => { HighlightButton(transformButton); };
            transformButton.OnHighlightEnded += () => { EndHighlight(); }; 
        }

        origColor = ColorCycle.material.color;
    }

    void Start()
    {
        foreach (KMSelectable transformButton in TransformButtons)
            transformButton.gameObject.SetActive(false);
    }

    IEnumerator CycleColors(List<ATColor> colors)
    {
        while (true)
        {
            foreach (var color in colors)
            {
                ColorCycle.material.color = color == ATColor.Red ? Color.red : color == ATColor.Yellow ? new Color(1, 1, 0) : Color.blue;
                RightWindowCB.text = _program.NeedColorblind.Value ? color.ToString()[0].ToString() : string.Empty;
                RightWindowCB.color = color == ATColor.Yellow ? Color.black : Color.white;
                yield return new WaitForSeconds(0.5f);
                ColorCycle.material.color = origColor;
                RightWindowCB.text = string.Empty;
                yield return new WaitForSeconds(0.5f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void DisplayGrids()
    {
        if (copiedModifiedPattern == null)
            copiedModifiedPattern = _program.ModifiedPattern.ToArray();

        var selectedPatternGroup = _program.SelectedPattern.Select((x, i) => new { Index = i, Dot = x }).GroupBy(x => x.Index / 5).Select(x => x.Select(y => y.Dot).ToArray()).ToArray();
        var modifiedPatternGroup = _program.ModifiedPattern.Select((x, i) => new { Index = i, Dot = x }).GroupBy(x => x.Index / 5).Select(x => x.Select(y => y.Dot).ToArray()).ToArray();

        Grids[0].text = selectedPatternGroup.Select(x => x.Select(y => y ? "■" : " ").Join("")).Join("\n");
        Grids[1].text = modifiedPatternGroup.Select(x => x.Select(y => y ? "■" : " ").Join("")).Join("\n");
    }

    void ResetPress()
    {
        ResetButton.AddInteractionPunch(0.2f);

        if (_program.ProgramComplete)
            return;

        _program.ModifiedPattern = copiedModifiedPattern.ToArray();
        UpdateRightWindow();
    }

    void RightWindowPress()
    {
        RightWindow.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", RightWindow.transform);

        if (_program.ProgramComplete)
            return;

        TransformButtonsShown ^= true;

        foreach (var button in TransformButtons)
            button.gameObject.SetActive(TransformButtonsShown);

        if (rightWindowAnim != null)
            StopCoroutine(rightWindowAnim);

        rightWindowAnim = TransformButtonsShown ? StartCoroutine(CycleColors(_program.ColorsToTransform)) : null;

    }

    void UpdateRightWindow()
    {
        var modifiedPatternGroup = _program.ModifiedPattern.Select((x, i) => new { Index = i, Dot = x }).GroupBy(x => x.Index / 5).Select(x => x.Select(y => y.Dot).ToArray()).ToArray();

        Grids[1].text = modifiedPatternGroup.Select(x => x.Select(y => y ? "■" : " ").Join("")).Join("\n");
    }

    void CheckButtonPress()
    {
        CheckButton.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", CheckButton.transform);

        if ( _program.ProgramComplete)
            return;

        if (_program.CheckInformation(_program.ModifiedPattern))
        {
            _terminal.Module.DoLog("The pattern matches that of the left window. Program completed.");
            _program.ProgramComplete = true;
            _terminal.DoCreepyShit(_program.ProgramIndex);
        }
        else
        {
            _terminal.Module.DoLog("The pattern doesn't match that of the left window. Strike!");
            _terminal.Module.Module.HandleStrike();
        }
    }

    void TransformButtonPress(KMSelectable transformButton)
    {
        transformButton.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", transformButton.transform);

        if (_program.ProgramComplete)
            return;

        _program.ModifiedPattern = _program.ApplyTransformationFromButton(Array.IndexOf(TransformButtons, transformButton));
        UpdateRightWindow();
    }

    void HighlightButton(KMSelectable transformButton) => DisplayButtonInfo.text = buttonInfo[Array.IndexOf(TransformButtons, transformButton)];
    void EndHighlight() => DisplayButtonInfo.text = string.Empty;

}
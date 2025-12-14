using System;
using System.Collections;
using UnityEngine;
using static UnityEngine.Random;

public class TemperatureCheckDisplay : MonoBehaviour
{
    public KMSelectable[] Buttons;
    public TextMesh TempDisplay, ButtonInfo, MiscInfo;

    private TempCheck _program;
    private Terminal _terminal;

    private Coroutine miscAnim;

    private enum ButtonName
    {
        Decrease,
        Scan,
        Check,
        Increase
    }

    void Awake()
    {
        foreach (KMSelectable button in Buttons)
        {
            button.OnInteract += () => { ButtonPress(button); return false; };
            button.OnHighlight += () => { HighlightButton(button); };
            button.OnHighlightEnded += () => { EndHighlight(); };
        }
    }

    public void AssignProgram(TempCheck program) => _program = program;
    public void AssignTerminal(Terminal terminal) => _terminal = terminal;

    void ButtonPress(KMSelectable button)
    {
        button.AddInteractionPunch(0.2f);
        _terminal.Module.Audio.PlaySoundAtTransform($"Type{Range(0, 3)}", button.transform);

        if (_program.ProgramComplete || miscAnim != null)
            return;

        switch ((ButtonName)Array.IndexOf(Buttons, button))
        {
            case ButtonName.Decrease:
                if (_program.CurrentTemperature == -20)
                    return;

                _program.CurrentTemperature--;
                DisplayTemperature();
                break;
            case ButtonName.Scan:
            case ButtonName.Check:
                miscAnim = StartCoroutine((ButtonName)Array.IndexOf(Buttons, button) == ButtonName.Scan ? ScanForAnomalies() : CheckTemp());
                break;
            case ButtonName.Increase:
                if (_program.CurrentTemperature == 100)
                    return;

                _program.CurrentTemperature++;
                DisplayTemperature();
                break;
        }
    }

    public void DisplayTemperature() => TempDisplay.text = $"Current Temperature: {_program.CurrentTemperature}°C";

    IEnumerator ScanForAnomalies()
    {
        var loadCount = 0;

        MiscInfo.color = Color.white;

        while (loadCount < 3)
        {
            MiscInfo.text = "Scanning";
            var dotCount = 0;

            while (dotCount < 3)
            {
                MiscInfo.text += '.';
                yield return new WaitForSeconds(0.4f);
                dotCount++;
            }
            loadCount++;
        }

        if (_program.AnomalyPresent)
        {
            MiscInfo.text = "Anomaly Detected";
            MiscInfo.color = Color.red;
        }
        else
        {
            MiscInfo.text = "No Anomalies Detected";
            MiscInfo.color = Color.green;
        }

        miscAnim = null;
    }

    IEnumerator CheckTemp()
    {
        var loadCount = 0;

        MiscInfo.color = Color.white;

        while (loadCount < 3)
        {
            MiscInfo.text = "Checking Temperature";
            var dotCount = 0;

            while (dotCount < 3)
            {
                MiscInfo.text += '.';
                yield return new WaitForSeconds(0.4f);
                dotCount++;
            }
            loadCount++;
        }

        if (_program.CheckInformation(_program.CurrentTemperature))
        {
            _terminal.Module.DoLog("The temperature is in the right range of 25-38. Program complete.");
            _program.ProgramComplete = true;
            _terminal.DoCreepyShit(_program.ProgramIndex);
        }
        else
        {
            _terminal.Module.DoLog($"The temperature isn't in the correct range (current temperature is {_program.CurrentTemperature}). Strike!");
            _terminal.Module.Module.HandleStrike();
        }
        miscAnim = null;
    }

    void HighlightButton(KMSelectable button) => ButtonInfo.text = ((ButtonName)Array.IndexOf(Buttons, button)).ToString();

    void EndHighlight() => ButtonInfo.text = string.Empty;

}

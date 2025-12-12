using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ATColor;

public class Motherboard : MonoBehaviour
{
    public List<KMSelectable> Pins;
    public MeshRenderer Led;
    public TextMesh CBText;
    public Light LedLight;

    public AnomalousTerminalScript Module;
    private bool cbActive;

    [NonSerialized]
    public bool Phase2Completed;

    private static readonly Color[] baseLEDColors =
    {
        Color.red,
        new Color(1, 1, 0),
        Color.green,
        Color.blue
    };

    private static readonly Color[] lightLEDColors =
    {
        new Color(1, 0.5f, 0.5f),
        new Color(1, 1, 0.5f),
        new Color(0.5f, 1, 0.5f),
        new Color(0.5f, 0.5f, 1)
    };

    private static readonly ATColor[][] colorSequences =
    {
        new[] { Red, Yellow, Green },
        new[] { Yellow, Red, Blue },
        new[] { Yellow, Green, Red },
        new[] { Green, Blue, Yellow },
        new[] { Red, Yellow, Blue },
        new[] { Green, Blue, Red }
    };

    private Dictionary<ATColor[], int[][]> colorsToPinsToShort;

    private ATColor[] selectedSequence;
    private int[][] pinsToShort;
    private List<int> inputs = new List<int>();
    private List<List<int>> inputSequences = new List<List<int>>();
    private bool[] pinPairsShortedCorrectly = new bool[3];
    private int pinsShortCount = 0;

    private Coroutine ledPlaying;


    void Awake()
    {
        cbActive = Module.Colorblind.ColorblindModeActive;

        foreach (KMSelectable pin in Pins)
        {
            pin.OnInteract += () => { PinPress(pin); return false; };
            pin.gameObject.SetActive(false);
        }
            

        colorsToPinsToShort = Setup();
    }

    public void SetupBoard()
    {
        selectedSequence = colorSequences.PickRandom();

        Module.DoLog($"Color sequence selected: {colorSequences.IndexOf(x => x.SequenceEqual(selectedSequence)) + 1} ({selectedSequence.Join()})");

        pinsToShort = colorsToPinsToShort[selectedSequence];

        ledPlaying = StartCoroutine(ShowColorSequence());

        Pins.ForEach(x => x.gameObject.SetActive(true));
    }

    public void ToggleColorblind() => cbActive ^= true;

    IEnumerator ShowColorSequence()
    {
        while (true)
        {
            LedLight.enabled = true;

            foreach (var color in selectedSequence)
            {
                Led.material.color = baseLEDColors[(int)color];
                LedLight.color = lightLEDColors[(int)color];
                CBText.text = cbActive ? color.ToString()[0].ToString() : string.Empty;
                CBText.color = color == Yellow ? Color.black : Color.white;

                yield return new WaitForSeconds(0.75f);
            }

            CBText.text = string.Empty;
            LedLight.enabled = false;
            Led.material.color = Color.black;
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator ShowPinCount()
    {
        CBText.text = string.Empty;

        var countIx = 0;

        while (true)
        {
            while (countIx < pinsShortCount)
            {
                LedLight.enabled = true;
                Led.material.color = baseLEDColors[2];
                LedLight.color = lightLEDColors[2];
                yield return new WaitForSeconds(0.3f);
                LedLight.enabled = false;
                Led.material.color = Color.black;
                yield return new WaitForSeconds(0.3f);
                countIx++;
            }
            countIx = 0;
            yield return new WaitForSeconds(0.6f);
        }
    }

    private static Dictionary<ATColor[], int[][]> Setup()
    {
        var colorSeqs = colorSequences.ToArray();

        var pinsToShort = new[]
        {
            new[]
            {
                new[] { 2, 3 },
                new[] { 1, 4 },
                new[] { 2, 7 }
            },
            new[]
            {
                new[] { 2, 5 },
                new[] { 5, 6 },
                new[] { 7, 0 }
            },
            new[]
            {
                new[] { 3, 4 },
                new[] { 1, 3 },
                new[] { 3, 5 }
            },
            new[]
            {
                new[] { 5, 1 },
                new[] { 0, 2 },
                new[] { 2, 4 }
            },
            new[]
            {
                new[] { 7, 4 },
                new[] { 2, 6 },
                new[] { 0, 2 }
            },
            new[]
            {
                new[] { 0, 4 },
                new[] { 3, 4 },
                new[] { 6, 5 }
            }
        };

        return Enumerable.Range(0, 6).ToDictionary(x => colorSeqs[x], x => pinsToShort[x]);
    }

    void PinPress(KMSelectable pin)
    {
        pin.AddInteractionPunch(0.2f);
        Module.Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonRelease, pin.transform);

        var ix = Array.IndexOf(Pins.ToArray(), pin);

        if (Phase2Completed || inputs.Contains(ix))
            return;

        if (inputs.Count == 1)
        {
            inputs.Add(ix);
            pinPairsShortedCorrectly[pinsShortCount] = inputs.SequenceEqual(pinsToShort[pinsShortCount]);

            if (ledPlaying != null)
            {
                StopCoroutine(ledPlaying);
                ledPlaying = null;
            }

            pinsShortCount++;

            if (pinsShortCount == 3) // Todo: Make the module fall back into place and check to see if the pins in sequence are shorted correctly.
                return;

            ledPlaying = StartCoroutine(ShowPinCount());
            inputSequences.Add(inputs.ToList());
            inputs.Clear();
        }
        else
            inputs.Add(ix);
    }
}
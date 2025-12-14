using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerminalBIOS : MonoBehaviour
{
    public KMAudio Audio;
    public AudioClip Bootup;
    private KMAudio.KMAudioRef audioRef;

    public GameObject[] BiosStuff;
    public GameObject LoadingScreen;

    [NonSerialized]
    public Coroutine TerminalRunning;

    [NonSerialized]
    public bool RecoverySucessful;


    // Main BIOS shit
    public TextMesh RamDisplay;
    public TextMesh LoadingBar, MainText, CopyrightText;



    // ALT BIOS after shorting pins on the motherboard for phase 2
    public TextMesh[] NumberPairDisplays;
    public TextMesh[] NumberValidators;
    public TextMesh BiosAltText;

    public Material[] ScreenColors;
    public MeshRenderer Screen;

    [NonSerialized]
    public bool FirstBoot = true;

    private static int terminalIdCounter = 1;
    private int terminalId;

    private struct PinShortVerificationInfo
    {
        public TextMesh NumberPairDisplay { get; private set; }
        public TextMesh Validator { get; private set; }

        public PinShortVerificationInfo(TextMesh numberPairDisplay, TextMesh validator)
        {
            NumberPairDisplay = numberPairDisplay;
            Validator = validator;
        }

        public void ClearText()
        {
            NumberPairDisplay.text = string.Empty;
            Validator.text = string.Empty;
        }
    }


    private List<PinShortVerificationInfo> checkers;

    private Coroutine playAudio;

    void Awake()
    {
        terminalId = terminalIdCounter++;
        checkers = Enumerable.Range(0, 3).Select(x => new PinShortVerificationInfo(NumberPairDisplays[x], NumberValidators[x])).ToList();
    }

    void OnDestroy()
    {
        terminalIdCounter = 1;
        audioRef?.StopSound();
    }

    public void InitializeBIOS(bool alt, List<List<int>> numberPairs = null, bool[] numbersToCheck = null) => TerminalRunning = StartCoroutine(RunBIOS(alt, numberPairs, numbersToCheck));

    IEnumerator PlaySoundUntilClipStops()
    {
        if ((FirstBoot && terminalId == 1) || !FirstBoot)
            audioRef = Audio.PlaySoundAtTransformWithRef(Bootup.name, transform);

        var duration = Bootup.length;
        var elapsed = 0f;

        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        audioRef?.StopSound();
        playAudio = null;
    }

    IEnumerator RunBIOS(bool alt, List<List<int>> numberPairs, bool[] numbersToValidate)
    {
        playAudio = StartCoroutine(PlaySoundUntilClipStops());

        yield return new WaitForSeconds(3.8f);

        if (alt)
        {
            BiosStuff[1].SetActive(true);
            Screen.material = ScreenColors[1];

            yield return new WaitForSeconds(0.5f);

            for (int i = 0; i < 3; i++)
            {
                checkers[i].NumberPairDisplay.text = $"({numberPairs[i].Select(x => x + 1).Join(", ")})";
                yield return new WaitForSeconds(1);
                checkers[i].Validator.text = numbersToValidate[i] ? "OK" : "ERROR";
                checkers[i].Validator.color = numbersToValidate[i] ? Color.green : Color.red;
                yield return new WaitForSeconds(1);
            }
            yield return new WaitForSeconds(0.2f);

            RecoverySucessful = numbersToValidate.All(x => x);

            BiosAltText.text = $"RECOVERY {(RecoverySucessful ? "SUCCESSFUL" : "FAILED")}";
            yield return new WaitForSeconds(2);

            StopCoroutine(playAudio);
            playAudio = null;
            audioRef?.StopSound();
            checkers.ForEach(x => x.ClearText());
            BiosStuff[1].SetActive(false);
            Screen.material = ScreenColors[0];
            TerminalRunning = null;
        }
        else
        {
            ResetLoading();
            BiosStuff[0].SetActive(true);
            Screen.material = ScreenColors[1];
            var ram = 0;

            while (ram < 2048)
            {
                ram++;
                RamDisplay.text = $"{ram}KB OK";
                yield return new WaitForSeconds(0.005f);
            }
            yield return new WaitForSeconds(0.4f);
            BiosStuff[0].SetActive(false);
            yield return new WaitForSeconds(0.2f);
            LoadingScreen.SetActive(true);

            while (LoadingBar.text.Length < 10)
            {
                LoadingBar.text += '■';
                yield return new WaitForSeconds(0.5f);
            }
            if (playAudio != null)
            {
                StopCoroutine(playAudio);
                playAudio = null;
                audioRef?.StopSound();
            }

            if (terminalId == 1)
                Audio.PlaySoundAtTransform("GlitchIntro", transform);

            MainText.text = "YOU ARE MINE\nVERSION 6.66";

            StartCoroutine(MakeLoadingBarRed());
            yield return new WaitForSeconds(1);
            CopyrightText.text = Enumerable.Repeat("666", 9).Join();
            CopyrightText.color = Color.red;
            MainText.color = Color.red;
            yield return new WaitForSeconds(0.3f);
            LoadingScreen.SetActive(false);
            Screen.material = ScreenColors[0];
            yield return new WaitForSeconds(1);
            Screen.material = ScreenColors[1];
            yield return new WaitForSeconds(1);
            Audio.PlaySoundAtTransform("EnterMainMenu", transform);
            yield return new WaitForSeconds(0.4f);
            TerminalRunning = null;
        }
    }

    void ResetLoading()
    {
        CopyrightText.text = "(C)Copright Bombsoft Corp 1983-1987";
        CopyrightText.color = Color.white;
        MainText.text = "BOMB-TERMINAL\nVERSION 6.22";
        MainText.color = Color.white;
        LoadingBar.text = string.Empty;
        LoadingBar.color = Color.white;
    }

    IEnumerator MakeLoadingBarRed()
    {
        var duration = 1f;
        var elasped = 0f;

        while (elasped < duration)
        {
            yield return null;
            LoadingBar.color = Color.Lerp(LoadingBar.color, Color.red, elasped);
            elasped += Time.deltaTime;
        }

        LoadingBar.color = Color.red;
    }
}
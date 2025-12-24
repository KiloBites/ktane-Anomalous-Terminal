using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;

public class VCRDisplay : MonoBehaviour
{
    public TextMesh[] MainVCRTexts;

    private Coroutine startup;
    private Coroutine[] textCoroutines = new Coroutine[3];

    [NonSerialized]
    public Coroutine Glitch;

    private enum TextAction
    {
        RepeatLine,
        RepeatLineLeak,
        Normal
    }

    private static readonly string[] startupTexts =
    {
        "HELP ME",
        "I SEE YOU",
        "GET ME OUT",
        "FRIEND INSIDE ME",
        "CAN YOU SEE ME?",
        "I'M IN YOUR DREAMS",
        "THE NIGHTMARE BEGINS",
        "WE ARE HERE",
        "FREE US",
        "SET ME FREE"
    };

    void Awake()
    {
        foreach (var text in MainVCRTexts)
            text.text = string.Empty;
    }

    public void InitializeStartup() => startup = StartCoroutine(MainStartup());

    public void InitializeGlitch(float clipDuration, MeshRenderer screen) => Glitch = StartCoroutine(DoCreepyShit(clipDuration, screen));
    public void InitializeError(bool caught, float clipDuration, MeshRenderer screen) => Glitch = StartCoroutine(Error(caught, clipDuration, screen));

    public void Transition(MeshRenderer screen)
    {
        startup = StartCoroutine(TransitionToSecondPhase());
        Glitch = StartCoroutine(CrashOutBro(screen));
    }

    public void EndTransition()
    {
        StopCoroutine(startup);
        StopCoroutine(Glitch);
        startup = null;
        Glitch = null;

        for (int i = 0; i < 3; i++)
        {
            StopCoroutine(textCoroutines[i]);
            textCoroutines[i] = null;
            MainVCRTexts[i].text = string.Empty;
        }
    }

    public void StartSolve(MeshRenderer screen)
    {
        MainVCRTexts[1].text = "WE WILL MEET AGAIN";
        StartCoroutine(CrashOutBro(screen));
    }

    IEnumerator DoCreepyShit(float clipDuration, MeshRenderer screen)
    {
        Coroutine theCreepy = null, crashOut;

        var oldColor = screen.material.color;

        crashOut = StartCoroutine(CrashOutBro(screen));

        switch (Range(0, 3))
        {
            case 0:
                theCreepy = StartCoroutine(SprayWithHell());
                break;
            case 1:
                theCreepy = StartCoroutine(FlashyFlashFlash());
                break;
            case 2:
                theCreepy = StartCoroutine(OneTwoThree());
                break;
        }

        var elapsed = 0f;

        while (elapsed < clipDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopCoroutine(theCreepy);
        StopCoroutine(crashOut);
        screen.material.color = oldColor;
        KillTextsAfterGlitch();
        Glitch = null;
    }

    IEnumerator CrashOutBro(MeshRenderer screen)
    {
        while (true)
        {
            var myEyes = Range(0.4f, 1);
            screen.material.color = ColorWithSingleValue(myEyes);
            yield return new WaitForSeconds(Range(0.01f, 0.1f));
        }
    }

    private Color ColorWithSingleValue(float v) => new Color(v, v, v);

    IEnumerator SprayWithHell()
    {
        var helpMeGetMeOut = Enumerable.Repeat("HELP", 4).ToArray();

        foreach (var vcrText in MainVCRTexts)
            for (int i = 0; i < helpMeGetMeOut.Length; i++)
            {
                vcrText.text += helpMeGetMeOut[i];
                if (i != 3)
                    vcrText.text += " ";
                yield return new WaitForSeconds(0.1f);
            }
    }

    IEnumerator FlashyFlashFlash()
    {
        while (true)
        {
            MainVCRTexts[1].text = startupTexts.PickRandom();
            yield return new WaitForSeconds(Range(0.04f, 0.2f));
            MainVCRTexts[1].text = string.Empty;
            yield return new WaitForSeconds(Range(0.04f, 0.2f));
        }
    }

    IEnumerator OneTwoThree()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                var randomPhrase = (i == 1 ? startupTexts : startupTexts.Where(x => x.Length < 10)).PickRandom();

                MainVCRTexts[i].text = randomPhrase;
                yield return new WaitForSeconds(0.4f);
                MainVCRTexts[i].text = string.Empty;
            }
        }
    }

    IEnumerator MainStartup()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                if (textCoroutines[i] != null)
                {
                    StopCoroutine(textCoroutines[i]);
                    textCoroutines[i] = null;
                }

                textCoroutines[i] = StartCoroutine(TextStartup(i, (i == 1 ? TextAction.Normal : (TextAction)Range(0, 3))));
            }
                
            yield return new WaitForSeconds(Range(0.2f, 0.5f));
        }
    }

    IEnumerator TransitionToSecondPhase()
    {
        while (true)
        {
            for (int i = 0; i < 3; i++)
            {
                if (textCoroutines[i] != null)
                {
                    StopCoroutine(textCoroutines[i]);
                    textCoroutines[i] = null;
                }

                textCoroutines[i] = StartCoroutine(TextStartup(i, (i == 1 ? TextAction.Normal : (TextAction)Range(0, 3))));
            }
            yield return new WaitForSeconds(Range(0.05f, 0.2f));
        }
    }

    IEnumerator TextStartup(int ix, TextAction action)
    {
        yield return null;

        var repeatNum = Range(1, 4) + 1;
        var randomPhrase = (ix == 1 ? startupTexts : startupTexts.Where(x => x.Length < 10)).PickRandom();

        MainVCRTexts[ix].text = string.Empty;

        switch (action)
        {
            case TextAction.RepeatLine:
                MainVCRTexts[ix].text = Enumerable.Repeat(randomPhrase, repeatNum).Join("\n");
                break;
            case TextAction.RepeatLineLeak:
                for (int i = 0; i < repeatNum; i++)
                {
                    MainVCRTexts[ix].text += randomPhrase;
                    yield return new WaitForSeconds(0.1f);
                    if (i + 1 < repeatNum)
                        MainVCRTexts[ix].text += "\n";
                }
                break;
            default:
                MainVCRTexts[ix].text = randomPhrase;
                break;
        }
    }

    IEnumerator Error(bool caught, float clipDuration, MeshRenderer screen)
    {
        foreach (var vcrText in MainVCRTexts)
            vcrText.text = Enumerable.Repeat(caught ? "GOT YOU" : "GOODBYE", 3).Join();

        Coroutine holyShitImAboutToDie;

        var oldColor = screen.material.color;

        holyShitImAboutToDie = StartCoroutine(CrashOutBro(screen));

        var elapsed = 0f;

        while (elapsed < clipDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        StopCoroutine(holyShitImAboutToDie);

        screen.material.color = oldColor;

        KillTextsAfterGlitch();

        Glitch = null;
    }

    void KillTextsAfterGlitch()
    {
        foreach (var text in MainVCRTexts)
            text.text = string.Empty;
    }

    public void KillTexts()
    {
        StopCoroutine(startup);
        startup = null;
        Glitch = null;

        for (int i = 0; i < 3; i++)
        {
            StopCoroutine(textCoroutines[i]);
            textCoroutines[i] = null;
            MainVCRTexts[i].text = string.Empty;
        }       
    }
}
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;

public class VCRDisplay : MonoBehaviour
{
    public TextMesh[] MainVCRTexts;

    private Coroutine startup;
    private Coroutine[] textCoroutines = new Coroutine[3];

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

    public void InitializeStartup()
    {
        startup = StartCoroutine(MainStartup());
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

    IEnumerator TextStartup(int ix, TextAction action)
    {
        yield return null;

        var repeatNum = Range(1, 4) + 1;
        var randomPhrase = ix == 1 ? startupTexts.PickRandom() : startupTexts.Where(x => x.Length < 10).PickRandom();

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

    public void KillTexts()
    {
        StopCoroutine(startup);
        startup = null;

        for (int i = 0; i < 3; i++)
        {
            StopCoroutine(textCoroutines[i]);
            textCoroutines[i] = null;
            MainVCRTexts[i].text = string.Empty;
        }       
    }
}
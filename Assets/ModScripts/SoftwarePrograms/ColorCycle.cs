using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Random;

public class ColorCycle : SoftwareProgram
{
    private struct ColorInfo
    {
        public bool? NeedDigit { get; set; }
        public int? Index { get; private set; }
        public ATColor Color { get; private set; }

        public ColorInfo(bool? needDigit, int? index, ATColor color)
        {
            NeedDigit = needDigit;
            Index = index;
            Color = color;
        }
    }

    private readonly ColorInfo[] seq;
    private ColorInfo answer;

    private static readonly Color[] flashColors =
    {
        Color.red,
        new Color(1, 1, 0),
        Color.green,
        Color.blue
    };

    public ColorCycle(SoftwareProgramType programType) : base(programType)
    {
        seq = Enumerable.Range(0, 6).Select(x => new ColorInfo(null, x, (ATColor)Range(0, 4))).ToArray();
        answer = GenerateAnswer();

    }

    public override bool CheckInformation(object other)
    {
        if (other is int)
            return answer.NeedDigit != null && answer.Index == (int)other;
        else if (other is ATColor)
            return answer.NeedDigit == null && answer.Color == (ATColor)other;

        return false;
    }

    public override void ExecuteProgram()
    {
        throw new NotImplementedException();
    }

    public override IEnumerator SoftwareAnim()
    {
        yield return new WaitForSeconds(0.2f);

        while (true)
        {

        }
    }

    public object GetInput(int ix)
    {
        if (answer.NeedDigit == null)
            return seq[ix].Color;

        return seq[ix].Index.Value;
    }

    private ColorInfo GenerateAnswer()
    {
        var copiedSeq = seq.ToArray();

        var colorSeq = copiedSeq.Select(x => x.Color).ToArray();

        switch (seq.First().Color)
        {
            case ATColor.Red:
                if (new[] { 1, 5 }.Any(x => copiedSeq[x].Color == ATColor.Blue))
                {
                    if (!colorSeq.Contains(ATColor.Yellow))
                    {
                        copiedSeq[3].NeedDigit = true;
                        return copiedSeq[3];
                    }

                    return new ColorInfo(null, null, ATColor.Yellow);
                }
                else if (copiedSeq.Count(x => x.Color == ATColor.Red) >= 3)
                {
                    var redDifference = 6 - copiedSeq.Count(x => x.Color == ATColor.Red);

                    return copiedSeq[redDifference];
                }
                else if (!colorSeq.Contains(ATColor.Blue))
                {
                    copiedSeq[5].NeedDigit = true;
                    return copiedSeq.Last();
                }
                return new ColorInfo(null, null, ATColor.Blue);
            case ATColor.Yellow:
                if (copiedSeq.Last().Color == ATColor.Green)
                {
                    if (!colorSeq.Contains(ATColor.Blue))
                    {
                        copiedSeq[2].NeedDigit = true;
                        return copiedSeq[2];
                    }
                    return new ColorInfo(null, null, ATColor.Blue);
                }
                else if (copiedSeq.Count(x => x.Color == ATColor.Blue || x.Color == ATColor.Yellow) == 4)
                {
                    copiedSeq[4].NeedDigit = true;
                    return copiedSeq[4];
                }
                else if (!colorSeq.Contains(ATColor.Red))
                {
                    copiedSeq[5].NeedDigit = true;
                    return copiedSeq.Last();
                }
                return new ColorInfo(null, null, ATColor.Red);
            case ATColor.Green:
                if (new[] { 2, 3 }.Any(x => copiedSeq[x].Color == ATColor.Red))
                {
                    if (!colorSeq.Contains(ATColor.Blue))
                    {
                        copiedSeq[1].NeedDigit = true;
                        return copiedSeq[1];
                    }
                    return new ColorInfo(null, null, ATColor.Blue);
                }
                else if (Math.Abs(copiedSeq.Count(x => x.Color == ATColor.Red) - copiedSeq.Count(x => x.Color == ATColor.Yellow)) % 2 != 0)
                {
                    var yellowSum = copiedSeq.Count(x => x.Color == ATColor.Yellow) % 6;

                    copiedSeq[yellowSum].NeedDigit = true;
                    return copiedSeq[yellowSum];
                }

                return new ColorInfo(null, null, ATColor.Green);
            case ATColor.Blue:
                if (copiedSeq[1].Color == ATColor.Yellow)
                {
                    if (!colorSeq.Contains(ATColor.Green))
                    {
                        copiedSeq[4].NeedDigit = true;
                        return copiedSeq[4];
                    }
                    return new ColorInfo(null, null, ATColor.Green);
                }
                else if (Math.Abs(copiedSeq.Count(x => x.Color == ATColor.Blue) - copiedSeq.Count(x => x.Color == ATColor.Yellow)) % 2 == 0)
                {
                    copiedSeq[3].NeedDigit = true;
                    return copiedSeq[3];
                }
                else if (!colorSeq.Contains(ATColor.Yellow))
                {
                    copiedSeq[2].NeedDigit = true;
                    return copiedSeq[2];
                }
                return new ColorInfo(null, null, ATColor.Yellow);
        }

        throw new Exception("Enum index is invalid.");
    }


}
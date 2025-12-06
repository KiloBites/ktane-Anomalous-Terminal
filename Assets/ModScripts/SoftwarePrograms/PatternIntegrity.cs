using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class PatternIntegrity : SoftwareProgram
{
    private readonly bool[] selectedPattern;
    public bool[] ModifiedPattern;

    private static readonly bool[][] patterns = new[]
    {
        "..x...xxx.xxxxxxxxxx..x..",
        "x...x..x...xxx...x..x...x",
        "....x...x.xxxxx.x...x....",
        ".x.x.xxxxxxxxxx.xxx...x..",
        "xxxxx.x.x..x.x..x.x.xxxxx",
        "..x...xxx.xxxxx.xxx..x.x."
    }.Select(x => x.Select(y => y == 'x').ToArray()).ToArray();

    private readonly List<ATColor> colorsToTransform;

    public PatternIntegrity(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        selectedPattern = patterns.PickRandom();
        ModifiedPattern = selectedPattern.ToArray();

        var colors = (ATColor[])Enum.GetValues(typeof(ATColor));
        var colorList = colors.ToList();
        colorList.Remove(ATColor.Green);

        colorsToTransform = Enumerable.Range(0, 4).Select(_ => colorList.PickRandom()).ToList().Shuffle();

        if (Range(0, 2) != 0)
            for (int i = 0; i < colorsToTransform.Count; i++)
                ModifiedPattern = TransformPattern(ModifiedPattern, colorsToTransform[i], i);
    }

    public override string ToString() => $"The selected pattern in reading order is: {patterns.IndexOf(x => x.SequenceEqual(selectedPattern)) + 1}";

    private bool[] TransformPattern(bool[] pattern, ATColor color, int pos)
    {
        var groups = Enumerable.Range(0, 5).Select(x => Enumerable.Range(0, 5).Select(y => 10 * x + y).ToArray()).ToArray();
        var converted = Enumerable.Range(0, 5).Select(_ => new int[5]).ToArray();

        switch (color)
        {
            case ATColor.Red:
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (pos % 2 == 0)
                            converted[i][5 - j] = groups[i][j];
                        else
                            converted[5 - i][j] = groups[i][j];
                    }
                break;
            case ATColor.Yellow:
            case ATColor.Blue:
                var shifts = Enumerable.Range(0, 5).ToArray();

                for (int i = 0; i < 5; i++)
                {
                    if (color == ATColor.Yellow)
                        shifts[i] = (pos % 2 == 0 ? (shifts[i] - (pos + 1) + 5) : (shifts[i] + pos + 1)) % 5;
                    else
                        shifts[i] = (pos % 2 == 0 ? (shifts[i] + pos + 1) : (shifts[i] - (pos + 1) + 5)) % 5;
                }

                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                        converted[i][j] = color == ATColor.Yellow ? groups[shifts[i]][j] : groups[i][shifts[j]];
                break;
            default:
                throw new Exception($"{color} is not valid for transformation!");

        }

        return converted.SelectMany(x => x.Select(y => pattern[y])).ToArray();
    }

    public override bool CheckInformation(object other)
    {
        if (other is bool[])
        {
            var answerToCheck = (bool[])other;

            return answerToCheck.SequenceEqual(selectedPattern);
        }

        return false;
    }
}

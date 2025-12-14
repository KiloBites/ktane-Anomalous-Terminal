using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class PatternIntegrity : SoftwareProgram
{
    public bool[] SelectedPattern;
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

    public List<ATColor> ColorsToTransform;

    public PatternIntegrity(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        SelectedPattern = patterns.PickRandom();
        ModifiedPattern = SelectedPattern.ToArray();

        var colors = (ATColor[])Enum.GetValues(typeof(ATColor));
        var colorList = colors.ToList();
        colorList.Remove(ATColor.Green);

        ColorsToTransform = Enumerable.Range(0, 4).Select(_ => colorList.PickRandom()).ToList().Shuffle();

        if (Range(0, 2) == 0)
            for (int i = 0; i < ColorsToTransform.Count; i++)
                ModifiedPattern = TransformPattern(ModifiedPattern, ColorsToTransform[i], i);
    }

    public override string ToString() => $"The selected pattern in reading order is: {patterns.IndexOf(x => x.SequenceEqual(SelectedPattern)) + 1}. {(!ModifiedPattern.SequenceEqual(SelectedPattern) ? $"The anomaly transformed the grid. The colors in order are: {ColorsToTransform.Join(", ")}" : "The pattern on the right window matches that of the selected pattern.")}";

    public bool[] ApplyTransformationFromButton(int pos)
    {
        var groups = Enumerable.Range(0, 5).Select(x => Enumerable.Range(0, 5).Select(y => 5 * x + y).ToArray()).ToArray();
        var converted = Enumerable.Range(0, 5).Select(_ => new int[5]).ToArray();

        var currentState = ModifiedPattern.ToArray();

        switch (pos)
        {
            case 0:
            case 1:
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (pos == 0)
                            converted[i][4 - j] = groups[i][j];
                        else
                            converted[4 - i][j] = groups[i][j];
                    }
                break;
            case 2:
            case 3:
                var shifts = Enumerable.Range(0, 5).ToArray();

                for (int i = 0; i < 5; i++)
                {
                    if (pos == 2)
                        shifts[i] = (shifts[i] + pos + 1) % 5;
                    else
                        shifts[i] = (shifts[i] - (pos + 1) + 5) % 5;
                }

                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                        converted[i][j] = pos == 2 ? groups[shifts[i]][j] : groups[i][shifts[j]];
                break;

        }

        return converted.SelectMany(x => x.Select(y => currentState[y])).ToArray();
    }

    private bool[] TransformPattern(bool[] pattern, ATColor color, int pos)
    {
        var groups = Enumerable.Range(0, 5).Select(x => Enumerable.Range(0, 5).Select(y => 5 * x + y).ToArray()).ToArray();
        var converted = Enumerable.Range(0, 5).Select(_ => new int[5]).ToArray();

        switch (color)
        {
            case ATColor.Red:
                for (int i = 0; i < 5; i++)
                    for (int j = 0; j < 5; j++)
                    {
                        if (pos % 2 == 0)
                            converted[i][4 - j] = groups[i][j];
                        else
                            converted[4 - i][j] = groups[i][j];
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

            return answerToCheck.SequenceEqual(SelectedPattern);
        }

        return false;
    }
}

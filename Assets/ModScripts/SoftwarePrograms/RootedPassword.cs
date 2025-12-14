using System.Collections.Generic;
using System.Linq;

public class RootedPassword : SoftwareProgram
{
    private readonly int[] convertedFromSN;
    private readonly int[] answer;

    public RootedPassword(SoftwareProgramType programType, int programIndex, string sn) : base(programType, programIndex)
    {
        convertedFromSN = sn.Select(x => char.IsLetter(x) ? "-ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(x) % 10 : x - '0').ToArray();

        answer = Enumerable.Range(0, 6).Select(x => x == 0 ? GetDigitalRoot(convertedFromSN) : GetDigitalRoot(convertedFromSN.Skip(x + 1).ToArray())).ToArray();
    }

    public override bool CheckInformation(object other)
    {
        if (other is List<int>)
        {
            var answerToCheck = other as List<int>;

            return answerToCheck.SequenceEqual(answer);
        }

        return false;
    }

    public override string ToString() => $"Rooted password of {convertedFromSN.Join("")} is: {answer.Join("")}";

    public override string DisplayInfo => answer.Join("");

    private int GetDigitalRoot(int[] numbers)
    {
        var squished = 0;

        foreach (var number in numbers)
            squished = squished * 10 + number;

        return squished == 0 ? 0 : (squished - 1) % 9 + 1;
    }
}

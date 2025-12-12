using System.Collections.Generic;
using System.Linq;

public class HexCycle : SoftwareProgram
{

    private static readonly string[] phraseTypes =
    {
        "GET ME OUT",
        "FRIEND INSIDE ME",
        "TIME RUNS OUT",
        "NO TIME LEFT",
        "HELP ME",
        "HE'S GONE",
        "RUN RUN RUN",
        "NIGHTMARE FUEL",
        "WE ARE HERE",
        "AWAIT FOR ARRIVAL",
        "HE SEES ALL. DO YOU?",
        "LET ME OUT OF HERE"
    };

    private static readonly int[][] callResponseIxes =
    {
        new[] { 1, 3 },
        new[] { 2, 7 },
        new[] { 3, 4 },
        new[] { 9, 0 },
        new[] { 6, 5 },
        new[] { 11, 6 },
        new[] { 7, 10 },
        new[] { 10, 8 },
        new[] { 5, 1 },
        new[] { 4, 9 },
        new[] { 0, 11 },
        new[] { 8, 2 }
    };

    private Dictionary<string, string> callResponse;

    private string selectedWord;

    public override string DisplayInfo => ConvertToHex(selectedWord);

    public HexCycle(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        callResponse = callResponseIxes.ToDictionary(x => phraseTypes[x[0]], x => phraseTypes[x[1]]);
        selectedWord = phraseTypes.PickRandom();
    }

    private string ConvertToHex(string text) => text.ToCharArray().Aggregate("", (x, y) => x + ((byte)y).ToString("X") + " ").ToUpperInvariant().Trim();

    public override string ToString() => $"Call word: {selectedWord}, Response word: {callResponse[selectedWord]}";

    public override bool CheckInformation(object other) => other is string && other as string == ConvertToHex(callResponse[selectedWord]);
}

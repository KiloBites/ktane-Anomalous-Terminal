using System.Linq;
using static UnityEngine.Random;

public class TempCheck : SoftwareProgram
{
    public int CurrentTemperature;
    public bool AnomalyPresent;

    public TempCheck(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        AnomalyPresent = Range(0, 2) == 0;

        if (AnomalyPresent)
        {
            var negate = Range(0, 2) == 0;

            CurrentTemperature = negate ? Range(1, 21) * -1 : Enumerable.Range(0, 101).Where(x => !x.InRange(25, 38)).PickRandom();
        }
        else
            CurrentTemperature = Range(25, 39);
    }

    public override bool CheckInformation(object other)
    {
        if (other is int)
        {
            var temp = (int)other;

            return temp.InRange(25, 38);
        }

        return false;
    }

    public override string ToString() => $"The anomaly {(AnomalyPresent ? "is" : "isn't")} present. {(CurrentTemperature.InRange(25, 38) ? "You won't have to make any adjustments." : $"You'll have to adjust the temperature (the temperature initially is {CurrentTemperature})")}";
}
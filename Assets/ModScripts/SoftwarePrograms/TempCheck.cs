using System;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Random;

public class TempCheck : SoftwareProgram
{
    private int currentTemperature;

    public TempCheck(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {
        currentTemperature = Range(25, 39);
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
}
using System;
using System.Collections.Generic;
using System.Linq;

public class TempCheck : SoftwareProgram
{
    private int currentTemperature;

    private bool CheckRange(int temp) => temp >= 25 && temp <= 38;

    public TempCheck(SoftwareProgramType programType, int programIndex) : base(programType, programIndex)
    {

    }

    public override bool CheckInformation(object other)
    {
        if (other is int)
            return CheckRange((int)other);

        return false;
    }
}
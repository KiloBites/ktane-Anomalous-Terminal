using System.Collections;

public abstract class SoftwareProgram
{
    public SoftwareProgramType ProgramType { get; private set; }

    protected SoftwareProgram(SoftwareProgramType programType)
    {
        ProgramType = programType;
    }

    public abstract void ExecuteProgram();
    public virtual string DisplayInfo => null;
    public abstract bool CheckInformation(object other);

    public virtual IEnumerator SoftwareAnim() => null;
}
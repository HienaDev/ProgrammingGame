// Base class for parameter objects
public abstract class MethodParameters { }

// Parameter object for the Walk method
public class WalkParameters : MethodParameters
{
    public float Speed { get; }
    public float Duration { get; }

    public WalkParameters(float speed, float duration)
    {
        Speed = speed;
        Duration = duration;
    }
}

public class JumpParameters : MethodParameters
{
    public float Height { get; }

    public JumpParameters(float height)
    {
        Height = height;
    }
}

public class WaitParameters : MethodParameters
{
    public float Duration { get; }

    public WaitParameters(float duration)
    {
        Duration = duration;
    }
}

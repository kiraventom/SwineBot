namespace SwineBot.Updates;

public class UpdateHandleException(UpdateHandleResult result, Exception baseEx = null) : Exception
{
    public UpdateHandleResult Result { get; } = result;

    public override Exception GetBaseException() => baseEx;
}



namespace SwineBot.Updates;

public abstract class UpdateTypeHandler<T>
{
    public abstract Task<UpdateHandleResult> Handle(T type, CancellationToken token);
}


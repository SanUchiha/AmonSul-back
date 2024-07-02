namespace AS.Domain.Exceptions;

public class UniqueKeyViolationException : Exception
{
    public string Field { get; }

    public UniqueKeyViolationException(string message, string field) : base(message)
    {
        Field = field;
    }
}

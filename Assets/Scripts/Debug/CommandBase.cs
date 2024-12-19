using System;

public class CommandBase
{
    private string _commandId;
    private string _commandDescription;
    private string _commandFormat;

    public string CommandId => _commandId; // Command identifier
    public string CommandDescription => _commandDescription; // Command description
    public string CommandFormat => _commandFormat; // Expected input format

    public CommandBase(string id, string description, string format)
    {
        _commandId = id;
        _commandDescription = description;
        _commandFormat = format;
    }
}

// Command without parameters
public class Command : CommandBase
{
    private Action _command;

    public Command(string id, string description, string format, Action command)
        : base(id, description, format)
    {
        _command = command;
    }

    public void Invoke()
    {
        _command?.Invoke();
    }
}

// Command with a numerical parameter
public class Command<TypeNumerical> : CommandBase
{
    private Action<TypeNumerical> _command;

    public Command(string id, string description, string format, Action<TypeNumerical> command)
        : base(id, description, format)
    {
        _command = command;
    }

    public void Invoke(TypeNumerical value)
    {
        _command?.Invoke(value);
    }
}


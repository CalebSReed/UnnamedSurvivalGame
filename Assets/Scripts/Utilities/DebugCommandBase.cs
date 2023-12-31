using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugCommandBase
{
    private string _commandId;
    private string _commandDescription;
    private string _commandFormat;

    public string commandId { get { return _commandId; } }
    public string commandDescription { get { return _commandDescription; } }
    public string commandFormat { get { return _commandFormat; } }

    public DebugCommandBase(string id, string description, string format)
    {
        _commandId = id;
        _commandDescription = description;
        _commandFormat = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private System.Action command;

    public DebugCommand(string id, string description, string format, System.Action command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke()
    {
        command.Invoke();
    }
}

public class DebugCommand<T1> : DebugCommandBase
{
    private System.Action<T1> command;

    public DebugCommand(string id, string description, string format, System.Action<T1> command) : base (id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T1 val)
    {
        command.Invoke(val);
    }
}

public class DebugCommand<T1, T2> : DebugCommandBase
{
    private System.Action<T1, T2> command;

    public DebugCommand(string id, string description, string format, System.Action<T1, T2> command) : base(id, description, format)
    {
        this.command = command;
    }

    public void Invoke(T1 val, T2 val2)
    {
        command.Invoke(val, val2);
    }
}

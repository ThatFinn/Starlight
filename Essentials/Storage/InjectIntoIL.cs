using System;

namespace Starlight.Storage;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class InjectIntoIL : Attribute
{
    public readonly bool LOGOnFail = true;
    public readonly bool LOGOnSuccess = false;
    public readonly Type[]? Interfaces = null;
    public InjectIntoIL() {}

    public InjectIntoIL(bool logOnFail)
    {
        this.LOGOnFail = logOnFail;
    }
    
    public InjectIntoIL(bool logOnFail, bool logOnSuccess)
    {
        this.LOGOnFail = logOnFail;
        this.LOGOnSuccess = logOnSuccess;
    }
    public InjectIntoIL(bool logOnFail, params Type[]? interfaces)
    {
        this.LOGOnFail = logOnFail;
        this.Interfaces = interfaces;
    }
    public InjectIntoIL(bool logOnFail, bool logOnSuccess, params Type[]? interfaces)
    {
        this.LOGOnFail = logOnFail;
        this.LOGOnSuccess = logOnSuccess;
        this.Interfaces = interfaces;
    }
    public InjectIntoIL(params Type[]? interfaces)
    {
        this.LOGOnFail = true;
        this.Interfaces = interfaces;
    }
}
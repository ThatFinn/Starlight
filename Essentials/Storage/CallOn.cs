using System;
using Starlight.Enums;

namespace Starlight.Storage;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class CallOnAttribute(CallEvent callEvent) : Attribute
{
    public CallEvent callEvent { get; } = callEvent;
}
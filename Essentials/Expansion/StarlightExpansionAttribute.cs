using System;

namespace Starlight.Expansion;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class StarlightLoadExpansionAttribute : Attribute
{
    public StarlightLoadExpansionAttribute()
    {
        
    }
}
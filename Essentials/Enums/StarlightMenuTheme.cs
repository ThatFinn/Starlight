using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Starlight.Enums;

[Serializable]
[JsonConverter(typeof(StringEnumConverter))]
public enum StarlightMenuTheme
{
    None=0,
    Starlight=1,
    Native=2,
    Black=3,
    Melon=4,
}
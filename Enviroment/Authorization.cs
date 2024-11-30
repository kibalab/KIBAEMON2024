using Newtonsoft.Json;

namespace KIBAEMON2024_CSharp.Enviroment;

[Serializable]
public class Authorization
{
    public static Authorization Empty => new();

    public string Token { get; set; } = string.Empty;
}
namespace KIBAEMON2024_CSharp.System;

public class CommandParameters(Dictionary<string, object> parameters)
{
    public T? Get<T>(string key)
    {
        if (parameters.TryGetValue(key, out var value) && value is T tVal)
        {
            return tVal;
        }

        return default;
    }

    public object? this[string key] => parameters.ContainsKey(key) ? parameters[key] : null;
}
using Newtonsoft.Json;

namespace KIBAEMON2024_CSharp.Enviroment;

[Serializable]
public class BotEnvironment
{
    public Authorization Authorization { get; set; } = Authorization.Empty;

    public static Dictionary<string, BotEnvironment> Bots { get; set; } = new();

    public static void Initialize()
    {
        if (File.Exists("Bots.json"))
        {
            var authFile = File.ReadAllText("Bots.json");
            Bots = JsonConvert.DeserializeObject<Dictionary<string, BotEnvironment>>(authFile) ?? new Dictionary<string, BotEnvironment>();
        }
        else
        {
            Bots = new Dictionary<string, BotEnvironment>
            {
                { "UnknownBot", new BotEnvironment { Authorization = Authorization.Empty } }
            };

            var emptyFile = JsonConvert.SerializeObject(Bots);
            var writer = File.CreateText("Bots.json");

            writer.Write(emptyFile);
            writer.Dispose();
        }
    }
}
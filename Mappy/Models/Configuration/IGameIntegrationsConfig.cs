using KamiLib.AutomaticUserInterface;

namespace Mappy.Models;

[Category("GameIntegrations", 1)]
public interface IGameIntegrationsConfig
{
    [BoolDescriptionConfig("EnableIntegrations", "IntegrationsDescription")]
    public bool EnableIntegrations { get; set; }

    [BoolConfig("InsertFlagInChat", "InsertFlagHelp")]
    public bool InsertFlagInChat { get; set; }
}
using Dalamud.Interface.Internal;
using KamiLib.Classes;

namespace Mappy.Classes.Caches;

public class TextureCache : Cache<string, IDalamudTextureWrap> {
    protected override IDalamudTextureWrap? LoadValue(string key) 
        => Service.TextureProvider.GetTextureFromGame(key);
}
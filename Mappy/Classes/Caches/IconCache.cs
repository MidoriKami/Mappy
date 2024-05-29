using Dalamud.Interface.Internal;
using KamiLib.Classes;

namespace Mappy.Classes.Caches;

public class IconCache : Cache<uint, IDalamudTextureWrap?> {
    protected override IDalamudTextureWrap? LoadValue(uint key) 
        => Service.TextureProvider.GetIcon(key);
}
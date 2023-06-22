using System.Numerics;
using DailyDuty.System;
using ImGuiNET;
using KamiLib.AutomaticUserInterface;
using Mappy.Models;
using Mappy.Models.Enums;

namespace Mappy.Abstracts;

public abstract class ModuleBase
{
    public abstract ModuleName ModuleName { get; }
    public abstract ModuleConfigBase Configuration { get; protected set; }
    protected T GetConfig<T>() where T : ModuleConfigBase => (T) Configuration;

    // Map Marker
    public virtual void ZoneChanged(uint territoryType) { }
    public abstract void LoadForMap(uint newMapId);
    protected abstract void DrawMarkers();
    protected virtual bool ShouldDrawMarkers()
    {
        if (!Configuration.Enable) return false;

        return true;
    }
    public void Draw()
    {
        if (!ShouldDrawMarkers()) return;
        
        DrawMarkers();
    }

    // Tooltip
    public virtual bool HasTooltip { get; protected set; } = true;
    public virtual bool ShouldShowTooltip() { return true; }
    protected void DrawTooltip(string text, Vector4 color)
    {
        if (!ImGui.IsItemHovered()) return;
        
        ImGui.BeginTooltip();
        ImGui.TextColored(color, text);
        ImGui.EndTooltip();
    }
    
    // File IO
    public virtual void Load() => Configuration = LoadConfig();
    public virtual void Unload() { }
    public void DrawConfig() => DrawableAttribute.DrawAttributes(Configuration, SaveConfig);
    private ModuleConfigBase LoadConfig() => FileController.LoadFile<ModuleConfigBase>($"{ModuleName}.config.json", Configuration);
    public void SaveConfig() => FileController.SaveFile($"{ModuleName}.config.json", Configuration.GetType(), Configuration);
}
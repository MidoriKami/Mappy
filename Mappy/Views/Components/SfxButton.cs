using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using ImGuiNET;
using KamiLib.Utilities;
using Mappy.Utility;

namespace Mappy.Views.Components;

public class SfxButton
{
    public required string Label { get; init; }
    public required Action ClickAction { get; init; }
    public uint? MouseOverSfxId { get; init; }
    public uint? ClickSfxId { get; init; }
    public Vector2? Size { get; init; }
    public bool IsIconButton { get; init; }
    public string? TooltipText { get; init; }

    private bool isMouseOver;

    public void Draw()
    {
        if(IsIconButton) ImGui.PushFont(UiBuilder.IconFont);
        if (ImGui.Button(Label, Size ?? Vector2.Zero))
        {
            ClickAction.Invoke();

            if (ClickSfxId is { } clickSfx)
            {
                UIModule.PlaySound(clickSfx);
            }
        }
        if(IsIconButton) ImGui.PopFont();

        if (ImGui.IsItemHovered() && MouseOverSfxId is { } sfxId)
        {
            // We weren't hovered, but are now
            if (!isMouseOver)
            {
                UIModule.PlaySound(sfxId);
                isMouseOver = true;
            }
        }
        else
        {
            isMouseOver = false;
        }

        if (TooltipText is not null) DrawUtilities.DrawTooltip(KnownColor.White.AsVector4(), TooltipText);
    }
}
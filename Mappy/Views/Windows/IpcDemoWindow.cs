using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;
using ImGuiNET;
using KamiLib;
using KamiLib.Command;
using KamiLib.System;
using Mappy.System.Localization;

namespace Mappy.Views.Windows; 

public class IpcDemoWindow : Window {
    // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
    public ICallGateSubscriber<bool> IsReadyIpcFunction;

    public ICallGateSubscriber<uint, Vector2, uint, string, string, string> AddWorldMarkerIpcFunction;
    public ICallGateSubscriber<uint, Vector2, uint, string, string, string> AddTextureMarkerIpcFunction;
    public ICallGateSubscriber<uint, Vector2, uint, string, string, string> AddMapCoordinateMarkerIpcFunction;

    public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string> AddTextureLineIpcFunction;
    public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string> AddWorldLineIpcFunction;
    public ICallGateSubscriber<Vector2, Vector2, uint, Vector4, float, string> AddMapCoordLineIpcFunction;

    public ICallGateSubscriber<Vector2, uint, Vector4, Vector4, int, float, string> AddWorldShapeIpcFunction;
    public ICallGateSubscriber<Vector2, uint, Vector4, Vector4, int, float, string> AddTextureShapeIpcFunction;
    public ICallGateSubscriber<Vector2, uint, Vector4, Vector4, int, float, string> AddMapShapeIpcFunction;
    
    public ICallGateSubscriber<string, bool> RemoveIpcFunction;
    public ICallGateSubscriber<string, Vector2, bool> UpdateMarkerIpcFunction; 

    private bool ipcTestRan;
    private bool ipcTestResult;
    
    public IpcDemoWindow() : base("MappyIPC Demo Window") {
        SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(560, 475),
            MaximumSize = new Vector2(9999,9999),
        };
        
        CommandController.RegisterCommands(this);
        
        // Copy/Paste this to subscribe to these functions, be sure to check for IPCNotReady exceptions ;)
        IsReadyIpcFunction = Service.PluginInterface.GetIpcSubscriber<bool>("Mappy.IsReady");
        
        AddWorldMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.World.AddMarker");
        AddTextureMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.Texture.AddMarker");
        AddMapCoordinateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<uint, Vector2, uint, string, string, string>("Mappy.MapCoord.AddMarker");

        AddTextureLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.Texture.AddLine");
        AddWorldLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.World.AddLine");
        AddMapCoordLineIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, Vector2, uint, Vector4, float, string>("Mappy.MapCoord.AddLine");

        AddWorldShapeIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.World.AddShape");
        AddTextureShapeIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.Texture.AddShape");
        AddMapShapeIpcFunction = Service.PluginInterface.GetIpcSubscriber<Vector2, uint, Vector4, Vector4, int, float, string>("Mappy.MapCoord.AddShape");
        
        UpdateMarkerIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, Vector2, bool>("Mappy.UpdateMarker");
        RemoveIpcFunction = Service.PluginInterface.GetIpcSubscriber<string, bool>("Mappy.Remove");
    }
    
    public override void Draw() {
        try {
            if (ImGui.BeginTable("IPCTestTable", 2, ImGuiTableFlags.SizingStretchProp)) {
                IsReadyDemo();
                MarkerDemo();
                UpdateDemo();
                LineDemo();
                ShapeDemo();

                ImGui.EndTable();
            }
            
            UpdateDemoLogic();
        }
        catch (IpcNotReadyError e) {
            Service.Log.Error(e, "IPC Error");
        }
    }

    private readonly List<string> worldMarkersDemo = new();
    // string AddWorldMarker(uint IconId, Vector2 WorldPosition, uint MapId, string Tooltip, string Description);
    private void MarkerDemo() {
        ImGui.TableNextColumn();
        if (ImGui.Button("Add Icon Markers Demo")) {
            worldMarkersDemo.AddRange(new [] {
                AddWorldMarkerIpcFunction.InvokeFunc(103, new Vector2(0.0f, 0.0f), 0, "Map Center", "A world marker at the center of the map"),
                AddWorldMarkerIpcFunction.InvokeFunc(107, new Vector2(0.0f, -1024.0f), 0, "Map Top", "A world marker at the top of the map"),
                AddWorldMarkerIpcFunction.InvokeFunc(102, new Vector2(0.0f, 1024.0f), 0, "Map Bottom", "A world marker at the bottom of the map"),
                AddWorldMarkerIpcFunction.InvokeFunc(113, new Vector2(-1024.0f, 0.0f), 0, "Map Left", "A world marker at the Left of the map"),
                AddWorldMarkerIpcFunction.InvokeFunc(114, new Vector2(1024.0f, 0.0f), 0, "Map Right", "A world marker at the Right of the map"),
            });
        }

        ImGui.TableNextColumn();
        if (ImGui.Button("Remove Icon Markers Demo")) {
            foreach (var marker in worldMarkersDemo) {
                RemoveIpcFunction.InvokeFunc(marker);
            }
        }
    }

    private string? updateDemoMarker;
    private bool updateDemoActive;
    private Vector2 updatePosition;
    private bool moveRight = true;
    private void UpdateDemo() {
        ImGui.TableNextColumn();
        if (ImGui.Button("Add Update Demo")) {
            updateDemoMarker ??= AddTextureMarkerIpcFunction.InvokeFunc(103, new Vector2(0.0f, 0.0f), 0, "Update Marker", "A marker that is being updated each frame");
            updateDemoActive = true;
        }

        ImGui.TableNextColumn();
        if (ImGui.Button("Remove Update Markers Demo")) {
            updateDemoActive = false;
            if (updateDemoMarker is not null) {
                RemoveIpcFunction.InvokeFunc(updateDemoMarker);
            }
        }
    }

    private void UpdateDemoLogic() {
        if (updateDemoActive && updateDemoMarker is not null) {
            if (moveRight) {
                if (updatePosition.X < 1024.0f) {
                    updatePosition.X++;
                }
                else {
                    moveRight = false;
                }
            }
            else {
                if (updatePosition.X > 0.0f) {
                    updatePosition.X--;
                }
                else {
                    moveRight = true;
                }
            }

            UpdateMarkerIpcFunction.InvokeFunc(updateDemoMarker, updatePosition);
        }
    }

    private readonly List<string> lineDemo = new();
    // string AddLineMarker(Vector2 Start, Vector2 Stop, uint MapId, Vector4 Color, float Thickness);
    private void LineDemo() {
        ImGui.TableNextColumn();
        if (ImGui.Button("Add Lines Demo")) {
            lineDemo.AddRange(new [] {
                AddWorldLineIpcFunction.InvokeFunc(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1024.0f), 0, KnownColor.Red.Vector(), 2.0f),
                AddWorldLineIpcFunction.InvokeFunc(new Vector2(0.0f, 0.0f), new Vector2(0.0f, -1024.0f), 0, KnownColor.Green.Vector(), 2.0f),
                AddWorldLineIpcFunction.InvokeFunc(new Vector2(0.0f, 0.0f), new Vector2(1024.0f, 0.0f), 0, KnownColor.Blue.Vector(), 2.0f),
                AddWorldLineIpcFunction.InvokeFunc(new Vector2(0.0f, 0.0f), new Vector2(-1024.0f, 0.0f), 0, KnownColor.Yellow.Vector(), 2.0f),
            });
        }

        ImGui.TableNextColumn();
        if (ImGui.Button("Remove Lines Demo")) {
            foreach (var marker in lineDemo) {
                RemoveIpcFunction.InvokeFunc(marker);
            }
        }
    }

    private readonly List<string> shapesDemo = new();
    // string AddWorldShape(Vector2 WorldPosition, uint MapId, Vector4 outlineColor, Vector4 fillColor, int segments, float radius, string Tooltip, string Description);
    private void ShapeDemo() {
        ImGui.TableNextColumn();
        if (ImGui.Button("Add Shapes Markers Demo")) {
            shapesDemo.AddRange(new [] {
                AddWorldShapeIpcFunction.InvokeFunc(new Vector2(-512.0f, 512.0f), 0, KnownColor.Red.Vector(), KnownColor.Green.Vector(), 5, 30.0f),
                AddWorldShapeIpcFunction.InvokeFunc(new Vector2(-512.0f, -512.0f), 0, KnownColor.OrangeRed.Vector(), Vector4.Zero, 15, 55.0f),
                AddWorldShapeIpcFunction.InvokeFunc(new Vector2(512.0f, 512.0f), 0, KnownColor.Green.Vector(), KnownColor.Green.Vector(), 50, 10.0f),
                AddWorldShapeIpcFunction.InvokeFunc(new Vector2(512.0f, -512.0f), 0, KnownColor.Purple.Vector(), KnownColor.Green.Vector(), 3, 85.0f),
            });
        }

        ImGui.TableNextColumn();
        if (ImGui.Button("Remove Shapes Markers Demo")) {
            foreach (var marker in shapesDemo) {
                RemoveIpcFunction.InvokeFunc(marker);
            }
        }
    }

    /// <summary>
    /// Mappy.IsReady
    /// </summary>
    private void IsReadyDemo() {
        ImGui.TableNextColumn();
        if (ImGui.Button("Test IPC Ready")) {
            ipcTestRan = true;
            try {
                ipcTestResult = IsReadyIpcFunction.InvokeFunc();
            }
            catch (IpcNotReadyError) {
                ipcTestResult = false;
            }
        }

        ImGui.TableNextColumn();
        ImGui.Text(ipcTestRan ? ipcTestResult ? "IPC Successful" : "IPC Failed" : "Test not run.");
    }

    // ReSharper disable once UnusedMember.Local
    [SingleTierCommandHandler("OpenIPCDemo", "ipcdemo")]
    private void OpenDemo() {
        if (Service.ClientState is { IsPvP: true }) {
            Service.Chat.PrintError(Strings.PvPError);
            return;
        }
        
        if (KamiCommon.WindowManager.GetWindowOfType<MapWindow>() is { } mapWindow) {
            mapWindow.IsOpen = true;
        }
        
        IsOpen = true;
    }
}
using ExileCore;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared;
using ExileCore.Shared.Enums;
using Microsoft.VisualBasic.Devices;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Vector2 = System.Numerics.Vector2;
using ExileCore.PoEMemory.Components;

namespace ScarabVendor;

public class ScarabVendor : BaseSettingsPlugin<ScarabVendorSettings>
{
    private SyncTask<bool> _autoLoopTask;
    private bool _stopRequested = false;

    public override bool Initialise()
    {
        return true;
    }

    public override void AreaChange(AreaInstance area)
    {
    }

    public override Job Tick()
    {
        if (Input.IsKeyDown(Keys.F3) && _autoLoopTask == null)
        {
            _stopRequested = false;
            _autoLoopTask = RunFullAutoLoop();
        }

        if (Input.IsKeyDown(Keys.F5) && _autoLoopTask == null)
        {
            _stopRequested = false;
            _autoLoopTask = RunFaustusCollectionLoop();
        }

        if (Input.IsKeyDown(Keys.F4))
        {
            _stopRequested = true;
        }
        return null;
    }

    public override void Render()
    {
        if (_autoLoopTask != null)
            TaskUtils.RunOrRestart(ref _autoLoopTask, () => null);
    }

    private async SyncTask<bool> RunFullAutoLoop()
    {
        try
        {
            while (!_stopRequested)
            {
                var windowOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;

                if (!GameController.IngameState.IngameUi.StashElement.IsVisible)
                {
                    break;
                }

                var highlightItems = GameController.IngameState.IngameUi.StashElement.VisibleStash.VisibleInventoryItems
                    .Where(x => x.isHighlighted)
                    .OrderBy(x => x.GetClientRect().X)
                    .ThenBy(x => x.GetClientRect().Y)
                    .ToList();

                if (highlightItems.Count == 0)
                {
                    break;
                }

                Input.KeyDown(Keys.LControlKey);
                foreach (var item in highlightItems)
                {
                    if (_stopRequested) break;

                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(150, 201));

                    int currentCount = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
                        .Sum(x => x.Item?.GetComponent<ExileCore.PoEMemory.Components.Stack>()?.Size ?? 1);

                    if (currentCount >= 150) break;

                    var rect = item.GetClientRect();
                    Input.SetCursorPos(new System.Numerics.Vector2(rect.Center.X + windowOffset.X, rect.Center.Y + windowOffset.Y));
                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(30, 51));
                    Input.Click(MouseButtons.Left);
                }
                Input.KeyUp(Keys.LControlKey);
                if (_stopRequested) break;

                Input.KeyDown(Keys.Space); await TaskUtils.NextFrame(); Input.KeyUp(Keys.Space);
                await System.Threading.Tasks.Task.Delay(Random.Shared.Next(400, 601));

                var helena = GameController.Entities.FirstOrDefault(x => x.Path.Contains("Helena") && x.IsTargetable);
                if (helena != null)
                {
                    var screenPos = GameController.IngameState.Camera.WorldToScreen(helena.Pos);
                    Input.SetCursorPos(new System.Numerics.Vector2(screenPos.X + windowOffset.X, screenPos.Y + windowOffset.Y));
                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 151));

                    Input.KeyDown(Keys.LControlKey);
                    Input.Click(MouseButtons.Left);
                    Input.KeyUp(Keys.LControlKey);

                    for (int i = 0; i < 40 && !GameController.IngameState.IngameUi.SellWindow.IsVisible; i++) await TaskUtils.NextFrame();

                    if (GameController.IngameState.IngameUi.SellWindow.IsVisible)
                    {
                        var invItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
                            .Where(x => x.Item != null)
                            .OrderBy(x => x.GetClientRect().X)
                            .ThenBy(x => x.GetClientRect().Y)
                            .ToList();

                        Input.KeyDown(Keys.LControlKey);
                        foreach (var invItem in invItems)
                        {
                            if (_stopRequested) break;
                            var rect = invItem.GetClientRect();
                            Input.SetCursorPos(new System.Numerics.Vector2(rect.Center.X + windowOffset.X, rect.Center.Y + windowOffset.Y));
                            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(80, 151));
                            Input.Click(MouseButtons.Left);
                        }
                        Input.KeyUp(Keys.LControlKey);
                        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(300, 501));

                        var btn = GameController.IngameState.IngameUi.SellWindow.AcceptButton;
                        if (btn != null && btn.IsVisible)
                        {
                            Input.SetCursorPos(new System.Numerics.Vector2(btn.GetClientRect().Center.X + windowOffset.X, btn.GetClientRect().Center.Y + windowOffset.Y));
                            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(80, 151));
                            Input.Click(MouseButtons.Left);
                            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(900, 1201));
                        }
                    }
                }
                if (_stopRequested) break;

                var stashObj = GameController.Entities.FirstOrDefault(x =>
                x.Path.Contains("MiscellaneousObjects/Stash") &&              
                x.IsTargetable);

                if (stashObj != null)
                {
                    var sPos = GameController.IngameState.Camera.WorldToScreen(stashObj.Pos);
                    Input.SetCursorPos(new System.Numerics.Vector2(sPos.X + windowOffset.X, sPos.Y + windowOffset.Y));
                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 151));
                    Input.Click(MouseButtons.Left);

                    for (int i = 0; i < 40 && !GameController.IngameState.IngameUi.StashElement.IsVisible; i++) await TaskUtils.NextFrame();

                    if (GameController.IngameState.IngameUi.StashElement.IsVisible)
                    {
                        var finalInv = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
                            .Where(x => x.Item != null)
                            .OrderBy(x => x.GetClientRect().X)
                            .ThenBy(x => x.GetClientRect().Y)
                            .ToList();

                        Input.KeyDown(Keys.LControlKey);
                        foreach (var invItem in finalInv)
                        {
                            if (_stopRequested) break;
                            var rect = invItem.GetClientRect();
                            Input.SetCursorPos(new System.Numerics.Vector2(rect.Center.X + windowOffset.X, rect.Center.Y + windowOffset.Y));
                            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 161));
                            Input.Click(MouseButtons.Left);
                        }
                        Input.KeyUp(Keys.LControlKey);
                        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(400, 601));
                    }
                }
            }
        }
        finally
        {
            Input.KeyUp(Keys.LControlKey);
            _autoLoopTask = null;
            _stopRequested = false;
        }
        return true;
    }

    private async SyncTask<bool> RunFaustusCollectionLoop()
    {
        try
        {
            while (!_stopRequested)
            {
                var windowOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;

                if (!await OpenFaustusCurrencyExchange(windowOffset))
                {
                    break;
                }

                var orderChildren = GameController.IngameState.IngameUi.CurrencyExchangePanel.OrderElements.Children;
                if (orderChildren.Count <= 4 || !orderChildren[3].IsVisible || !orderChildren[4].IsVisible)
                {
                    break;
                }

                var completedOrder = orderChildren[3];
                if (!string.Equals(completedOrder.Text, "Order Completed", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var inventoryStackCountBefore = GetInventoryStackCount();
                var collectRect = orderChildren[4].GetClientRect();
                Input.SetCursorPos(new System.Numerics.Vector2(
                    collectRect.Center.X + windowOffset.X,
                    collectRect.Center.Y + windowOffset.Y));
                await System.Threading.Tasks.Task.Delay(Random.Shared.Next(80, 151));

                Input.KeyDown(Keys.LControlKey);
                Input.Click(MouseButtons.Right);
                Input.KeyUp(Keys.LControlKey);
                await System.Threading.Tasks.Task.Delay(Random.Shared.Next(400, 601));

                if (_stopRequested || GetInventoryStackCount() <= inventoryStackCountBefore)
                {
                    break;
                }

                Input.KeyDown(Keys.Space);
                await TaskUtils.NextFrame();
                Input.KeyUp(Keys.Space);
                await System.Threading.Tasks.Task.Delay(Random.Shared.Next(300, 501));

                if (!await DepositInventoryIntoStash(windowOffset))
                {
                    break;
                }
            }
        }
        finally
        {
            Input.KeyUp(Keys.LControlKey);
            _autoLoopTask = null;
            _stopRequested = false;
        }

        return true;
    }

    private async SyncTask<bool> OpenFaustusCurrencyExchange(SharpDX.Vector2 windowOffset)
    {
        if (GameController.IngameState.IngameUi.CurrencyExchangePanel.IsVisible)
        {
            return true;
        }

        var faustus = GameController.Entities.FirstOrDefault(x =>
            string.Equals(x.Path, "Metadata/NPC/League/Kalguur/VillageFaustusHideout", StringComparison.OrdinalIgnoreCase) &&
            x.IsTargetable);
        if (faustus == null)
        {
            return false;
        }

        var screenPos = GameController.IngameState.Camera.WorldToScreen(faustus.Pos);
        Input.SetCursorPos(new System.Numerics.Vector2(screenPos.X + windowOffset.X, screenPos.Y + windowOffset.Y));
        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 151));

        Input.KeyDown(Keys.LControlKey);
        Input.Click(MouseButtons.Left);
        Input.KeyUp(Keys.LControlKey);

        for (var index = 0; index < 40 && !GameController.IngameState.IngameUi.CurrencyExchangePanel.IsVisible; index++)
        {
            await TaskUtils.NextFrame();
        }

        return GameController.IngameState.IngameUi.CurrencyExchangePanel.IsVisible;
    }

    private async SyncTask<bool> DepositInventoryIntoStash(SharpDX.Vector2 windowOffset)
    {
        if (!GameController.IngameState.IngameUi.StashElement.IsVisible)
        {
            var stashObject = GameController.Entities.FirstOrDefault(x =>
                x.Path.Contains("MiscellaneousObjects/Stash") &&
                x.IsTargetable);

            if (stashObject == null)
            {
                return false;
            }

            var screenPos = GameController.IngameState.Camera.WorldToScreen(stashObject.Pos);
            Input.SetCursorPos(new System.Numerics.Vector2(screenPos.X + windowOffset.X, screenPos.Y + windowOffset.Y));
            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 151));
            Input.Click(MouseButtons.Left);

            for (var index = 0; index < 40 && !GameController.IngameState.IngameUi.StashElement.IsVisible; index++)
            {
                await TaskUtils.NextFrame();
            }

            if (!GameController.IngameState.IngameUi.StashElement.IsVisible)
            {
                return false;
            }
        }

        var inventoryItems = GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
            .Where(x => x.Item != null)
            .OrderBy(x => x.GetClientRect().X)
            .ThenBy(x => x.GetClientRect().Y)
            .ToList();

        if (inventoryItems.Count == 0)
        {
            return false;
        }

        Input.KeyDown(Keys.LControlKey);
        foreach (var inventoryItem in inventoryItems)
        {
            if (_stopRequested)
            {
                break;
            }

            var rect = inventoryItem.GetClientRect();
            Input.SetCursorPos(new System.Numerics.Vector2(rect.Center.X + windowOffset.X, rect.Center.Y + windowOffset.Y));
            await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 161));
            Input.Click(MouseButtons.Right);
        }
        Input.KeyUp(Keys.LControlKey);
        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(400, 601));

        return !_stopRequested;
    }

    private int GetInventoryStackCount()
    {
        return GameController.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems
            .Sum(x => x.Item?.GetComponent<Stack>()?.Size ?? 1);
    }

    public override void EntityAdded(Entity entity)
    {
    }
}

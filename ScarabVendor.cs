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
            LogFaustus("F5 pressed; starting collection loop.");
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
            LogFaustus($"Collection loop started. Inventory stack count: {GetInventoryStackCount()}.");
            while (!_stopRequested)
            {
                var windowOffset = GameController.Window.GetWindowRectangleTimeCache.TopLeft;

                if (!await OpenFaustusCurrencyExchange(windowOffset))
                {
                    LogFaustus("Stopping: Currency Exchange did not open.");
                    break;
                }

                LogFaustus($"Currency Exchange ready. Order rows: {GameController.IngameState.IngameUi.CurrencyExchangePanel.OrderElements.Count}; raw panel children: {GameController.IngameState.IngameUi.CurrencyExchangePanel.Children.Count}; inventory stack count: {GetInventoryStackCount()}.");
                LogCurrencyExchangeOrderRows();
                while (!_stopRequested)
                {
                    var completedOrder = GameController.IngameState.IngameUi.CurrencyExchangePanel.OrderElements
                        .FirstOrDefault(order =>
                            order.Children.Count > 4 &&
                            IsCompletedOrderStatus(order.Children[3].Text));
                    if (completedOrder == null)
                    {
                        LogFaustus("No visible completed order was found.");
                        break;
                    }

                    var inventoryStackCountBefore = GetInventoryStackCount();
                    var completedItem = completedOrder.Children[4];
                    var collectRect = completedItem.GetClientRect();
                    LogFaustus($"Completed order found. Ctrl + Right Clicking child 4; inventory stack count before: {inventoryStackCountBefore}.");
                    Input.SetCursorPos(new System.Numerics.Vector2(
                        collectRect.Center.X + windowOffset.X,
                        collectRect.Center.Y + windowOffset.Y));
                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(80, 151));

                    Input.KeyDown(Keys.LControlKey);
                    Input.Click(MouseButtons.Right);
                    Input.KeyUp(Keys.LControlKey);
                    await System.Threading.Tasks.Task.Delay(Random.Shared.Next(400, 601));

                    var inventoryStackCountAfter = GetInventoryStackCount();
                    LogFaustus($"Claim attempt complete. Inventory stack count: {inventoryStackCountBefore} -> {inventoryStackCountAfter}.");
                    if (inventoryStackCountAfter <= inventoryStackCountBefore)
                    {
                        LogFaustus("Claim did not change inventory; moving to the stash phase.");
                        LogCurrencyExchangeOrderRows();
                        break;
                    }
                }

                if (_stopRequested)
                {
                    LogFaustus("Stopping: collection was cancelled.");
                    break;
                }

                var inventoryStackCount = GetInventoryStackCount();
                if (inventoryStackCount == 0)
                {
                    LogFaustus("Stopping: no inventory items are available to stash.");
                    break;
                }

                LogFaustus($"Moving {inventoryStackCount} inventory stacks to stash.");
                if (!await CloseAllWindows())
                {
                    LogFaustus("Stopping: collection was cancelled while closing Currency Exchange.");
                    break;
                }

                if (!await DepositInventoryIntoStash(windowOffset))
                {
                    LogFaustus("Stopping: inventory could not be moved to stash.");
                    break;
                }

                if (!await CloseAllWindows())
                {
                    LogFaustus("Stopping: collection was cancelled while closing stash.");
                    break;
                }

                LogFaustus("Stash phase complete; returning to Faustus.");
            }
        }
        finally
        {
            Input.KeyUp(Keys.LControlKey);
            LogFaustus(_stopRequested ? "Collection loop stopped by F4." : "Collection loop finished.");
            _autoLoopTask = null;
            _stopRequested = false;
        }

        return true;
    }

    private async SyncTask<bool> OpenFaustusCurrencyExchange(SharpDX.Vector2 windowOffset)
    {
        var currencyExchangePanel = GameController.IngameState.IngameUi.CurrencyExchangePanel;
        if (currencyExchangePanel.IsVisible || currencyExchangePanel.OrderElements.Count > 0)
        {
            LogFaustus($"Currency Exchange data is already available. IsVisible: {currencyExchangePanel.IsVisible}; order rows: {currencyExchangePanel.OrderElements.Count}.");
            return true;
        }

        var faustus = GameController.Entities.FirstOrDefault(x =>
            string.Equals(x.Path, "Metadata/NPC/League/Kalguur/VillageFaustusHideout", StringComparison.OrdinalIgnoreCase) &&
            x.IsTargetable);
        if (faustus == null)
        {
            LogFaustus("Faustus was not found or is not targetable.");
            return false;
        }

        LogFaustus("Opening Currency Exchange with Ctrl + Left Click on Faustus.");
        var screenPos = GameController.IngameState.Camera.WorldToScreen(faustus.Pos);
        Input.SetCursorPos(new System.Numerics.Vector2(screenPos.X + windowOffset.X, screenPos.Y + windowOffset.Y));
        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(100, 151));

        Input.KeyDown(Keys.LControlKey);
        Input.Click(MouseButtons.Left);
        Input.KeyUp(Keys.LControlKey);

        for (var index = 0; index < 40 && !currencyExchangePanel.IsVisible && currencyExchangePanel.OrderElements.Count == 0; index++)
        {
            await TaskUtils.NextFrame();
        }

        LogFaustus($"Currency Exchange state after interaction. IsVisible: {currencyExchangePanel.IsVisible}; order rows: {currencyExchangePanel.OrderElements.Count}. Continuing with the order scan.");
        return true;
    }

    private async SyncTask<bool> CloseAllWindows()
    {
        LogFaustus("Pressing Space to close open windows.");
        Input.KeyDown(Keys.Space);
        await TaskUtils.NextFrame();
        Input.KeyUp(Keys.Space);
        await System.Threading.Tasks.Task.Delay(Random.Shared.Next(300, 501));

        return !_stopRequested;
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
                LogFaustus("Stash was not found or is not targetable.");
                return false;
            }

            LogFaustus("Opening stash.");
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
                LogFaustus("Stash did not open.");
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
            LogFaustus("No inventory items were found for the stash phase.");
            return false;
        }

        var inventoryStackCountBefore = GetInventoryStackCount();
        LogFaustus($"Ctrl + Right Clicking {inventoryItems.Count} inventory item(s) into stash.");
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

        LogFaustus($"Stash attempt complete. Inventory stack count: {inventoryStackCountBefore} -> {GetInventoryStackCount()}.");
        return !_stopRequested;
    }

    private void LogFaustus(string message)
    {
        LogMessage($"[Faustus] {message}", 5);
    }

    private static bool IsCompletedOrderStatus(string statusText)
    {
        var normalizedStatusText = string.Concat((statusText ?? string.Empty)
            .Where(character => !char.IsWhiteSpace(character)));
        return normalizedStatusText.Contains("OrderCompleted", StringComparison.OrdinalIgnoreCase);
    }

    private void LogCurrencyExchangeOrderRows()
    {
        var orderElements = GameController.IngameState.IngameUi.CurrencyExchangePanel.OrderElements;
        LogFaustus($"Dumping {orderElements.Count} Currency Exchange order row(s).");

        if (orderElements.Count == 0)
        {
            LogFaustus("OrderElements is empty; dumping the raw Currency Exchange panel tree.");
            LogRawCurrencyExchangePanelTree();
            return;
        }

        for (var orderIndex = 0; orderIndex < orderElements.Count; orderIndex++)
        {
            var order = orderElements[orderIndex];
            var orderRect = order.GetClientRect();
            var matchesCompletedOrder = order.Children.Count > 4 &&
                IsCompletedOrderStatus(order.Children[3].Text);

            LogFaustus($"Order row {orderIndex}: type={order.GetType().Name}; children={order.Children.Count}; completed-match={matchesCompletedOrder}; rect=({orderRect.X:F0},{orderRect.Y:F0},{orderRect.Width:F0},{orderRect.Height:F0}).");

            for (var childIndex = 0; childIndex < order.Children.Count; childIndex++)
            {
                var child = order.Children[childIndex];
                var childRect = child.GetClientRect();
                LogFaustus($"Order row {orderIndex}, child {childIndex}: type={child.GetType().Name}; visible={child.IsVisible}; text=\"{FormatElementText(child.Text)}\"; children={child.Children.Count}; rect=({childRect.X:F0},{childRect.Y:F0},{childRect.Width:F0},{childRect.Height:F0}).");

                for (var nestedChildIndex = 0; nestedChildIndex < child.Children.Count; nestedChildIndex++)
                {
                    var nestedChild = child.Children[nestedChildIndex];
                    var nestedChildRect = nestedChild.GetClientRect();
                    LogFaustus($"Order row {orderIndex}, child {childIndex}, nested child {nestedChildIndex}: type={nestedChild.GetType().Name}; visible={nestedChild.IsVisible}; text=\"{FormatElementText(nestedChild.Text)}\"; children={nestedChild.Children.Count}; rect=({nestedChildRect.X:F0},{nestedChildRect.Y:F0},{nestedChildRect.Width:F0},{nestedChildRect.Height:F0}).");
                }
            }
        }
    }

    private void LogRawCurrencyExchangePanelTree()
    {
        const int maximumNodesToLog = 150;
        var nodesLogged = 0;
        var panel = GameController.IngameState.IngameUi.CurrencyExchangePanel;
        var panelRect = panel.GetClientRect();
        LogFaustus($"Raw Currency Exchange panel: type={panel.GetType().Name}; children={panel.Children.Count}; rect=({panelRect.X:F0},{panelRect.Y:F0},{panelRect.Width:F0},{panelRect.Height:F0}).");

        for (var firstLevelIndex = 0; firstLevelIndex < panel.Children.Count && nodesLogged < maximumNodesToLog; firstLevelIndex++)
        {
            var firstLevelChild = panel.Children[firstLevelIndex];
            var firstLevelRect = firstLevelChild.GetClientRect();
            nodesLogged++;
            LogFaustus($"Panel child {firstLevelIndex}: type={firstLevelChild.GetType().Name}; visible={firstLevelChild.IsVisible}; completed-status={IsCompletedOrderStatus(firstLevelChild.Text)}; text=\"{FormatElementText(firstLevelChild.Text)}\"; children={firstLevelChild.Children.Count}; rect=({firstLevelRect.X:F0},{firstLevelRect.Y:F0},{firstLevelRect.Width:F0},{firstLevelRect.Height:F0}).");

            for (var secondLevelIndex = 0; secondLevelIndex < firstLevelChild.Children.Count && nodesLogged < maximumNodesToLog; secondLevelIndex++)
            {
                var secondLevelChild = firstLevelChild.Children[secondLevelIndex];
                var secondLevelRect = secondLevelChild.GetClientRect();
                nodesLogged++;
                LogFaustus($"Panel child {firstLevelIndex}, child {secondLevelIndex}: type={secondLevelChild.GetType().Name}; visible={secondLevelChild.IsVisible}; completed-status={IsCompletedOrderStatus(secondLevelChild.Text)}; text=\"{FormatElementText(secondLevelChild.Text)}\"; children={secondLevelChild.Children.Count}; rect=({secondLevelRect.X:F0},{secondLevelRect.Y:F0},{secondLevelRect.Width:F0},{secondLevelRect.Height:F0}).");

                for (var thirdLevelIndex = 0; thirdLevelIndex < secondLevelChild.Children.Count && nodesLogged < maximumNodesToLog; thirdLevelIndex++)
                {
                    var thirdLevelChild = secondLevelChild.Children[thirdLevelIndex];
                    var thirdLevelRect = thirdLevelChild.GetClientRect();
                    nodesLogged++;
                    LogFaustus($"Panel child {firstLevelIndex}, child {secondLevelIndex}, child {thirdLevelIndex}: type={thirdLevelChild.GetType().Name}; visible={thirdLevelChild.IsVisible}; completed-status={IsCompletedOrderStatus(thirdLevelChild.Text)}; text=\"{FormatElementText(thirdLevelChild.Text)}\"; children={thirdLevelChild.Children.Count}; rect=({thirdLevelRect.X:F0},{thirdLevelRect.Y:F0},{thirdLevelRect.Width:F0},{thirdLevelRect.Height:F0}).");
                }
            }
        }

        if (nodesLogged == maximumNodesToLog)
        {
            LogFaustus($"Raw panel tree logging stopped after {maximumNodesToLog} nodes.");
        }
    }

    private static string FormatElementText(string elementText)
    {
        return (elementText ?? "<null>")
            .Replace("\\", "\\\\")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\"", "\\\"");
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

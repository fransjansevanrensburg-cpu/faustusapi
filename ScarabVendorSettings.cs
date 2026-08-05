using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using ExileCore.Shared.Attributes;

namespace ScarabVendor;

public class ScarabVendorSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Hotkeys: F3 Helena sell | F5 Faustus collect | F4 stop")]
    public ToggleNode InstructionText { get; set; } = new(true);

    [Menu("Helena & Stash must be near")]
    public ToggleNode HelenaStash { get; set; } = new(true);

    [Menu("Faustus & Stash must be near; Currency Exchange opens with Ctrl + Click")]
    public ToggleNode FaustusStash { get; set; } = new(true);

    [Menu("Scarab Tab must be open with regex and empty inventory")]
    public ToggleNode ScarabTab { get; set; } = new(true);

    [Menu("regex from poe.re")]
    public ToggleNode Regex { get; set; } = new(true);

    [Menu("Need change Close All User Interface in Input Setting game to Spacebar button")]
    public ToggleNode Spacebar { get; set; } = new(true);
}

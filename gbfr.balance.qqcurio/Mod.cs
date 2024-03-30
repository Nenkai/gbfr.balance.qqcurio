using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Diagnostics;

using gbfr.balance.qqcurio.Template;

using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.ReloadedII.Interfaces;
using Reloaded.Mod.Interfaces;
using Reloaded.Memory.SigScan.ReloadedII.Interfaces;
using IReloadedHooks = Reloaded.Hooks.ReloadedII.Interfaces.IReloadedHooks;

namespace gbfr.balance.qqcurio
{
    /// <summary>
    /// Your mod logic goes here.
    /// </summary>
    public class Mod : ModBase // <= Do not Remove.
    {
        /// <summary>
        /// Provides access to the mod loader API.
        /// </summary>
        private readonly IModLoader _modLoader;

        /// <summary>
        /// Provides access to the Reloaded.Hooks API.
        /// </summary>
        /// <remarks>This is null if you remove dependency on Reloaded.SharedLib.Hooks in your mod.</remarks>
        private readonly IReloadedHooks? _hooks;

        /// <summary>
        /// Provides access to the Reloaded logger.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Entry point into the mod, instance that created this class.
        /// </summary>
        private readonly IMod _owner;

        /// <summary>
        /// The configuration of the currently executing mod.
        /// </summary>
        private readonly IModConfig _modConfig;

        private static IStartupScanner? _startupScanner = null!;

        private IHook<OnGiveItemDelegate> _giveItemHook;

        public unsafe Mod(ModContext context)
        {
            _modLoader = context.ModLoader;
            _hooks = context.Hooks;
            _logger = context.Logger;
            _owner = context.Owner;
            _modConfig = context.ModConfig;


            var startupScannerController = _modLoader.GetController<IStartupScanner>();
            if (startupScannerController == null || !startupScannerController.TryGetTarget(out _startupScanner))
            {
                return;
            }

            SigScan("55 41 57 41 56 41 55 41 54 56 57 53 48 81 EC ?? ?? ?? ?? 48 8D AC 24 ?? ?? ?? ?? " +
                "48 83 E4 ?? 48 89 E3 48 89 AB ?? ?? ?? ?? 48 C7 85 ?? ?? ?? ?? ?? ?? ?? ?? 45 85 C0 0F 84", "", address =>
            {
                _giveItemHook = _hooks!.CreateHook<OnGiveItemDelegate>(OnGiveItem_Hook, address).Activate();
                _logger.WriteLine($"[gbfr.balance.qqcurio] Successfully hooked OnGiveItem (0x{address:X8})", _logger.ColorGreen);
            });
        }

        private static void SigScan(string pattern, string name, Action<nint> action)
        {
            var baseAddress = Process.GetCurrentProcess().MainModule!.BaseAddress;
            _startupScanner?.AddMainModuleScan(pattern, result =>
            {
                if (!result.Found)
                {
                    return;
                }
                action(result.Offset + baseAddress);
            });
        }
        
        public unsafe delegate void OnGiveItemDelegate(byte* a1, uint itemIdHash, uint count, bool flag);

        // used by a method in ui::component::ControllerGetGoldTicketWindow
        public unsafe void OnGiveItem_Hook(byte* a1, uint itemId, uint count, bool flag)
        {
            Console.WriteLine($"{itemId:X8} (x{count}) - {flag}");
            if (itemId == XXHash32Custom.Hash("ITEM_36_0000")) // Gold Badge Ticket
                _giveItemHook.OriginalFunction(a1, XXHash32Custom.Hash("ITEM_19_0004"), 1, true); // Curio T4

            _giveItemHook.OriginalFunction(a1, itemId, count, flag);
        }

        #region For Exports, Serialization etc.
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public Mod() { }
#pragma warning restore CS8618
        #endregion
    }
}
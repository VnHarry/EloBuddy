﻿using EloBuddy.SDK;
using EloBuddy.SDK.Menu;

namespace VnHarry_AIO.Internal
{
    internal class Variables
    {
        public static Menu InfoMenu;
        public static Menu Config;
        public static Menu Activator;
        public static bool NoneMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.None;
        public static bool ComboMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo;
        public static bool HarassMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass;
        public static bool LaneClearMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear || Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.JungleClear;
        public static bool LastHitMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit;
    }
}
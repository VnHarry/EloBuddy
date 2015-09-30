using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace VnHarry_AIO.Internal
{
    internal class Variables
    {
        public static Menu InfoMenu;
        public static Menu Config;
        public static Menu Activator;
        public static bool ComboMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Combo;
        public static bool HarassMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.Harass;
        public static bool LaneClearMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LaneClear;
        public static bool LastHitMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.LastHit;
        public static bool NoneMode = Orbwalker.ActiveModesFlags == Orbwalker.ActiveModes.None;

        public static bool GetCheckBoxConfig(string pString)
        {
            return Config[pString].Cast<CheckBox>().CurrentValue;
        }
        public static int GetSliderConfig(string pString)
        {
            return Config[pString].Cast<Slider>().CurrentValue;
        }
        public static bool GetKeyConfig(string pString)
        {
            return Config[pString].Cast<KeyBind>().CurrentValue;
        }
    }
}
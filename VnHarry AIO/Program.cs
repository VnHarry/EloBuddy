using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Drawing;
using System.Globalization;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Marksman;
using VnHarry_AIO.Utilities;
using Champion = VnHarry_AIO.Internal.Champion;
using Core = VnHarry_AIO.Activator.Core;

namespace VnHarry_AIO
{
    internal class Program
    {
        public static PluginBase ChampionPlugin;

        public static AIHeroClient _Player
        {
            get { return ObjectManager.Player; }
        }

        private static void Main(string[] args)
        {
            try
            {
                Loading.OnLoadingComplete += Loading_OnLoadingComplete;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            TargetSelector2.init();
            Bootstrap.Init(null);
            VnHarryFarmLogic.init();
            VnHarryDrawLogic.init();

            Variables.InfoMenu = MainMenu.AddMenu("VnHarry AIO", "MarksmanBuddy");
            Variables.InfoMenu.AddGroupLabel("VnHarry AIO");
            Variables.InfoMenu.AddLabel("Version: " + "1.0.0.0");
            Variables.InfoMenu.AddSeparator();
            Variables.InfoMenu.AddLabel("Supported Champions: ");
            Variables.InfoMenu.AddLabel("Corki ");
            Variables.InfoMenu.AddLabel("Draven ");
            Variables.InfoMenu.AddLabel("Graves ");
            Variables.InfoMenu.AddLabel("Kalista ");
            Variables.InfoMenu.AddLabel("Sivir ");
            Variables.InfoMenu.AddLabel("Vayne ");
            Variables.InfoMenu.AddSeparator();
            Variables.InfoMenu.AddLabel("Creators: " + "VnHarry");

            Variables.Activator = Variables.InfoMenu.AddSubMenu("MB Activator", "MBActivator");
            Variables.Activator.AddGroupLabel("Summoner Spells");
            Variables.Activator.Add("Activator.UseHeal", new CheckBox("Use Heal"));
            Variables.Activator.Add("Activator.UseHealPercent", new Slider("Use Heal when under X Percent Health", 35));
            Variables.Activator.AddGroupLabel("Potions");
            Variables.Activator.Add("Activator.UseHPPot", new CheckBox("Use Healing Potions"));
            Variables.Activator.Add("Activator.UseHPPotPercent",
                new Slider("Use Healing Potions when under X Percent Health", 60));
            Variables.Activator.Add("Activator.UseMPPot", new CheckBox("Use Mana Potions"));
            Variables.Activator.Add("Activator.UseMPPotPercent",
                new Slider("Use Mana Potions when under X Percent Mana", 40));
            Variables.Activator.AddGroupLabel("Items");
            Variables.Activator.Add("Activator.UseCutlass", new CheckBox("Use Cutlass in Combo"));
            Variables.Activator.Add("Activator.UseYoumuus", new CheckBox("Use Youmuu's in Combo"));
            Variables.Activator.Add("Activator.UseBotrK", new CheckBox("Use Blade of the Ruined King in Combo"));

            var _Activator = new Core();
            Chat.Print("VnHarry AIO - <font color=\"#FFFFFF\">Loaded</font>", Color.FromArgb(255, 210, 68, 74));
            var championName = ObjectManager.Player.ChampionName.ToLower(CultureInfo.InvariantCulture);
            Variables.Config = Variables.InfoMenu.AddSubMenu(Player.Instance.ChampionName, Player.Instance.ChampionName);
            Chat.Print("VnHarry AIO - <font color=\"#FFFFFF\">{0} Loaded</font>", Color.FromArgb(255, 210, 68, 74), ObjectManager.Player.ChampionName);
            switch (championName)
            {
                case "ashe":
                    ChampionPlugin = new Champion();
                    break;

                case "caitlyn":
                    ChampionPlugin = new Champion();
                    break;

                case "corki":
                    ChampionPlugin = new Corki();
                    break;

                case "draven":
                    ChampionPlugin = new Draven();
                    break;

                case "ezreal":
                    ChampionPlugin = new Champion();
                    break;

                case "graves":
                    ChampionPlugin = new Graves();
                    break;

                case "gnar":
                    ChampionPlugin = new Champion();
                    break;

                case "jinx":
                    ChampionPlugin = new Champion();
                    break;

                case "kalista":
                    ChampionPlugin = new Kalista();
                    break;

                case "kindred":
                    ChampionPlugin = new Champion();
                    break;

                case "kogmaw":
                    ChampionPlugin = new Champion();
                    break;

                case "lucian":
                    ChampionPlugin = new Champion();
                    break;

                case "missfortune":
                    ChampionPlugin = new Champion();
                    break;

                case "quinn":
                    ChampionPlugin = new Champion();
                    break;

                case "sivir":
                    ChampionPlugin = new Sivir();
                    break;

                case "teemo":
                    ChampionPlugin = new Champion();
                    break;

                case "tristana":
                    ChampionPlugin = new Champion();
                    break;

                case "twitch":
                    ChampionPlugin = new Champion();
                    break;

                case "urgot":
                    ChampionPlugin = new Champion();
                    break;

                case "vayne":
                    ChampionPlugin = new Vayne();
                    break;

                case "varus":
                    ChampionPlugin = new Champion();
                    break;
            }

            Game.OnTick += Game_OnTick;
        }

        private static void Game_OnTick(EventArgs args)
        {
            Variables.ComboMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo);
            Variables.HarassMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass);
            Variables.LaneClearMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear);
            Variables.LastHitMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit);
            Variables.NoneMode = Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None);
        }
    }
}
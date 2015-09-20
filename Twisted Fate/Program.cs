using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;

namespace VnHarry_Twisted_Fate
{
    class Program
    {
        static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static Spell.Skillshot Q;
        public static Spell.Targeted W;
        public static Spell.Targeted E;
        public static Spell.Active R;
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        public static int Mana { get { return (int)_Player.Mana; } }
        public static Menu TwistedFateMenu, QMenu, WMenu, LaneClearMenu, DrawingsMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {

            //if (ObjectManager.Player.BaseSkinName != "TwistedFate") return;

            TargetSelector2.init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 1450, SkillShotType.Linear, (int)0.25f, (int)1000f, (int)40f);

            TwistedFateMenu = MainMenu.AddMenu("VnHarry Twisted Fate", "tf");
            TwistedFateMenu.AddGroupLabel("Twisted Fate");
            TwistedFateMenu.AddSeparator();
            TwistedFateMenu.AddLabel("VnHarry Twisted Fate V1.0.0.0");

            //Q Menu
            QMenu = TwistedFateMenu.AddSubMenu("Q - Wildcards", "qsettings");
            QMenu.AddGroupLabel("Cast Q (tap)");
            QMenu.Add("qmenu.autoqi", new CheckBox("Auto-Q immobile"));
            QMenu.Add("qmenu.autoqd", new CheckBox("Auto-Q dashing"));

            //W Menu
            WMenu = TwistedFateMenu.AddSubMenu("W Settings", "wsettings");
            WMenu.AddGroupLabel("W Settings");
            WMenu.Add("wmenu.yellow", new KeyBind("Select Yellow", false, KeyBind.BindTypes.HoldActive, 'W'));
            WMenu.Add("wmenu.blue", new KeyBind("Select Blue", false, KeyBind.BindTypes.HoldActive, 'E'));
            WMenu.Add("wmenu.red", new KeyBind("Select Red", false, KeyBind.BindTypes.HoldActive, 'T'));

            LaneClearMenu = TwistedFateMenu.AddSubMenu("LaneClear Settings", "laneclearsettings");
            LaneClearMenu.Add("laneclear.mana", new Slider("Mana manager (%)", 20, 0, 100));
            
            //DrawingsMenu
            DrawingsMenu = TwistedFateMenu.AddSubMenu("Drawings Settings", "drawingsmenu");
            DrawingsMenu.AddGroupLabel("Drawings Settings");
            DrawingsMenu.Add("drawings.q", new CheckBox("Draw Q"));
            DrawingsMenu.Add("drawings.r", new CheckBox("Draw R"));
          
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_Player.IsDead)
            {
                if (Program.DrawingsMenu["drawings.q"].Cast<CheckBox>().CurrentValue)
                {
                    Drawing.DrawCircle(_Player.Position, Q.Range, System.Drawing.Color.BlueViolet);
                }

                if (Program.DrawingsMenu["drawings.r"].Cast<CheckBox>().CurrentValue)
                {
                    Drawing.DrawCircle(_Player.Position, 5500, System.Drawing.Color.BlueViolet);
                }
            }
        }

        private static void Game_OnTick(EventArgs args)
        {
            //
            if (Program.WMenu["wmenu.yellow"].Cast<KeyBind>().CurrentValue ||
                Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }
            if (Program.WMenu["wmenu.blue"].Cast<KeyBind>().CurrentValue)
            {
                CardSelector.StartSelecting(Cards.Blue);
            }

            if (Program.WMenu["wmenu.red"].Cast<KeyBind>().CurrentValue)
            {
                CardSelector.StartSelecting(Cards.Red);
            }
            //
            Program.WMenu["wmenu.yellow"].Cast<KeyBind>().CurrentValue = false;
            Program.WMenu["wmenu.blue"].Cast<KeyBind>().CurrentValue = false;
            Program.WMenu["wmenu.red"].Cast<KeyBind>().CurrentValue = false;
            //

            var autoQI = Program.QMenu["qmenu.autoqi"].Cast<CheckBox>().CurrentValue;
            var autoQD = Program.QMenu["qmenu.autoqi"].Cast<CheckBox>().CurrentValue;

            if (ObjectManager.Player.Spellbook.CanUseSpell(SpellSlot.Q) == SpellState.Ready)
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    var pred = Q.GetPrediction(enemy);
                    if ((pred.HitChance == HitChance.Immobile && autoQI) ||
                        (pred.HitChance == HitChance.Dashing && autoQD))
                    {
                        Q.Cast(enemy);
                    }
                }
            }
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                StateHandler.Combo();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateHandler.Harass();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateHandler.WaveClear();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LastHit))
            {
                StateHandler.LastHit();
            } 
        }
    }
}

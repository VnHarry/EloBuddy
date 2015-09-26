using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;

namespace VnHarry_Twisted_Fate
{
    internal class Program
    {
        private static void Main(string[] args)
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
            TwistedFateMenu.AddLabel("VnHarry Twisted Fate V1.0.0.1");

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
            LaneClearMenu.Add("laneclear.q", new CheckBox("Auto-Q", false));
            LaneClearMenu.Add("laneclear.w", new CheckBox("Auto-W", false));
            LaneClearMenu.Add("laneclear.mana", new Slider("Mana manager (%)", 50, 0, 100));

            //DrawingsMenu
            DrawingsMenu = TwistedFateMenu.AddSubMenu("Drawings Settings", "drawingsmenu");
            DrawingsMenu.AddGroupLabel("Drawings Settings");
            DrawingsMenu.Add("drawings.q", new CheckBox("Draw Q"));
            DrawingsMenu.Add("drawings.r", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && args.SData.Name == "gate")
            {
                CardSelector.StartSelecting(Cards.Yellow);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_Player.IsDead)
            {
                if (Program.DrawingsMenu["drawings.q"].Cast<CheckBox>().CurrentValue)
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Instance.Position);
                }

                if (Program.DrawingsMenu["drawings.r"].Cast<CheckBox>().CurrentValue)
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = 5500 }.Draw(Player.Instance.Position);
                }
                if (true)
                {
                    var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
                    foreach (Obj_AI_Minion minion in minionList)
                    {
                        {
                            if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true))
                                new Circle { Color = System.Drawing.Color.Lime, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                            else if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true) * 2)
                                new Circle { Color = System.Drawing.Color.Gold, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                        }
                    }
                }
                if (true)
                {
                    var target = Orbwalker.GetTarget();

                    if (target != null)
                        new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = target.BoundingRadius + 15 }.Draw(target.Position);
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

            if (Q.IsReady())
            {
                var heroes = HeroManager.Enemies.Where(t => t.IsFeared || t.IsCharmed || t.IsTaunted || t.IsRecalling);
                if (heroes != null)
                {
                    foreach (var enemy in heroes)
                    {
                        var pred = Q.GetPrediction(enemy);
                        Q.Cast(pred.CastPosition);
                    }
                }
            }

            if (Q.IsReady() && Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.None))
            {
                var enemy = TargetSelector.GetTarget(Q.Range, DamageType.Mixed);

                if (enemy != null)
                {
                    if (enemy.Health < _Player.GetSpellDamage(enemy, SpellSlot.Q))
                    {
                        var pred = Q.GetPrediction(enemy);
                        Q.Cast(pred.CastPosition);
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
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;

namespace VnHarry_Diana
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Loading.OnLoadingComplete += Loading_OnLoadingComplete;
        }

        public static Spell.Skillshot Q;
        public static Spell.Active W;
        public static Spell.Active E;
        public static Spell.Targeted R;

        public static SpellSlot ignite;
        public static bool gotIgnite = false;

        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        public static int Mana { get { return (int)_Player.Mana; } }
        public static Menu DianaMenu, ComboMenu, HarassMenu, LaneClearMenu, DrawingsMenu;

        private static void Loading_OnLoadingComplete(EventArgs args)
        {
            //if (ObjectManager.Player.BaseSkinName != "TwistedFate") return;

            TargetSelector2.init();
            Bootstrap.Init(null);

            Q = new Spell.Skillshot(SpellSlot.Q, 895, SkillShotType.Circular, (int)0.25f, (int)1400f, (int)195f);
            W = new Spell.Active(SpellSlot.W, 240);
            E = new Spell.Active(SpellSlot.E, 350);
            R = new Spell.Targeted(SpellSlot.R, 825);
            try
            {
                ignite = Player.Spells.FirstOrDefault(o => o.SData.Name == "summonerdot").Slot;
                gotIgnite = true;
            }
            catch { }
            TargetSelector.ActiveMode = TargetSelectorMode.LeastHealth;
            DamageIndicator.Initialize(GetComboDamage);

            DianaMenu = MainMenu.AddMenu("VnHarry Diana", "diana");
            DianaMenu.AddGroupLabel("Diana");
            DianaMenu.AddSeparator();
            DianaMenu.AddLabel("VnHarry Diana V1.0.0.0");
            //combo
            ComboMenu = DianaMenu.AddSubMenu("Combo", "combo");   
            ComboMenu.AddGroupLabel("Combo Settings");
            ComboMenu.Add("combo.q", new CheckBox("Use Q", true));
            ComboMenu.Add("combo.w", new CheckBox("Use W", true));
            ComboMenu.Add("combo.e", new CheckBox("Use E", true));
            ComboMenu.Add("combo.r", new CheckBox("Use R", true));
            ComboMenu.Add("combo.secure", new CheckBox("Use R to secure kill", true));
            ComboMenu.AddGroupLabel("R Settings");
            ComboMenu.Add("combo.mod", new Slider("Normal (Q->R) | Misaya Combo (R->Q)", 0, 0, 1));
            ComboMenu.Add("combo.misayaminrange", new Slider("R Minimum Range for Misaya", (int)(R.Range * 0.8), 0, (int)R.Range));
            ComboMenu.Add("combo.preventundertower", new Slider("Don't use ult if HP% <", 20, 0, 100));
            ComboMenu.Add("combo.UseSecondRLimitation", new Slider("Max close enemies for secure kill with R", 5, 1, 5));

            //Harass Menu
            HarassMenu = DianaMenu.AddSubMenu("HarassMenu", "harass");
            HarassMenu.Add("harass.q", new CheckBox("Use Q", true));
            HarassMenu.Add("harass.w", new CheckBox("Use W", true));
            HarassMenu.Add("harass.e", new CheckBox("Use E", true));
            HarassMenu.Add("harass.mana", new Slider("Mana manager (%)", 55, 0, 100));
            //LaneClear Menu
            LaneClearMenu = DianaMenu.AddSubMenu("LaneClear Settings", "laneclear");
            LaneClearMenu.Add("laneclear.q", new CheckBox("Use Q", true));
            LaneClearMenu.Add("laneclear.qcount", new Slider("Minions in range for Q", 2, 1, 5));
            LaneClearMenu.Add("laneclear.w", new CheckBox("Use W", true));
            LaneClearMenu.Add("laneclear.wcount", new Slider("Minions in range for W", 2, 1, 5));
            LaneClearMenu.Add("laneclear.e", new CheckBox("Use E", true));
            LaneClearMenu.Add("laneclear.ecount", new Slider("Minions in range for E", 2, 1, 5));
            LaneClearMenu.Add("laneclear.r", new CheckBox("Use E", false));

            LaneClearMenu.Add("laneclear.mana", new Slider("Mana manager (%)", 50, 0, 100));
            //DrawingsMenu
            DrawingsMenu = DianaMenu.AddSubMenu("Drawings Settings", "drawingsmenu");
            DrawingsMenu.AddGroupLabel("Drawings Settings");
            DrawingsMenu.Add("drawing.q", new CheckBox("Draw Q"));
            DrawingsMenu.Add("drawing.w", new CheckBox("Draw W"));
            DrawingsMenu.Add("drawing.e", new CheckBox("Draw E"));
            DrawingsMenu.Add("drawing.r", new CheckBox("Draw R"));

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
          
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
            {
                damage += QDamage(enemy);
            }

            if (W.IsReady())
            {
                damage += WDamage(enemy);
            }


            if (R.IsReady())
            {
                damage += RDamage(enemy);
            }

            return damage;
        }
        public static float QDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 60, 95, 130, 165, 200 }[Q.Level] + 0.7 * _Player.FlatMagicDamageMod));
        }
        public static float WDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 66, 102, 138, 174, 210 * 3 }[W.Level] + 0.6 * _Player.FlatMagicDamageMod));
        }
        public static float RDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 100, 160, 220 }[R.Level] + 0.6 * _Player.FlatMagicDamageMod));
        }
        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!_Player.IsDead)
            {
                var drawQ = DrawingsMenu["drawing.q"].Cast<CheckBox>().CurrentValue;
                var drawW = DrawingsMenu["drawing.w"].Cast<CheckBox>().CurrentValue;
                var drawE = DrawingsMenu["drawing.e"].Cast<CheckBox>().CurrentValue;
                var drawR = DrawingsMenu["drawing.r"].Cast<CheckBox>().CurrentValue;              

                if (drawQ && Q.Level > 0 && Q.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = Q.Range }.Draw(Player.Instance.Position);
                }
                if (drawW && W.Level > 0 && W.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = W.Range }.Draw(Player.Instance.Position);
                }
                if (drawE && E.Level > 0 && E.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = E.Range }.Draw(Player.Instance.Position);
                }
                if (drawR && R.Level > 0 && R.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.BlueViolet, BorderWidth = 1, Radius = R.Range }.Draw(Player.Instance.Position);
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
            if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Combo))
            {
                var ultType = ComboMenu["combo.mod"].Cast<Slider>().CurrentValue;

                
                if (ultType == 0)
                {
                    StateHandler.Combo();
                }
                else if (ultType == 1)
                {
                    StateHandler.MisayaCombo();
                }
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass))
            {
                StateHandler.Harass();
            }
            else if (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.LaneClear))
            {
                StateHandler.WaveClear();
            }
           }
    }
}
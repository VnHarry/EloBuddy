using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Constants;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;
using VnHarry_AIO.Internal;

namespace VnHarry_AIO.Marksman
{
    internal class Sivir : PluginBase
    {
        private static Spell.Skillshot _Q;
        private static Spell.Active _W;
        private static Spell.Active _E;
        private static Spell.Active _R;

        public Sivir()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
        }

        #region Setup

        public override sealed void _SetupSpells()
        {
            _Q = new Spell.Skillshot(SpellSlot.Q, 1245, SkillShotType.Linear, (int)0.25f, 1030, 90);
            _W = new Spell.Active(SpellSlot.W);
            _E = new Spell.Active(SpellSlot.E);
            _R = new Spell.Active(SpellSlot.R, 1000);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo"));
            Variables.Config.Add("commbo.w", new CheckBox("Use E in Combo"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo"));
            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.q", new CheckBox("Use Q in Harass", false));
            Variables.Config.Add("harass.w", new CheckBox("Use W in Harass", false));
            Variables.Config.Add("harass.mana", new Slider("Mana manager (%)", 50, 0, 100));
            Variables.Config.AddGroupLabel("LaneClear");
            //Variables.Config.Add("harass.q", new CheckBox("Use Q in LaneClear", false));
            //Variables.Config.Add("harass.w", new CheckBox("Use W in LaneClear", false));
            //Variables.Config.Add("laneclear.mana", new Slider("Mana manager (%)", 50, 0, 100));
            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.autoq", new CheckBox("Auto Q"));
            Variables.Config.Add("misc.autoe", new CheckBox("Auto E"));
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use E upon Gapcloser"));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.r", new CheckBox("Draw R"));
        }

        #endregion Setup

        public static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (Variables.Config["misc.autoe"].Cast<CheckBox>().CurrentValue && !Program._Player.IsDead)
            {
                if (!sender.IsMinion &&
                    sender.IsEnemy && args.Target.IsMe && !args.SData.IsAutoAttack() && _E.IsReady())
                    _E.Cast();
            }
        }

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue && _E.IsReady() &&
                 sender.IsValidTarget(5000))
            {
                _E.Cast();
            }
        }

        public override void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            if (Variables.Config["misc.autoq"].Cast<CheckBox>().CurrentValue)
            {
                foreach (
                    var target in
                        HeroManager.Enemies.Where(
                            x => x.IsValidTarget(_Q.Range) && !x.HasBuffOfType(BuffType.Invulnerability)))
                {
                    if (_Q.GetPrediction(target).HitChance >= HitChance.Immobile)
                        _Q.Cast(target);
                }
            }

            if (Variables.ComboMode)
            {
                Combo();
            }
            if (Variables.HarassMode)
            {
                Harass();
            }
            if (Variables.LaneClearMode)
            {
                Clear();
                Jungle();
            }
        }

        private void Jungle()
        {
            //code here
        }

        private void Clear()
        {
           
        }

        private void Harass()
        {
            var target = TargetSelector.GetTarget(_Q.Range, DamageType.Physical);
            if (target == null) return;
            if (Variables.Config["harass.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady() &&
                Variables.Config["harass.mana"].Cast<Slider>().CurrentValue <= Program._Player.ManaPercent)
            {
                _Q.Cast(target);
            }
            if (Variables.Config["harass.w"].Cast<CheckBox>().CurrentValue && _W.IsReady() &&
                 Variables.Config["harass.mana"].Cast<Slider>().CurrentValue <= Program._Player.ManaPercent)
            {
                _W.Cast(target);
            }
        }

        private void Combo()
        {
            try
            {
                var useQ = Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue;
                var useW = Variables.Config["commbo.w"].Cast<CheckBox>().CurrentValue;
                var useR = Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue;

                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1200) && !o.IsDead && !o.IsZombie))
                {
                    if (useQ && _Q.IsReady())
                    {
                        _Q.Cast(target);
                    }
                    if (useW && _W.IsReady())
                    {
                        _W.Cast();
                    }
                    if (useR && _R.IsReady())
                    {
                        if (Program._Player.CountEnemiesInRange(800) > 2)
                        {
                            _R.Cast();
                        }
                        else if (target.IsValidTarget() && Program._Player.GetAutoAttackDamage(target) * 2 > target.Health && !_Q.IsReady() &&
                                 target.CountEnemiesInRange(800) < 3)
                            _R.Cast();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program._Player.IsDead)
            {
                if (Variables.Config["draw.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
                }

                if (Variables.Config["draw.r"].Cast<CheckBox>().CurrentValue)
                {
                    foreach (var buff in Program._Player.Buffs)
                    {
                        if (buff.Name.ToLower() == "sivirr")
                        {
                            var mypos = Drawing.WorldToScreen(Program._Player.Position);
                            Drawing.DrawText(mypos[0] - 10, mypos[1] - 140, System.Drawing.Color.Gold, "" + (buff.EndTime - Game.Time));
                            break;
                        }
                    }
                }
            }
        }
    }
}
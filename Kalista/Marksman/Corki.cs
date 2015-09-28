using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Utilities;

namespace VnHarry_AIO.Marksman
{
    internal class Corki : PluginBase
    {
        private readonly int[] _RDamage = { 100, 180, 260 };
        private readonly float[] _RDamageScale = { 0.2f, 0.3f, 0.4f };
        private static Spell.Active _E;
        private static Spell.Skillshot _Q;
        private static Spell.Skillshot _R;
        private static Spell.Skillshot _W;

        public Corki()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        #region Setup

        public override sealed void _SetupSpells()
        {
            _E = new Spell.Active(SpellSlot.E, 600);
            _Q = new Spell.Skillshot(SpellSlot.Q, 825, SkillShotType.Circular, 300, 1000, 250);
            _R = new Spell.Skillshot(SpellSlot.R, 1300, SkillShotType.Linear, 200, 2000, 40);
            _W = new Spell.Skillshot(SpellSlot.W, 800, SkillShotType.Linear);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo"));
            Variables.Config.Add("commbo.e", new CheckBox("Use E in Combo"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo"));
            Variables.Config.Add("commbo.userstacks", new Slider("Save x Rockets", 1, 0, 7));
            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.q", new CheckBox("Use Q in Harass", false));
            Variables.Config.Add("harass.r", new CheckBox("Use R in Harass"));
            Variables.Config.Add("harass.userstacks", new Slider("Save x Rockets", 4, 0, 7));
            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.autor", new CheckBox("Auto R"));
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use W upon Gapcloser", false));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.r", new CheckBox("Draw R"));
        }

        #endregion Setup

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (sender.Distance(ObjectManager.Player) < 1400
                && (e.End.Distance(ObjectManager.Player) < sender.Distance(ObjectManager.Player)))
            {
                _W.Cast((ObjectManager.Player.Position.Extend(e.End, -1 * _W.Range)).To3D());
            }
        }

        public override void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

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

            foreach (var hero in
                HeroManager.Enemies
                    .Where(x => x.Position.Distance(ObjectManager.Player) < _R.Range))
            {
                if (!hero.IsDead && !hero.IsZombie && _RCanKill(hero) &&
                    Variables.Config["misc.autor"].Cast<CheckBox>().CurrentValue)
                {
                    _R.Cast(hero);
                }
            }
        }

        private bool _RCanKill(Obj_AI_Base target)
        {
            var RDamage = (_RDamage[_R.Level] +
                           ObjectManager.Player.TotalAttackDamage * _RDamageScale[_R.Level]
                           + ObjectManager.Player.TotalMagicalDamage * 0.3f) - 20.0f; //Damage Calc is off

            return ObjectManager.Player.CalculateDamageOnUnit(target, DamageType.Magical, RDamage) > target.Health;
        }

        private void Jungle()
        {
            //code here
        }

        private void Clear()
        {
            //code here
        }

        private void Harass()
        {
            var QTarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Magical);
            var RTarget = TargetSelector2.GetTarget(_R.Range, DamageType.Magical);
            if (QTarget.IsValidTarget() && Variables.Config["harass.q"].Cast<CheckBox>().CurrentValue)
            {
                _Q.Cast(QTarget);
            }

            if (RTarget.IsValidTarget() && Variables.Config["harass.r"].Cast<CheckBox>().CurrentValue
                && Variables.Config["harass.userstacks"].Cast<Slider>().CurrentValue < _R.Handle.Ammo)
            {
                _R.Cast(RTarget);
            }
        }

        private void Combo()
        {
            try
            {
                var QTarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Magical);
                var RTarget = TargetSelector2.GetTarget(_R.Range, DamageType.Magical);
                if (QTarget.IsValidTarget() && Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue)
                {
                    _Q.Cast(QTarget);
                }
                var inERange = false;
                foreach (var target in HeroManager.Enemies)
                {
                    if (target.Distance(Player.Instance) <= 550)
                        inERange = true;
                }
                if (inERange && Variables.Config["commbo.e"].Cast<CheckBox>().CurrentValue)
                {
                    _E.Cast();
                }
                if (RTarget.IsValidTarget() && Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue
                    && Variables.Config["commbo.userstacks"].Cast<Slider>().CurrentValue < _R.Handle.Ammo)
                {
                    _R.Cast(RTarget);
                }
            }
            catch
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

                if (Variables.Config["draw.r"].Cast<CheckBox>().CurrentValue && _R.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
                }
            }
        }
    }
}
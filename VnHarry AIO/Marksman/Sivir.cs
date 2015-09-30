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
using VnHarry_AIO.Utilities;

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
            Variables.Config.AddGroupLabel(MessageText.txtCombo);
            Variables.Config.Add(MessageText.ucomboQ, new CheckBox(MessageText.txtcomboQ));
            Variables.Config.Add(MessageText.ucomboW, new CheckBox(MessageText.txtcomboW));
            Variables.Config.Add(MessageText.ucomboR, new CheckBox(MessageText.txtcomboR));
            Variables.Config.AddGroupLabel(MessageText.txtHarass);
            Variables.Config.Add(MessageText.uharassQ, new CheckBox(MessageText.txtharassQ));
            Variables.Config.Add(MessageText.uharassW, new CheckBox(MessageText.txtharassW));
            Variables.Config.Add(MessageText.uharassMana, new Slider(MessageText.txtharassMana, 50, 0, 100));
            Variables.Config.AddGroupLabel(MessageText.txtMisc);
            Variables.Config.Add("misc.autoq", new CheckBox("Auto Q"));
            Variables.Config.Add("misc.autoe", new CheckBox("Auto E"));
            Variables.Config.Add("misc.antigapcloser", new CheckBox("User E in Gapcloser"));
            Variables.Config.AddGroupLabel(MessageText.txtDraw);
            Variables.Config.Add(MessageText.udrawQ, new CheckBox(MessageText.txtdrawQ));
            Variables.Config.Add(MessageText.udrawR, new CheckBox(MessageText.txtdrawR));
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
            var target = TargetSelector2.GetTarget(_Q.Range, DamageType.Physical);
            if (target == null) return;
            if (Variables.GetCheckBoxConfig(MessageText.uharassQ) && _Q.IsReady() &&
                Variables.GetSliderConfig(MessageText.uharassMana) <= Program._Player.ManaPercent)
            {
                _Q.Cast(target);
            }
            if (Variables.GetCheckBoxConfig(MessageText.uharassW) && _Q.IsReady() &&
                Variables.GetSliderConfig(MessageText.uharassMana) <= Program._Player.ManaPercent)
            {
                _W.Cast(target);
            }
        }

        private void Combo()
        {
            try
            {
                var useQ = Variables.GetCheckBoxConfig(MessageText.ucomboQ);
                var useW = Variables.GetCheckBoxConfig(MessageText.ucomboW);
                var useR = Variables.GetCheckBoxConfig(MessageText.ucomboR);

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
                if (Variables.GetCheckBoxConfig(MessageText.udrawQ) && _Q.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
                }

                if (Variables.GetCheckBoxConfig(MessageText.udrawR))
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
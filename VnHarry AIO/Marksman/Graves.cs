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
    internal class Graves : PluginBase
    {
        private static Spell.Skillshot _Q;
        private static Spell.Skillshot _W;
        private static Spell.Skillshot _E;
        private static Spell.Skillshot _R;

        public Graves()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            DamageIndicator.Initialize(GetComboDamage);
        }

        #region Setup

        public override sealed void _SetupSpells()
        {
            _Q = new Spell.Skillshot(SpellSlot.Q, (int)720f, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)(15f * (float)Math.PI / 180));
            _W = new Spell.Skillshot(SpellSlot.W, (int)850f, SkillShotType.Circular, (int)0.25f, (int)1650f, (int)250f);
            _E = new Spell.Skillshot(SpellSlot.E, (uint)Program._Player.GetAutoAttackRange(), SkillShotType.Circular);
            _R = new Spell.Skillshot(SpellSlot.R, (int)1100f, SkillShotType.Linear, (int)0.25f, (int)2100f, (int)100f);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel(MessageText.txtCombo);
            Variables.Config.Add(MessageText.ucomboQ, new CheckBox(MessageText.txtcomboQ));
            Variables.Config.Add(MessageText.ucomboE, new CheckBox(MessageText.txtcomboE));
            Variables.Config.Add(MessageText.ucomboW, new CheckBox(MessageText.txtcomboW));
            Variables.Config.Add(MessageText.ucomboR, new CheckBox(MessageText.txtcomboR));
            Variables.Config.AddGroupLabel(MessageText.txtHarass);
            Variables.Config.Add(MessageText.uharassQ, new CheckBox(MessageText.txtharassQ));
            Variables.Config.AddGroupLabel(MessageText.txtLaneClear);
            Variables.Config.Add(MessageText.ulaneclearMana, new Slider(MessageText.txtlaneclearMana));
            Variables.Config.AddGroupLabel(MessageText.txtMisc);
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use E in Gapcloser", true));
            Variables.Config.AddGroupLabel(MessageText.txtDraw);
            Variables.Config.Add(MessageText.udrawQ, new CheckBox(MessageText.txtdrawQ));
            Variables.Config.Add(MessageText.udrawW, new CheckBox(MessageText.txtdrawW));
            Variables.Config.Add(MessageText.udrawR, new CheckBox(MessageText.txtdrawR));
        }

        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            var damage = 0f;

            if (_Q.IsReady())
                damage += QDamage(enemy);

            if (_W.IsReady())
                damage += WDamage(enemy);

            if (_R.IsReady())
                damage += RDamage(enemy);

            return (float)damage;
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 60, 90, 120, 150, 180 }[_Q.Level - 1] + 0.75 * Program._Player.FlatPhysicalDamageMod));
        }

        public static float WDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float)(new[] { 60, 110, 160, 210, 260 }[_W.Level - 1] + 0.60 * Program._Player.FlatMagicDamageMod));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 200, 400, 550 }[_R.Level - 1] + 1.5 * Program._Player.FlatPhysicalDamageMod));
        }

        #endregion Setup

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }

            if (sender.Distance(ObjectManager.Player) < (_E.Range + 300)
                && (e.End.Distance(ObjectManager.Player) < sender.Distance(ObjectManager.Player)))
            {
                _E.Cast((ObjectManager.Player.Position.Extend(e.End, -1 * _W.Range)).To3D());
            }
        }

        public override void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;
            if (_Q.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(_Q.Range) && !o.IsDead && !o.IsZombie))
                {
                    var pred = _Q.GetPrediction(enemy);
                    if ((pred.HitChance == HitChance.Immobile) ||
                        (pred.HitChance == HitChance.Dashing))
                    {
                        _Q.Cast(pred.CastPosition);
                    }
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
            var allMinionsQ = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), _Q.Range + _Q.Width + 30);
            if (allMinionsQ == null) return;

            if (Program._Player.ManaPercent >= Variables.GetSliderConfig(MessageText.ulaneclearMana))
            {
                foreach (Obj_AI_Minion minion in allMinionsQ)
                {
                    if ((!Program._Player.IsInAutoAttackRange(minion) || (!Orbwalker.CanAutoAttack && Orbwalker.LastTarget.NetworkId != minion.NetworkId)) && (minion.Health < 0.8 * QDamage(minion)))
                    {
                        _Q.Cast(minion);
                        return;
                    }
                }
            }
            else
            {
            }
        }

        private void Harass()
        {
            var target = TargetSelector2.GetTarget(_Q.Range + 100, DamageType.Magical);
            if (target == null) return;
            _Q.Cast(target);
        }

        private void Combo()
        {
            try
            {
                var useQ = Variables.GetCheckBoxConfig(MessageText.ucomboQ);
                var useW = Variables.GetCheckBoxConfig(MessageText.ucomboW);
                var useE = Variables.GetCheckBoxConfig(MessageText.ucomboE);
                var useR = Variables.GetCheckBoxConfig(MessageText.ucomboR);

                foreach (var target in HeroManager.Enemies.Where(o => o.IsValidTarget(1200) && !o.IsDead && !o.IsZombie))
                {
                    var comboDamage = target != null ? GetComboDamage(target) : 0;
                    if (useQ && _Q.IsReady() && _Q.GetPrediction(target).HitChance >= HitChance.Medium)
                    {
                        _Q.Cast(target);
                    }
                    if (useW && _W.IsReady() && _W.GetPrediction(target).HitChance >= HitChance.Medium && target.IsValidTarget(_W.Range))
                    {
                        _W.Cast(target);
                    }
                    if (useE && _E.IsReady() && target.IsValidTarget(700))
                    {
                        _E.Cast(Game.CursorPos);
                    }
                    if (useR && _R.IsReady() && _R.GetPrediction(target).HitChance >= HitChance.High && target.Health <= RDamage(target) && target.IsValidTarget(_R.Range))
                    {
                        _R.Cast(target);
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
                if (Variables.GetCheckBoxConfig(MessageText.udrawQ) && _Q.IsReady() && _Q.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
                }

                if (Variables.GetCheckBoxConfig(MessageText.udrawW) && _W.IsReady() && _W.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);
                }
                if (Variables.GetCheckBoxConfig(MessageText.udrawR) && _R.IsReady() && _R.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
                }
            }
        }
    }
}
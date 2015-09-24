using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;
using VnHarry_AIO.Internal;

namespace VnHarry_AIO.Marksman
{
    internal class Vayne : PluginBase
    {
        private static Spell.Ranged _Q;
        private static Spell.Targeted _E;
        private static Spell.Active _R;

        public Vayne()
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
            _Q = new Spell.Skillshot(SpellSlot.Q, 325, SkillShotType.Linear);
            _E = new Spell.Targeted(SpellSlot.E, (uint)Program._Player.Spellbook.GetSpell(SpellSlot.E).SData.CastRange);
            _R = new Spell.Active(SpellSlot.R);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo"));
            Variables.Config.Add("commbo.e", new CheckBox("Use E in Combo"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo"));
            Variables.Config.Add("commbo.rsminenemiesforr", new Slider("Min Enemies for cast R: ", 2, 1, 5));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.e", new CheckBox("Draw E"));
            Variables.Config.Add("draw.condemnpos", new CheckBox("Draw Condemn Position", true));
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
                _E.Cast((ObjectManager.Player.Position.Extend(e.End, -1 * _Q.Range)).To3D());
            }
        }

        public static void OnAfterAttack(AttackableUnit target, EventArgs args)
        {
            if (target != null && (!target.IsValid || target.IsDead))
                return;

            var orbwalkermode = Orbwalker.ActiveModesFlags;

            if (orbwalkermode == Orbwalker.ActiveModes.Combo)
            {
                if (Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady())
                {
                    if (target != null) _Q.Cast(target.Position);
                }
            }
        }

        public static void OnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (target != null && (!target.IsValid || target.IsDead))
                return;

            var orbwalkermode = Orbwalker.ActiveModesFlags;

            if (orbwalkermode == Orbwalker.ActiveModes.Combo)
            {
                if (Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady())
                {
                    if (target != null) _Q.Cast(target.Position);
                }
            }
        }

        public static void OnProcessSpell(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe) return;
            if (args.SData.Name.ToLower().Contains("vaynetumble"))
            {
                Core.DelayAction(Orbwalker.ResetAutoAttack, 250);
            }
        }

        public static bool IsCondenavel(AIHeroClient target)
        {
            if (target.HasBuffOfType(BuffType.SpellImmunity) || target.HasBuffOfType(BuffType.SpellShield) || Program._Player.IsDashing()) return false;

            var posicao = Program._Player.Position.Extend(target.Position, Program._Player.Distance(target) - 20).To3D();
            for (int i = 0; i < 470 - 20; i += 10)
            {
                var cPos = Program._Player.Position.Extend(posicao, Program._Player.Distance(posicao) + i).To3D();
                if (cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) || cPos.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building))
                {
                    return true;
                }
            }
            return false;
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
        }

        private void Jungle()
        {
            //code here
        }

        private void Clear()
        {
            //
        }

        private void Harass()
        {
        }

        private void Combo()
        {
            try
            {
                var _target = TargetSelector.GetTarget(1100, DamageType.Physical);

                if (_target == null || !_target.IsValid)
                    return;

                if (Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue && _R.IsReady())
                {
                    if (Program._Player.CountEnemiesInRange(Program._Player.GetAutoAttackRange()) >= Variables.Config["commbo.rsminenemiesforr"].Cast<Slider>().CurrentValue)
                    {
                        _R.Cast();
                    }
                }

                if (Variables.Config["commbo.e"].Cast<CheckBox>().CurrentValue && _E.IsReady())
                {
                    AIHeroClient priorityTarget = null;
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy).Where(a => !a.IsDead).Where(a => _E.IsInRange(a)))
                    {
                        if (priorityTarget == null)
                        {
                            priorityTarget = enemy;
                        }
                        if (!IsCondenavel(priorityTarget))
                            return;
                    }

                    if (priorityTarget != null && priorityTarget.IsValid && IsCondenavel(priorityTarget))
                    {
                        _E.Cast(priorityTarget);
                    }
                }
                if (Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady())
                {
                    if (Program._Player.Distance(_target.Position) > Program._Player.GetAutoAttackRange() && Program._Player.Distance(_target.Position) < (Program._Player.GetAutoAttackRange() + _Q.Range + 50))
                    {
                        _Q.Cast(_target.Position);
                    }
                    else { _Q.Cast(Game.CursorPos); }
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

                if (Variables.Config["draw.e"].Cast<CheckBox>().CurrentValue && _E.IsReady())
                {
                    foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy).Where(a => !a.IsDead).Where(a => _E.IsInRange(a)))
                    {
                        var condemnPos = Program._Player.Position.Extend(enemy.Position, Program._Player.Distance(enemy) + 470 - 20);

                        var realStart = Drawing.WorldToScreen(enemy.Position);
                        var realEnd = Drawing.WorldToScreen(condemnPos.To3D());

                        Drawing.DrawLine(realStart, realEnd, 2f, System.Drawing.Color.Red);
                        new Circle() { Color = System.Drawing.Color.Red, Radius = 60, BorderWidth = 2f }.Draw(condemnPos.To3D());
                    }
                }
            }
        }
    }
}
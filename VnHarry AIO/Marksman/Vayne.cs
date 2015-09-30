using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System;
using System.Linq;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Utilities;

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
            _E = new Spell.Targeted(SpellSlot.E, 590);
            _R = new Spell.Active(SpellSlot.R);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel(MessageText.txtCombo);
            Variables.Config.Add(MessageText.ucomboQ, new CheckBox(MessageText.txtcomboQ));
            Variables.Config.Add(MessageText.ucomboE, new CheckBox(MessageText.txtcomboE));
            Variables.Config.Add(MessageText.ucomboR, new CheckBox(MessageText.txtcomboR));
            Variables.Config.Add("commbo.rsminenemiesforr", new Slider("Minimum heros use R: ", 2, 1, 5));
            Variables.Config.AddGroupLabel(MessageText.txtHarass);
            Variables.Config.Add(MessageText.uharassQ, new CheckBox(MessageText.txtharassQ, false));
            Variables.Config.Add(MessageText.uharassMana, new CheckBox(MessageText.txtharassMana));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add(MessageText.udrawQ, new CheckBox(MessageText.txtdrawQ));
            Variables.Config.Add(MessageText.udrawE, new CheckBox(MessageText.txtdrawE));
            Variables.Config.Add("draw.condemnpos", new CheckBox("Draw Condemnpos E", true));
        }

        #endregion Setup

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            //if (!Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue)
            //{
            //    return;
            //}

            if (e.Sender.IsValidTarget(1000))
            {
                new Circle() { Color = System.Drawing.Color.Gold, Radius = e.Sender.BoundingRadius, BorderWidth = 5 }.Draw(e.Sender.Position);
                var targetpos = Drawing.WorldToScreen(e.Sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, System.Drawing.Color.Gold, "Gapcloser");
            }
            if (_E.IsInRange(e.Sender) && _E.IsReady())
            {
                _E.Cast(e.Sender);
            }
        }

        public static void OnAfterAttack(AttackableUnit target, EventArgs args)
        {
            if (target != null && (!target.IsValid || target.IsDead))
                return;
                  
            if (Variables.ComboMode)
            {
                if (Variables.GetCheckBoxConfig(MessageText.ucomboQ) && _Q.IsReady())
                {
                    if (target != null) _Q.Cast(target.Position);
                }
            }
        }

        public static void OnBeforeAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (target != null && (!target.IsValid || target.IsDead))
                return;

            if (Variables.ComboMode)
            {
                if (Variables.GetCheckBoxConfig(MessageText.ucomboQ) && _Q.IsReady())
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
            if (ObjectManager.Player.ManaPercent > Variables.GetSliderConfig(MessageText.uharassMana))
            {
                if (_Q.IsReady() && Variables.GetCheckBoxConfig(MessageText.uharassQ))
                {
                    foreach (var qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(_Q.Range)))
                    {
                        if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                        {
                            _Q.Cast(qTarget);
                        }
                    }
                }
            }
        }

        private void Harass()
        {
            if (ObjectManager.Player.ManaPercent > Variables.GetSliderConfig(MessageText.uharassMana))
            {
                if (_Q.IsReady() && Variables.GetCheckBoxConfig(MessageText.uharassQ))
                {
                    foreach (var qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(_Q.Range)))
                    {
                        if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                        {
                            _Q.Cast(qTarget);
                        }
                    }
                }
            }
        }

        private void Combo()
        {
            try
            {
                var _target = TargetSelector2.GetTarget(Program._Player.GetAutoAttackRange(), DamageType.Physical);

                if (_target == null || !_target.IsValid)
                    return;

                if (Variables.GetCheckBoxConfig(MessageText.ucomboR) && _R.IsReady())
                {
                    if (Program._Player.CountEnemiesInRange(Program._Player.GetAutoAttackRange()) >= Variables.GetSliderConfig("commbo.rsminenemiesforr"))
                    {
                        _R.Cast();
                    }
                }

                if (Variables.GetCheckBoxConfig(MessageText.ucomboE) && _E.IsReady())
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

                if (Variables.GetCheckBoxConfig(MessageText.ucomboQ) && _Q.IsReady())
                {
                    foreach (var qTarget in HeroManager.Enemies.Where(x => x.IsValidTarget(Program._Player.GetAutoAttackRange() + _Q.Range)))
                    {
                        if (qTarget.Buffs.Any(buff => buff.Name == "vaynesilvereddebuff" && buff.Count == 2))
                        {
                            _Q.Cast(qTarget.Position);
                        }
                    }

                    if (Program._Player.Distance(_target.Position) > Program._Player.GetAutoAttackRange() && Program._Player.Distance(_target.Position) < (Program._Player.GetAutoAttackRange() + _Q.Range + 50))
                    {
                        if (_Q.IsReady())
                        {
                            _Q.Cast(_target.Position);
                        }
                    }
                    _Q.Cast(Game.CursorPos);
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
                if (Variables.GetCheckBoxConfig(MessageText.udrawQ) && _Q.IsReady() && _Q.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
                }

                if (Variables.GetCheckBoxConfig(MessageText.udrawE) && _E.IsReady() && _E.Level >= 1)
                {
                    //foreach (var enemy in ObjectManager.Get<AIHeroClient>().Where(a => a.IsEnemy).Where(a => !a.IsDead).Where(a => _E.IsInRange(a)))
                    //{
                    //    var condemnPos = Program._Player.Position.Extend(enemy.Position, Program._Player.Distance(enemy) + 470 - 20);
                    //
                    //    var realStart = Drawing.WorldToScreen(enemy.Position);
                     //   var realEnd = Drawing.WorldToScreen(condemnPos.To3D());
                    //
                     //   Drawing.DrawLine(realStart, realEnd, 2f, System.Drawing.Color.Red);
                      //  new Circle() { Color = System.Drawing.Color.Red, Radius = 60, BorderWidth = 2f }.Draw(condemnPos.To3D());
                    //}//

                    foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(1100)).Where(a => !a.IsDead))
                    {
                        var realStart = Drawing.WorldToScreen(enemy.Position);
                        
                        int pushDist = 425;
                        for (int i = 0; i < pushDist; i += (int)enemy.BoundingRadius)
                        {
                            Vector3 loc3 = enemy.Position.To2D().Extend(Program._Player.Position.To2D(), -i).To3D();
                            if (loc3.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Wall) || loc3.ToNavMeshCell().CollFlags.HasFlag(CollisionFlags.Building))
                            {
                                var realEnd = Drawing.WorldToScreen(loc3);
                                Drawing.DrawLine(realStart, realEnd, 2f, System.Drawing.Color.Yellow);
                                new Circle() { Color = System.Drawing.Color.Yellow, Radius = enemy.BoundingRadius, BorderWidth = 2f }.Draw(loc3);
  
                            }
                        }
                    }
                }
            }
        }
    }
}
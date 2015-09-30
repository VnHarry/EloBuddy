using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;
using SharpDX;
using System.Linq;

namespace VnHarry_Diana
{
    internal class StateHandler
    {
        public static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        public static void Combo()
        {
            var target = TargetSelector2.GetTarget(Program.Q.Range, DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }
            var useQ = Program.ComboMenu["combo.q"].Cast<CheckBox>().CurrentValue;
            var useW = Program.ComboMenu["combo.w"].Cast<CheckBox>().CurrentValue;
            var useE = Program.ComboMenu["combo.e"].Cast<CheckBox>().CurrentValue;
            var useR = Program.ComboMenu["combo.r"].Cast<CheckBox>().CurrentValue;
            var secondR = Program.ComboMenu["combo.secure"].Cast<CheckBox>().CurrentValue;
            var UseSecondRLimitation = Program.ComboMenu["combo.UseSecondRLimitation"].Cast<Slider>().CurrentValue;
            var minHpToDive = Program.ComboMenu["combo.preventundertower"].Cast<Slider>().CurrentValue;

            if (useQ && Program.Q.IsReady() && Program.Q.IsInRange(target))
            {
                var pred = Program.Q.GetPrediction(target);
                if (pred.HitChance >= EloBuddy.SDK.Enumerations.HitChance.Medium)
                {
                    Program.Q.Cast(target);
                }
            }

            if (useR && Program.R.IsReady() && Program.R.IsInRange(target)
                && target.HasBuff("dianamoonlight")
                 && (!IsUnderTurret(target.ServerPosition) || (minHpToDive <= Program._Player.HealthPercent)))
            {
                Program.R.Cast(target);
            }

            if (useW && Program.W.IsReady() && Program.W.IsInRange(target))
            {
                Program.W.Cast();
            }

            if (useE && Program.E.IsReady() && Program.E.IsInRange(target))
            {
                Program.E.Cast();
            }

            if (secondR && (!IsUnderTurret(target.ServerPosition) || (minHpToDive <= Program._Player.HealthPercent)))
            {
                var closeEnemies = HeroManager.Enemies.Where<AIHeroClient>(a => a.Distance(Program._Player) < Program.R.Range * 2 && !a.IsInvulnerable && !a.IsZombie && !a.IsDead);
                int enemycount = 0;
                foreach (AIHeroClient enemy in closeEnemies)
                {
                    enemycount++;
                }
                if (enemycount <= UseSecondRLimitation && useR && !Program.Q.IsReady()
                && Program.R.IsReady())
                {
                    if (target.Health < Program.RDamage(target)
                        && (!IsUnderTurret(target.ServerPosition) || (minHpToDive <= Program._Player.HealthPercent)))
                    {
                        Program.R.Cast(target);
                    }
                }

                if (enemycount <= UseSecondRLimitation && Program.R.IsReady())
                {
                    if (target.Health < Program.RDamage(target))
                    {
                        Program.R.Cast(target);
                    }
                }
            }

            //Ignite
            if (Program.gotIgnite && target != null && Program.ignite != SpellSlot.Unknown &&
                Program._Player.Spellbook.CanUseSpell(Program.ignite) == SpellState.Ready & !target.IsInvulnerable & !target.IsZombie)
            {
                if (ObjectManager.Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health)
                {
                    Program._Player.Spellbook.CastSpell(Program.ignite, target);
                    Program.R.Cast(target);
                    return;
                }
            }
        }
        public static void MisayaCombo()
        {
            var target = TargetSelector.GetTarget(Program.Q.Range, DamageType.Magical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Program.ComboMenu["combo.q"].Cast<CheckBox>().CurrentValue;
            var useW = Program.ComboMenu["combo.w"].Cast<CheckBox>().CurrentValue;
            var useE = Program.ComboMenu["combo.e"].Cast<CheckBox>().CurrentValue;
            var useR = Program.ComboMenu["combo.r"].Cast<CheckBox>().CurrentValue;
            var secondR = Program.ComboMenu["combo.secure"].Cast<CheckBox>().CurrentValue;
            var UseSecondRLimitation = Program.ComboMenu["combo.UseSecondRLimitation"].Cast<Slider>().CurrentValue;
            var minHpToDive = Program.ComboMenu["combo.preventundertower"].Cast<Slider>().CurrentValue;
            var misayaMinRange = Program.ComboMenu["combo.misayaminrange"].Cast<Slider>().CurrentValue;
            var distToTarget = Program._Player.Distance(target, false);
           
            // Can use R, R is ready but player too far from the target => do nothing
            if (useR && Program.R.IsReady() && distToTarget > Program.R.Range)
            {
                return;
            }

            // Prerequisites for Misaya Combo : If target is too close, won't work
            if (useQ && useR && Program.Q.IsReady() && Program.R.IsReady()
                && distToTarget >= misayaMinRange)
            {
                Program.R.Cast(target);
                // No need to check the hitchance since R is a targeted dash.
                Program.Q.Cast(target);
            }

            // Misaya Combo is not possible, classic mode then

            if (useQ && Program.Q.IsReady() && Program.Q.IsInRange(target))
            {
              
                    Program.Q.Cast(target);
            }

            if (useR && Program.R.IsReady() && Program.R.IsInRange(target)
                && target.HasBuff("dianamoonlight"))
            {
                Program.R.Cast(target);
            }

            if (useW && Program.W.IsReady() && Program.W.IsInRange(target))
            {
                Program.W.Cast();
            }

            if (useE && Program.E.IsReady() && Program.E.IsInRange(target))
            {

                Program.E.Cast();

            }

            if (secondR)
            {
                var closeEnemies = HeroManager.Enemies.Where<AIHeroClient>(a => a.Distance(Program._Player) < Program.R.Range * 2 && !a.IsInvulnerable && !a.IsZombie && !a.IsDead);
                int enemycount = 0;
                foreach (AIHeroClient enemy in closeEnemies)
                {
                    enemycount++;
                }
                if (enemycount <= UseSecondRLimitation && useR && !Program.Q.IsReady()
                    && Program.R.IsReady())
                {
                    if (target.Health < Program.RDamage(target))
                    {
                        Program.R.Cast(target);
                    }
                }

                if (enemycount <= UseSecondRLimitation && Program.R.IsReady())
                {
                    if (target.Health < Program.RDamage(target))
                    {
                        Program.R.Cast(target);
                    }
                }
            }

            //Ignite
            if (Program.gotIgnite && target != null && Program.ignite != SpellSlot.Unknown &&
                Program._Player.Spellbook.CanUseSpell(Program.ignite) == SpellState.Ready & !target.IsInvulnerable & !target.IsZombie)
            {
                if (ObjectManager.Player.GetSummonerSpellDamage(target, DamageLibrary.SummonerSpells.Ignite) > target.Health)
                {
                    Program._Player.Spellbook.CastSpell(Program.ignite, target);
                    Program.R.Cast(target);
                    return;
                }
            }
        }

        public static void Harass()
        {
            var target = TargetSelector2.GetTarget(Program.Q.Range, DamageType.Physical);
            if (target == null || !target.IsValid)
            {
                return;
            }

            var useQ = Program.HarassMenu["harass.q"].Cast<CheckBox>().CurrentValue;
            var useW = Program.HarassMenu["harass.w"].Cast<CheckBox>().CurrentValue;
            var useE = Program.HarassMenu["harass.e"].Cast<CheckBox>().CurrentValue;
            var checkMana = Program.ComboMenu["harass.mana"].Cast<Slider>().CurrentValue;

            if (Program._Player.ManaPercent < checkMana)
            {
                return;
            }

            if (useQ && Program.Q.IsReady() && Program.Q.IsInRange(target))
            {
                var pred = Program.Q.GetPrediction(target);
                Program.Q.Cast(target);
            }

            if (useW && Program.W.IsReady() && Program.W.IsInRange(target))
            {
                Program.W.Cast();
            }

            if (useE && Program.E.IsReady() && Program.E.IsInRange(target))
            {
                Program.E.Cast();
            }
        }

       

        public static void WaveClear()
        {
            var minion = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program.Q.Range).FirstOrDefault();
            if (minion == null)
            {
                return;
            }

            var useQ = Program.LaneClearMenu["laneclear.q"].Cast<CheckBox>().CurrentValue;
            var useW = Program.LaneClearMenu["laneclear.w"].Cast<CheckBox>().CurrentValue;
            var useE = Program.LaneClearMenu["laneclear.e"].Cast<CheckBox>().CurrentValue;
            var useR = Program.LaneClearMenu["laneclear.r"].Cast<CheckBox>().CurrentValue;

            var countQ = Program.LaneClearMenu["laneclear.qcount"].Cast<Slider>().CurrentValue;
            var countW = Program.LaneClearMenu["laneclear.wcount"].Cast<Slider>().CurrentValue;
            var countE = Program.LaneClearMenu["laneclear.ecount"].Cast<Slider>().CurrentValue;

            var minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program.Q.Range);

            var qMinions = minions.FindAll(minionQ => minion.IsValidTarget(Program.Q.Range));
            var qMinion = qMinions.Find(minionQ => minionQ.IsValidTarget());

            if (useQ && Program.Q.IsReady())
            {
                var mline = Program.Q.GetPrediction(qMinion);
                if (mline.GetCollisionObjects<Obj_AI_Minion>().Count() >= countQ)
                {
                    Program.Q.Cast(qMinion);
                }
               
            }

            if (useW && Program.W.IsReady())
            //  && spells[Spells.W].GetCircularFarmLocation(minions).MinionsHit >= countW)
            {

                Program.W.Cast();
            }

            if (useE && Program.E.IsReady() && Program._Player.Distance(qMinion, false) < 200)
            //  && spells[Spells.E].GetCircularFarmLocation(minions).MinionsHit >= countE)
            {
                Program.E.Cast();
            }

            var minionsR = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program.Q.Range);

            if (useR && Program.R.IsReady())
            {
                foreach (var canBeKilled in minionsR)
                {
                    if (canBeKilled.HasBuff("dianamoonlight"))
                    {
                        if (canBeKilled.Health < Program.RDamage(minion))
                        {
                            if (canBeKilled.IsValidTarget())
                            {
                                Program.R.Cast(canBeKilled);
                            }
                        }
                    }
                }
            }
        }
    }
}
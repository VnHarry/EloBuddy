using EloBuddy;
using EloBuddy.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VnHarry_AIO.Internal;

namespace VnHarry_AIO.Utilities
{
    public class VnHarryWalker
    {
        public VnHarryWalker()
        {

        }
        public static float GetAutoAttackRange(Obj_AI_Base source = null, AttackableUnit target = null)
        {
            if (source == null)
                source = Program._Player;
            var ret = source.AttackRange + Program._Player.BoundingRadius;
            if (target != null)
                ret += target.BoundingRadius;
            return ret;
        }

        public static bool InAutoAttackRange(AttackableUnit target)
        {
            if (target == null)
                return false;
            var myRange = GetAutoAttackRange(Program._Player, target);
            return target.IsValidTarget(myRange);
        }

        private static int ProjectTime(Obj_AI_Base target)
        {
            return (int)(Program._Player.AttackCastDelay * 1000) - 100 + Game.Ping / 2 +
                   1000 * (int)Program._Player.Distance(target.Position) / (int)MyProjectileSpeed;
        }

        private static float MyProjectileSpeed
        {
            get
            {
                return Program._Player.CombatType == GameObjectCombatType.Melee ? float.MaxValue : Program._Player.BasicAttack.MissileSpeed;
            }
        }
        public static int _lastAaTick;
        public static bool _attack = true;
        public static bool _movement = true;
        public static int _lastRealAttack;
        public static bool CanAttack
        {
            get
            {
                return _lastAaTick <= Environment.TickCount &&
                       (Environment.TickCount + Game.Ping / 2 + 25 >= _lastAaTick + Program._Player.AttackDelay * 1000 && _attack);
            }
        }
        public static bool HaveCancled
        {
            get
            {
                return _lastAaTick - Environment.TickCount > Program._Player.AttackCastDelay * 1000 + 25 && _lastRealAttack < _lastAaTick;
            }
        }
        public static bool IsAllowedToAttack()
        {
            if (!_attack)
                return false;
        
            if (Variables.ComboMode)
                return false;
            if (Variables.HarassMode)
                return false;
            if (Variables.LaneClearMode)
                return false;
            return !Variables.LastHitMode;

        }
        public static void SetAttack(bool value)
        {
            _attack = value;
        }

        public static void SetMovement(bool value)
        {
            _movement = value;
        }
        public static double CountKillhits(Obj_AI_Base enemy)
        {
            return enemy.Health / Program._Player.GetAutoAttackDamage(enemy);
        }

        private static AIHeroClient GetBestHeroTarget()
        {
            AIHeroClient killableEnemy = null;
            var hitsToKill = double.MaxValue;
            foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget() && InAutoAttackRange(hero)))
            {
                var killHits = CountKillhits(enemy);
                if (killableEnemy != null && (!(killHits < hitsToKill) || enemy.HasBuffOfType(BuffType.Invulnerability)))
                    continue;
                hitsToKill = killHits;
                killableEnemy = enemy;
            }
            return hitsToKill <= 3 ? killableEnemy : TargetSelector2.GetTarget(GetAutoAttackRange(), DamageType.Physical);
        }

        private static AttackableUnit GetBestMinion(bool lastHitOnly)
        {
            AttackableUnit tempTarget = null;
            var enemies = ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget() && x.Name != "Beacon" && InAutoAttackRange(x)).ToList();

            foreach (var minion in from minion in enemies
                                   let t = ProjectTime(minion)
                                   let predHealth = minion.Health
                                   where minion.Team != GameObjectTeam.Neutral && predHealth > 0 &&
                                         (predHealth <= Program._Player.GetAutoAttackDamage(minion, true))
                                   select minion)
                return minion;

            if (true)
            {
                var turret =
                    ObjectManager.Get<Obj_AI_Turret>()
                        .Where(x => x.IsValid && x.IsAlly)
                        .OrderBy(x => Program._Player.Distance(x))
                        .FirstOrDefault();

                if (turret != null && lastHitOnly)
                {
                    foreach (var minion in enemies.Where(x => turret.Distance(x.ServerPosition) < 1000).OrderBy(x => x.Distance(turret)))
                    {
                        var playerProjectile = ProjectTime(minion);
                        var predHealth = minion.Health;
                        var turretProjectile = turret.AttackCastDelay * 1000 + turret.Distance(minion) / turret.BasicAttack.MissileSpeed * 1000;

                        if (predHealth < 0 || playerProjectile * 1.8 > turretProjectile)
                            continue;

                        if (predHealth - turret.GetAutoAttackDamage(minion) - Program._Player.GetAutoAttackDamage(minion, true) * 2 <=
                            0 &&
                            predHealth - turret.GetAutoAttackDamage(minion) - Program._Player.GetAutoAttackDamage(minion, true) > 0 &&
                            predHealth - turret.GetAutoAttackDamage(minion) * 2 < 0)
                        {
                            return minion;
                        }
                    }
                }
            }

            return tempTarget;
        }
        public static AttackableUnit GetTarget()
        {
            AttackableUnit tempTarget = null;

            if ((Variables.HarassMode || Variables.LaneClearMode))
            {
                tempTarget = TargetSelector2.GetTarget(-1, DamageType.Physical);
                if (tempTarget != null)
                    return tempTarget;
            }

            if (Variables.ComboMode)
            {
                tempTarget = GetBestHeroTarget();
            }
            if (Variables.HarassMode)
            {
                tempTarget = GetBestMinion(true);

                if (tempTarget != null)
                    return tempTarget;

                if (GetBestHeroTarget() != null)
                    tempTarget = GetBestHeroTarget();

                if (GetBaseStructures() != null)
                    return GetBaseStructures();
            }
            if (Variables.LastHitMode)
            {
                tempTarget = GetBestMinion(true);

                if (tempTarget != null)
                    return tempTarget;
            }
            if (Variables.LastHitMode)
            {
                tempTarget = GetBestMinion(true);

                if (tempTarget != null)
                    return tempTarget;
            }
               

            return tempTarget;
        }

        private static AttackableUnit GetBaseStructures()
        {
            //turrets 
            foreach (
                    var turret in
                        ObjectManager.Get<Obj_AI_Turret>().Where(turret => turret.IsValidTarget(GetAutoAttackRange(Program._Player, turret))))
                return turret;

            //inhib
            foreach (var barrack in
                ObjectManager.Get<Obj_BarracksDampener>().Where(t => t.IsValidTarget(GetAutoAttackRange(Program._Player, t))))
            {
                return barrack;
            }

            //nexus
            return ObjectManager.Get<Obj_HQ>().FirstOrDefault(t => t.IsValidTarget(GetAutoAttackRange(Program._Player, t)));
        }
        
    }
}

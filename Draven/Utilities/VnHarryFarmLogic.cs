﻿using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using System;
using System.Linq;
using VnHarry_AIO.Internal;

namespace VnHarry_AIO.Utilities
{
    public class VnHarryFarmLogic
    {
        public static void init()
        {
            Game.OnUpdate += Game_OnUpdate;
            //Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
        }

      
        public static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        private static void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!Variables.ComboMode)
            {
                var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
                foreach (var minion in minionList)
                {
                    {
                        if (minion.Health < Program._Player.GetAutoAttackDamage(minion, true) * 2)
                        {
                            return;
                        }
                    }
                }
            }
        }
        private static void Game_OnUpdate(EventArgs args)
        {
            
            //if (!Variables.ComboMode)
            //{
                //foreach (var turret in ObjectManager.Get<Obj_AI_Turret>().Where(t => Program._Player.Distance(t.Position) < 1000))
                //{
                //    var minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, turret.Position.To2D(), 900);

                //    var minions2 = minions.OrderBy(minion => turret.Distance(minion.Position));

                //    foreach (var minion in minions2.Where(minion => minion.IsValidTarget() && !IsUnderTurret(minion.ServerPosition)))
                //    {
                //        if (Program._Player.GetAutoAttackDamage(minion) > minion.Health)
                //        {
                //            Orbwalker.DisableAttacking = false;
                //        }
                //        else
                //            Orbwalker.DisableAttacking = true;

                //        var hpAfter = minion.Health % turret.GetAutoAttackDamage(minion);

                //        if (hpAfter > Program._Player.GetAutoAttackDamage(minion))
                //        {
                //            Orbwalker.DisableAttacking = false;
                //            Orbwalker.ForcedTarget = minion;
                //            return;
                //        }
                //    }
                //    Orbwalker.DisableAttacking = true;
                //}
            //}
            //if (!Variables.ComboMode)
            //{
            //    var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
            //    foreach (var minion in minionList)
            //    {
            //        {
            //            if (minion.Health < Program._Player.GetAutoAttackDamage(minion, true))
            //            {
            //                Orbwalker.ForcedTarget = minion;
            //            }
            //            else if (minion.Health < Program._Player.GetAutoAttackDamage(minion, true) * 2)
            //            {
            //                Orbwalker.DisableAttacking = true;
            //            }
            //        }
            //    }
            //    Orbwalker.DisableAttacking = false;
            //}
        }
    }
}
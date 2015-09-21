﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Menu.Values;

namespace VnHarry_Twisted_Fate
{
    class StateHandler
    {
        public static AIHeroClient _Player { get { return ObjectManager.Player; } }
        public static float GetDynamicRange()
        {
            if (Program.Q.IsReady())
            {
                return Program.Q.Range;
            }
            return _Player.GetAutoAttackRange();
        }
        public static void Combo()
        {
            var target = TargetSelector2.GetTarget(GetDynamicRange() + 100, DamageType.Magical);
            if (target == null) return;
            Program.Q.Cast(target);
        }

        public static void Harass()
        {
            var target = TargetSelector2.GetTarget(GetDynamicRange() + 100, DamageType.Magical);
            if (target == null) return;
            CardSelector.StartSelecting(Cards.Blue);
            Program.Q.Cast(target);          
        }

        public static void LastHit()
        {
            var minion = ObjectManager.Get<Obj_AI_Minion>().FirstOrDefault(a => a.IsEnemy && a.Health <= QDamage(a));
            if (minion == null) return;
            CardSelector.StartSelecting(Cards.Blue);
        }

        public static void WaveClear()
        {
            var allMinionsQ = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Player.Instance.ServerPosition.To2D(), Program.Q.Range + Program.Q.Width + 30);
            if (allMinionsQ == null) return;

            if (Program._Player.ManaPercent >= Program.LaneClearMenu["laneclear.mana"].Cast<Slider>().CurrentValue)
            {
                CardSelector.StartSelecting(Cards.Red);
                foreach (Obj_AI_Minion minion in allMinionsQ)
                {
                    if ((!Player.Instance.IsInAutoAttackRange(minion) || (!Orbwalker.CanAutoAttack && Orbwalker.LastTarget.NetworkId != minion.NetworkId)) && (minion.Health < 0.8 * QDamage(minion)))
                    {
                        Program.Q.Cast(minion);
                        break;
                    }
                }
            }
            else
            {
                CardSelector.StartSelecting(Cards.Blue);
            }
        }


        public static float QDamage(Obj_AI_Base target)
        {
            return _Player.CalculateDamageOnUnit(target, DamageType.Magical,
                (float) (new[] {60, 110, 160, 210, 260}[Program.Q.Level] + 0.65*_Player.FlatMagicDamageMod));
        }
    }
}

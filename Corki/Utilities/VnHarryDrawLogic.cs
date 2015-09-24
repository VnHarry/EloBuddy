using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Rendering;
using System;

namespace VnHarry_AIO.Utilities
{
    public class VnHarryDrawLogic
    {
        public static void init()
        {
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program._Player.IsDead)
            {
                var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
                foreach (Obj_AI_Minion minion in minionList)
                {
                    if (minion != null)
                    {
                        if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true))
                            new Circle { Color = System.Drawing.Color.Lime, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                        else if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true) * 2)
                            new Circle { Color = System.Drawing.Color.Gold, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                    }
                }
            }
            if (!Program._Player.IsDead)
            {
                var target = Orbwalker.GetTarget();

                if (target != null)
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = target.BoundingRadius + 15 }.Draw(target.Position);
            }
            //

        }
    }
}
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System;
using System.Drawing;
using System.Linq;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Utilities;

namespace VnHarry_AIO.Marksman
{
    internal class Graves : PluginBase
    { 

        public static Spell.Skillshot _Q;
        public static Spell.Skillshot _W;
        public static Spell.Active _E;
        public static Spell.Skillshot _R;

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
            _Q = new Spell.Skillshot(SpellSlot.Q, (int)720f, SkillShotType.Cone, (int)0.25f, (int)2000f, (int)(15f * (float)Math.PI / 180));
            _W = new Spell.Skillshot(SpellSlot.W, (int)850f, SkillShotType.Circular, (int)0.25f, (int)1650f, (int)250f);
            _R = new Spell.Skillshot(SpellSlot.R, (int)1100f, SkillShotType.Linear, (int)0.25f, (int)2100f, (int)100f);
            _E = new Spell.Active(SpellSlot.E, (int)425f);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo"));
            Variables.Config.Add("commbo.w", new CheckBox("Use E in Combo"));
            Variables.Config.Add("commbo.e", new CheckBox("Use E in Combo"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo"));
            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.q", new CheckBox("Use Q in Harass", false));
            Variables.Config.AddGroupLabel("LaneClear");
            Variables.Config.Add("laneclear.mana", new Slider("Mana manager (%)", 50, 0, 100));
            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use E upon Gapcloser", false));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.w", new CheckBox("Draw W"));
            Variables.Config.Add("draw.r", new CheckBox("Draw R"));
        }
        public static float GetComboDamage(Obj_AI_Base enemy)
        {
            //Chat.Print("GetComboDamage");
            var damage = 0f;

            if (_Q.IsReady())
                damage += QDamage(enemy);

            if (_W.IsReady())
                damage += WDamage(enemy);

            if (_E.IsReady())
                damage += RDamage(enemy);

            Chat.Print("GetComboDamage: " + damage.ToString());

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
            //var target = VnHarryWalker.GetTarget();
            //Chat.Print("ten ne: " + target.Name);

            //if (_Q.IsReady())
            //{
            //    foreach (var enemy in HeroManager.Enemies.Where(o => o.IsValidTarget(_Q.Range) && !o.IsDead && !o.IsZombie))
            //    {
            //        var pred = _Q.GetPrediction(enemy);
            //        if ((pred.HitChance == HitChance.Immobile) ||
            //            (pred.HitChance == HitChance.Dashing))
            //        {
            //            _Q.Cast(pred.CastPosition);
            //        }
            //    }
            //}

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
            //Chat.Print("Clear");
            //var target = VnHarryWalker.GetTarget();
            //Chat.Print("ten ne: " + target.Name);
            
            var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
            foreach (Obj_AI_Minion minion in minionList)
            {
                {


                    if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true))
                    {
                       

                        if (minion != null && (VnHarryWalker.CanAttack || VnHarryWalker.HaveCancled) && VnHarryWalker.IsAllowedToAttack())
                        {
                          
                                if (Variables.HarassMode)
                                {
                                    Player.IssueOrder(GameObjectOrder.AttackUnit, minion);
                                    VnHarryWalker._lastAaTick = Environment.TickCount + Game.Ping / 2;
                                    
                                }
                            
                        }
                        
                       
                        //Chat.Print("ten ne: " + minion.Name);
                    }


                }
            }
            var allMinionsQ = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), _Q.Range + _Q.Width + 30);
            if (allMinionsQ == null) return;

            if (Program._Player.ManaPercent >= Variables.Config["laneclear.mana"].Cast<Slider>().CurrentValue)
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
            //Chat.Print("Harass");
            var target = TargetSelector2.GetTarget(_Q.Range + 100, DamageType.Magical);
            if (target == null) return;
            _Q.Cast(target);
        }

        private void Combo()
        {
            //Chat.Print("Combo");
            var useQ = Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue;
            var useW = Variables.Config["commbo.w"].Cast<CheckBox>().CurrentValue;
            var useE = Variables.Config["commbo.e"].Cast<CheckBox>().CurrentValue;
            var useR = Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue;

            var qTarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Physical);
            var wTarget = TargetSelector2.GetTarget(_W.Range, DamageType.Magical);
            var rTarget = TargetSelector2.GetTarget(_R.Range, DamageType.Physical);

            var comboDamage = rTarget != null ? GetComboDamage(rTarget) : 0;

            if (_Q.IsReady() && useQ && qTarget.IsValidTarget())
            {
                _Q.Cast(qTarget);
            }
            if (_W.IsReady() && useQ && wTarget.IsValidTarget())
            {
                _W.Cast(qTarget);
            }
            if (_E.IsReady() && useE)
            {
                _E.Cast(Game.CursorPos);
            }
            if (_R.IsReady() && useR && rTarget.IsValidTarget() && (comboDamage > rTarget.Health))
            //if (_R.IsReady() && useR && rTarget.IsValidTarget())
            {
                _R.Cast(rTarget);
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

                if (Variables.Config["draw.w"].Cast<CheckBox>().CurrentValue && _W.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);

                }
                if (Variables.Config["draw.r"].Cast<CheckBox>().CurrentValue && _R.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.White, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
                }
            }

            if (true)
            {
                var minionList = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), Program._Player.AttackRange + 500);
                foreach (Obj_AI_Minion minion in minionList)
                {
                    {

                        //var attackToKill = Math.Ceiling(minion.MaxHealth / Program._Player.GetAutoAttackDamage(minion, true));
                        //var hpBarPosition = minion.HPBarPosition;
                        //var barWidth = minion.IsMelee ? 75 : 80;
                        //if (minion.HasBuff("turretshield"))
                        //    barWidth = 70;
                        //var barDistance = (float)(barWidth / attackToKill);

                        //for (var i = 1; i < attackToKill; i++)
                        //{
                        //    var startposition = hpBarPosition.X + 45 + barDistance * i;
                        //    Drawing.DrawLine(
                        //        new Vector2(startposition, hpBarPosition.Y + 18),
                        //        new Vector2(startposition, hpBarPosition.Y + 23),
                        //        1,
                        //        System.Drawing.Color.Black);
                        //}

                        if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true))
                            new Circle { Color = System.Drawing.Color.Lime, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                        else if (minion.Health <= Program._Player.GetAutoAttackDamage(minion, true) * 2)
                            new Circle { Color = System.Drawing.Color.Gold, BorderWidth = 1, Radius = minion.BoundingRadius }.Draw(minion.Position);
                    }
                }
            }

        }
    }

}
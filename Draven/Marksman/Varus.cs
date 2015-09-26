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

namespace VnHarry_AIO.Marksman
{
    internal class Varus : PluginBase
    {
        private static Spell.Skillshot _E;
        private static Spell.Skillshot _Q;
        private static Spell.Skillshot _R;
        private static Spell.Skillshot _W;
        float CastTime = Game.Time;
        bool CanCast = true;

        public Varus()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
            AIHeroClient.OnProcessSpellCast += AIHeroClient_OnProcessSpellCast;
        }

        private void AIHeroClient_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                if (args.SData.Name == "VarusQ" || args.SData.Name == "VarusE" || args.SData.Name == "VarusR")
                {
                    CastTime = Game.Time;
                    CanCast = false;
                }
            }
        }

        #region Setup

        public override sealed void _SetupSpells()
        {
            
            _Q = new Spell.Skillshot(SpellSlot.Q, 925, SkillShotType.Linear, 250, 1900, 70);
            _W = new Spell.Skillshot(SpellSlot.W, 975, SkillShotType.Linear, 200, 1500, 120);
            _R = new Spell.Skillshot(SpellSlot.R, 1050, SkillShotType.Linear,250, 1950, 120);
            _Q.Set("VarusQ", "VarusQ", 925, 1600, 1.5f);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo"));
            Variables.Config.Add("commbo.w", new CheckBox("Use W in Combo"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo"));
            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.q", new CheckBox("Use Q in Harass"));
            Variables.Config.Add("harass.w", new CheckBox("Use W in Harass"));
            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use W upon Gapcloser", false));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.w", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.r", new CheckBox("Draw R"));
        }

        #endregion Setup

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue)
            {
                return;
            }
            if (_R.IsReady() && Variables.Config["misc.antigapcloser"].Cast<CheckBox>().CurrentValue)
            {
                var Target = sender;
                if (Target.IsValidTarget(_R.Range))
                {
                    _R.Cast(Target);
                }
            }      
        }

     

        public override void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

           
                if (!CanCast)
                {
                    if (Game.Time - CastTime > 1)
                    {
                        CanCast = true;
                        return;
                    }
                    var t = Orbwalker.GetTarget() as Obj_AI_Base;
                    if (t.IsValidTarget())
                    {
                        if (t.GetBuffCount("varuswdebuff") < 3)
                            CanCast = true;
                    }
                    else
                    {
                        CanCast = true;
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
            //code here
        }

        private void Harass()
        {
           
        }

        private void Combo()
        {
            try
            {
            
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
                if (Variables.Config["draw.w"].Cast<CheckBox>().CurrentValue && _Q.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);
                }
                if (Variables.Config["draw.r"].Cast<CheckBox>().CurrentValue && _R.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
                }
            }
        }
    }
}

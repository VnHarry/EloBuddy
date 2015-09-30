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
    internal class Ashe : PluginBase
    {
        
        private static Spell.Active _Q;
        private static Spell.Skillshot _W;
        private static Spell.Active _E;
        private static Spell.Skillshot _R;

        public Ashe()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            DamageIndicator.Initialize(GetComboDamage);
        }

        public override void Game_OnTick(EventArgs args)
        {
            Orbwalker.ForcedTarget = null;

            //adjust range
            if (_R.IsReady())
            {
              //  _R.Range = Variables.GetSliderConfig("R_Max_Range");
            }

            if (Variables.GetCheckBoxConfig("misc.ksR"))
            {
                CheckKs();
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
                LaneClear();
            }

        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variables.GetCheckBoxConfig(MessageText.udrawW) && _W.IsReady() && _W.Level >= 1)
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);
            }
        }
        private void Combo()
        {
            UseSpells(Variables.GetCheckBoxConfig(MessageText.ucomboQ), Variables.GetCheckBoxConfig(MessageText.ucomboW),
                false, Variables.GetCheckBoxConfig(MessageText.ucomboR), "Combo");
        }

        private void Harass()
        {
            UseSpells(Variables.GetCheckBoxConfig(MessageText.uharassQ), Variables.GetCheckBoxConfig(MessageText.uharassW),
                false, false, "Harass");
        }
        private void LaneClear()
        {
            if (Variables.GetSliderConfig(MessageText.ulaneclearMana) < Program._Player.ManaPercent)
                return;

            var allMinionsQ = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), _Q.Range);

            var useQ = Variables.GetCheckBoxConfig(MessageText.ulaneclearQ);
            var useW = Variables.GetCheckBoxConfig(MessageText.ulaneclearW);

            if (useQ && allMinionsQ.Count > 0 && _Q.IsReady())
            {
                    var qMin = Variables.GetSliderConfig("Q_Min_Stack");

                    if (qMin <= QStacks)
                        _Q.Cast();
              
            }

            if (useW)
            {
               
            }
        }
        public override sealed void _SetupSpells()
        {
            _Q = new Spell.Active(SpellSlot.Q);
            _W = new Spell.Skillshot(SpellSlot.W, 1200, SkillShotType.Cone, (int)0.25f, (int)1500f, (int)0.25f);
            _E = new Spell.Active(SpellSlot.E);
            _R = new Spell.Skillshot(SpellSlot.R, 20000, SkillShotType.Linear, (int)250f, (int)1600f, (int)130f);
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Q Settings");
            Variables.Config.Add("Q_Min_Stack", new Slider("Require Q Min Stacks", 5, 0, 5));
            Variables.Config.AddGroupLabel("R Settings");
            Variables.Config.Add("R_Min_Range", new Slider("R Min Range Sliders", 300, 0, 1000));
            Variables.Config.Add("R_Max_Range", new Slider("R Max Range Sliders", 2000, 0, 4000));
            Variables.Config.AddGroupLabel("Don't use R on");
            foreach (var enemy in HeroManager.Enemies)
            {
                Variables.Config.Add("Dont_R" + enemy.ChampionName, new CheckBox(enemy.ChampionName));
            }
            Variables.Config.AddGroupLabel(MessageText.txtCombo);
            Variables.Config.Add(MessageText.ucomboQ, new CheckBox(MessageText.txtcomboQ));
            Variables.Config.Add(MessageText.ucomboW, new CheckBox(MessageText.txtcomboW));
            Variables.Config.Add(MessageText.ucomboR, new CheckBox(MessageText.txtcomboR));
            Variables.Config.AddGroupLabel(MessageText.txtHarass);
            Variables.Config.Add(MessageText.uharassQ, new CheckBox(MessageText.txtharassQ));
            Variables.Config.Add(MessageText.uharassW, new CheckBox(MessageText.txtharassW));
            Variables.Config.Add(MessageText.uharassMana, new Slider(MessageText.txtharassMana, 50, 0, 100));
            Variables.Config.AddGroupLabel(MessageText.txtLaneClear);
            Variables.Config.Add(MessageText.ulaneclearQ, new CheckBox(MessageText.txtlaneclearQ));
            Variables.Config.Add(MessageText.ulaneclearW, new CheckBox(MessageText.txtlaneclearW, false));
            Variables.Config.Add(MessageText.ulaneclearMana, new Slider(MessageText.ulaneclearMana, 50, 0, 100));
            Variables.Config.AddGroupLabel(MessageText.txtMisc);
            Variables.Config.Add("misc.smartKS", new CheckBox("Smart KS"));
            Variables.Config.Add("misc.ksR", new CheckBox("KS with R"));
            Variables.Config.Add("misc.UseInt", new CheckBox("Use R to Interrupt"));
            Variables.Config.AddGroupLabel(MessageText.txtDraw);
            Variables.Config.Add(MessageText.udrawW, new CheckBox(MessageText.txtdrawW));
        }
        private float GetComboDamage(Obj_AI_Base target)
        {
            double comboDamage = 0;

            if (_Q.IsReady())
                comboDamage += Program._Player.GetSpellDamage(target, SpellSlot.Q);

            if (_W.IsReady())
                comboDamage += Program._Player.GetSpellDamage(target, SpellSlot.W);

            if (_R.IsReady())
                comboDamage += Program._Player.GetSpellDamage(target, SpellSlot.R);

            return (float)(comboDamage + Program._Player.GetAutoAttackDamage(target) * 2);
        }

        private int QStacks
        {
            get
            {
                return
                    (from buff in Program._Player.Buffs
                     where buff.Name == "asheqcastready" || buff.Name == "AsheQ"
                     select buff.Count).FirstOrDefault();
            }
        }
        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && Variables.GetSliderConfig(MessageText.uharassMana) < Program._Player.ManaPercent)
                return;

            var target = TargetSelector2.GetTarget(Variables.GetSliderConfig("R_Max_Range"), DamageType.Physical);

            if (target.IsValidTarget(Variables.GetSliderConfig("R_Max_Range")))
            {
                var dmg = GetComboDamage(target);

                if (useR && dmg > target.Health && Program._Player.Distance(target) > Variables.GetSliderConfig("R_Min_Range"))
                {
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        if (!Variables.GetCheckBoxConfig("Dont_R" + enemy.ChampionName))
                        {
                            _R.Cast(target);
                        }
                    }
                    
                }
                if (_Q.IsReady() && Program._Player.Distance(target) < 550)
                {
                    var qMin = Variables.GetSliderConfig("Q_Min_Stack");

                    if (qMin <= QStacks)
                        _Q.Cast();
                }
            }
            var Wtarget = TargetSelector2.GetTarget(_W.Range, DamageType.Physical);
            if (useW && _W.IsReady())
            {
                _W.Cast(Wtarget);
            }
                
        }
        private void CheckKs()
        {
            foreach (Obj_AI_Base target in ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidTarget(Variables.GetSliderConfig("R_Max_Range"))).OrderByDescending(GetComboDamage))
            {
                //W
                if (Program._Player.Distance(target) <= _W.Range && Program._Player.GetSpellDamage(target, SpellSlot.W) > target.Health && _W.IsReady())
                {
                    _W.Cast(target);
                    return;
                }

                //R
                if (Program._Player.Distance(target) <= Variables.GetSliderConfig("R_Max_Range") && Program._Player.GetSpellDamage(target, SpellSlot.R) > target.Health && _R.IsReady() && Variables.GetCheckBoxConfig("misc.ksR"))
                {
                    foreach (var enemy in HeroManager.Enemies)
                    {
                        if (!Variables.GetCheckBoxConfig("Dont_R" + enemy.ChampionName))
                        {
                            _R.Cast(target);
                        }
                    }
                    return;
                }
            }
        }
    }
}

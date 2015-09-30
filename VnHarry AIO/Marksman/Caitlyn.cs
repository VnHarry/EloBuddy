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
    internal class Caitlyn : PluginBase
    {
        public static Spell.Skillshot _Q;
        public static Spell.Targeted _W;
        public static Spell.Skillshot _E;
        public static Spell.Targeted _R;

        public Caitlyn()
        {
            _SetupMenu();
            _SetupSpells();

            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            DamageIndicator.Initialize(GetComboDamage);
            Gapcloser.OnGapcloser += Gapcloser_OnGapcloser;
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
                LaneClear();
            }

            

            if (Variables.GetKeyConfig("misc.dash"))
            {
                if (_E.IsReady())
                    if (_E.Cast(ObjectManager.Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -300).To3D()))
                    {
                        Variables.Config["misc.dash"].Cast<KeyBind>().CurrentValue = false;
                    }
            }
            if (Variables.GetCheckBoxConfig("misc.smartQ"))
            {
                SmartQ();
            }
            if (Variables.GetCheckBoxConfig("misc.smartW"))
            {
                SmartW();
            }
            if (Variables.GetCheckBoxConfig("misc.smartR"))
            {
                SmartR();
            }
        }

        private static void Combo()
        {
            if (Variables.GetCheckBoxConfig(MessageText.ucomboQ))
            {
                var Qtarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Physical);

                if (_Q.IsReady())
                    _Q.Cast(Qtarget);
            }

            if (Variables.GetCheckBoxConfig(MessageText.ucomboW))
            {
                var Wtarget = W_GetBestTarget();

                if (_W.IsReady() && !Wtarget.HasBuffOfType(BuffType.SpellImmunity))
                    _W.Cast(Wtarget);
            }

            if (Variables.GetCheckBoxConfig(MessageText.ucomboR) && _R.IsReady())
            {
                foreach (var Rtarget in HeroManager.Enemies.Where(t => t.IsValidTarget(1500 + (500 * _R.Level)) && !t.IsValidTarget(Program._Player.GetAutoAttackRange()) && !t.HasBuffOfType(BuffType.Invulnerability) && !t.HasBuffOfType(BuffType.SpellShield)))
                {
                    if (Rtarget.Health + Rtarget.HPRegenRate <= GetComboDamage(Rtarget))
                        _R.Cast(Rtarget);
                }
            }
        }

        private static void Harass()
        {
            if (Program._Player.ManaPercent > Variables.GetSliderConfig(MessageText.uharassMana))
                return;

            if (Variables.GetCheckBoxConfig(MessageText.uharassQ))
            {
                var Qtarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Physical);

                if (_Q.IsReady())
                    _Q.Cast(Qtarget);
            }
        }

        private static void LaneClear()
        {
            if (Program._Player.ManaPercent > Variables.GetSliderConfig(MessageText.ulaneclearMana))
                return;

            var Minions = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), _Q.Range);

            if (Minions.Count <= 0)
                return;

            if (Variables.GetCheckBoxConfig(MessageText.ulaneclearQ))
            {
                //var farmloc = _Q.IsInRange(Minions.);

                //if (farmloc.MinionsHit >= 3)
                //    Q.Cast(farmloc.Position);
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (Variables.GetCheckBoxConfig(MessageText.udrawQ) && _Q.IsReady() && _Q.Level >= 1)
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
            }
            if (Variables.GetCheckBoxConfig(MessageText.udrawW) && _W.IsReady() && _W.Level >= 1)
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);
            }
            if (Variables.GetCheckBoxConfig(MessageText.udrawE) && _E.IsReady() && _E.Level >= 1)
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _E.Range }.Draw(Player.Instance.Position);
            }
            if (Variables.GetCheckBoxConfig(MessageText.udrawR) && _R.IsReady() && _R.Level >= 1)
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
            }

            if (Variables.GetKeyConfig("misc.dash"))
            {
                new Circle { Color = System.Drawing.Color.Gold, Radius = 50 }.Draw(Game.CursorPos);
                var targetpos = Drawing.WorldToScreen(Game.CursorPos);
                Drawing.DrawText(targetpos[0] - 30, targetpos[1] + 40, System.Drawing.Color.Gold, "Dash");
            }
        }

        private void Gapcloser_OnGapcloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            if (!Variables.GetCheckBoxConfig("misc.antigapcloser") || Program._Player.IsDead)
                return;

            if (e.Sender.IsValidTarget(1000))
            {
                new Circle { Color = System.Drawing.Color.Red, BorderWidth = 5, Radius = e.Sender.BoundingRadius }.Draw(e.Sender.Position);
                var targetpos = Drawing.WorldToScreen(e.Sender.Position);
                Drawing.DrawText(targetpos[0] - 40, targetpos[1] + 20, System.Drawing.Color.Gold, "Gapcloser");
            }

            if (_E.IsInRange(e.Sender) && _E.IsReady())
                _E.Cast(e.Sender);
            else if (_W.IsInRange(e.Sender) && _W.IsReady())
                _W.Cast(e.End);
        }

      
        private static void SmartW()
        {
            foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(_W.Range) && !target.IsDead &&
                    (target.HasBuffOfType(BuffType.Stun)
                    || target.HasBuffOfType(BuffType.Snare)
                    || target.HasBuffOfType(BuffType.Taunt)
                    || target.HasBuffOfType(BuffType.Knockup)
                    || target.HasBuff("Recall"))))
            {
                _W.Cast(enemy);
            }
        }

        private static void SmartQ()
        {
            foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(_Q.Range) && !target.IsDead &&
                    (target.HasBuffOfType(BuffType.Stun)
                    || target.HasBuffOfType(BuffType.Snare)
                    || target.HasBuffOfType(BuffType.Taunt)
                    || target.HasBuffOfType(BuffType.Slow)
                    || target.HasBuffOfType(BuffType.Suppression)
                    || target.HasBuffOfType(BuffType.Charm)
                    || target.HasBuffOfType(BuffType.Knockup)
                    || target.HasBuff("Recall"))))
            {
                _Q.Cast(enemy);
            }
        }
        private static void SmartR()
        {
            if (_R.IsReady())
            {
                foreach (var enemy in HeroManager.Enemies.Where(target => target.IsValidTarget(_R.Range)
                                                                      && !target.IsDead
                                                                      && !target.IsZombie
                                                                      && target.Health <= GetComboDamage(target)))
                {
                    _R.Cast(enemy);
                }
            }
        }
        private static float GetComboDamage(Obj_AI_Base target)
        {
            if (_R.IsReady())
            {
                return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                 (float)(new[] { 250, 475, 700 }[_R.Level - 1] + 2.0 * Program._Player.FlatPhysicalDamageMod));
            }
            else
                return 0;
        }

        private static double UnitIsImmobileUntil(Obj_AI_Base unit)
        {
            var result =
                unit.Buffs.Where(
                    buff =>
                        buff.IsActive && Game.Time <= buff.EndTime &&
                        (buff.Type == BuffType.Charm || buff.Type == BuffType.Knockup || buff.Type == BuffType.Stun ||
                         buff.Type == BuffType.Suppression || buff.Type == BuffType.Snare))
                    .Aggregate(0d, (current, buff) => Math.Max(current, buff.EndTime));
            return (result - Game.Time);
        }

        public override sealed void _SetupSpells()
        {
        
            // SpellSlot , Range , Skillshot type , Cast  delay , width 
            _Q = new Spell.Skillshot(SpellSlot.Q, 1240, SkillShotType.Linear, (int)0.25f, (int)2000f, (int)60f);
            _W = new Spell.Targeted(SpellSlot.W, 820);
            _E = new Spell.Skillshot(SpellSlot.E, 800, SkillShotType.Linear, (int)0.25f, (int)1600f, (int)80f);
            _R = new Spell.Targeted(SpellSlot.R, 2000);
        }

        private static Obj_AI_Base W_GetBestTarget()
        {
            return HeroManager.Enemies.Where(x => _W.IsReady() && !x.HasBuffOfType(BuffType.SpellImmunity) && !x.IsFacing(Program._Player) && x.IsValidTarget(550)).OrderBy(x => x.Distance(Program._Player, false)).FirstOrDefault();
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel(MessageText.txtCombo);
            Variables.Config.Add(MessageText.ucomboQ, new CheckBox(MessageText.txtcomboQ));
            Variables.Config.Add(MessageText.ucomboW, new CheckBox(MessageText.txtcomboW));
            Variables.Config.Add(MessageText.ucomboR, new CheckBox(MessageText.txtcomboR));
            Variables.Config.AddGroupLabel(MessageText.txtHarass);
            Variables.Config.Add(MessageText.uharassQ, new CheckBox(MessageText.txtharassQ));
            Variables.Config.Add(MessageText.uharassMana, new Slider(MessageText.txtharassMana, 50, 0, 100));
            Variables.Config.AddGroupLabel(MessageText.txtLaneClear);
            Variables.Config.Add(MessageText.ulaneclearQ, new CheckBox(MessageText.txtlaneclearQ));
            Variables.Config.Add(MessageText.ulaneclearMana, new Slider(MessageText.ulaneclearMana, 50, 0, 100));
            Variables.Config.AddGroupLabel(MessageText.txtMisc);
            Variables.Config.Add("misc.antigapcloser", new CheckBox("Use Anti-Gapcloser"));
            Variables.Config.Add("misc.smartQ", new CheckBox("Smart W"));
            Variables.Config.Add("misc.smartW", new CheckBox("Smart Q"));
            Variables.Config.Add("misc.smartR", new CheckBox("Smart R"));
            Variables.Config.Add("misc.dash", new KeyBind("Dash to Mouse", false, KeyBind.BindTypes.HoldActive, 'G'));
            Variables.Config.AddGroupLabel(MessageText.txtDraw);
            Variables.Config.Add(MessageText.udrawQ, new CheckBox(MessageText.txtdrawQ));
            Variables.Config.Add(MessageText.udrawW, new CheckBox(MessageText.txtdrawW));
            Variables.Config.Add(MessageText.udrawE, new CheckBox(MessageText.txtdrawE));
            Variables.Config.Add(MessageText.udrawR, new CheckBox(MessageText.txtdrawR));
        }
    }
}
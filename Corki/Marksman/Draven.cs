using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using VnHarry_AIO.Internal;

namespace VnHarry_AIO.Marksman
{
    internal class Draven : PluginBase
    {
        private static Spell.Active _Q;
        private static Spell.Active _W;
        private static Spell.Skillshot _E;
        private static Spell.Skillshot _R;
        private int LastAxeMoveTime { get; set; }
        public static List<QRecticle> QReticles { get; set; }

        public int QCount
        {
            get
            {
                return (Program._Player.HasBuff("dravenspinningattack")
                            ? Program._Player.Buffs.First(x => x.Name == "dravenspinningattack").Count
                            : 0) + QReticles.Count;
            }
        }

        public Draven()
        {
            _SetupMenu();
            _SetupSpells();

            GameObject.OnCreate += GameObject_OnCreate;
            GameObject.OnDelete += GameObject_OnDelete;
            Interrupter.OnInterruptableSpell += Interrupter_OnInterruptableSpell;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            Game.OnTick += Game_OnTick;
            Drawing.OnDraw += Drawing_OnDraw;
            Gapcloser.OnGapcloser += Gapcloser_OnGapCloser;
        }

        private void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            QReticles.Add(new QRecticle(sender, Environment.TickCount + 1800));
            Core.DelayAction(() => QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId), 1800);
        }

        private void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            QReticles.RemoveAll(x => x.Object.NetworkId == sender.NetworkId);
        }

        private void Interrupter_OnInterruptableSpell(Obj_AI_Base sender, Interrupter.InterruptableSpellEventArgs e)
        {
            var useEInterrupt = Variables.Config["misc.UseEInterrupt"].Cast<CheckBox>().CurrentValue;

            if (!useEInterrupt || !_E.IsReady() || !sender.IsValidTarget(_E.Range))
            {
                return;
            }

            if (e.DangerLevel >= DangerLevel.Medium)
            {
                _E.Cast(sender);
            }
        }

        private void Orbwalker_OnPreAttack(AttackableUnit target, Orbwalker.PreAttackArgs args)
        {
            if (!_Q.IsReady())
            {
                return;
            }

            if (Variables.ComboMode)
            {
                var useQ = Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue;
                var axeSettingAxe = Variables.Config["axeSetting.axe"].Cast<Slider>().CurrentValue;

                if ((useQ) && (QCount < axeSettingAxe) && (target is AIHeroClient))
                {
                    _Q.Cast();
                }
            }

            if (Variables.LaneClearMode)
            {
                var useQ = Variables.Config["laneclear.q"].Cast<CheckBox>().CurrentValue;
                var axeSettingAxe = Variables.Config["axeSetting.axe"].Cast<Slider>().CurrentValue;
                var laneclearMana = Variables.Config["laneclear.mana"].Cast<Slider>().CurrentValue;

                if (Program._Player.ManaPercent < laneclearMana)
                {
                    return;
                }

                if (useQ && (QCount < laneclearMana) && (target as Obj_AI_Base).IsMinion)
                {
                    _Q.Cast();
                }
            }
        }

        private void Gapcloser_OnGapCloser(AIHeroClient sender, Gapcloser.GapcloserEventArgs e)
        {
            var useEGapcloser = Variables.Config["misc.UseEGapcloser"].Cast<CheckBox>().CurrentValue;
            if (!useEGapcloser || !_E.IsReady() || !sender.IsValidTarget(_E.Range))
            {
                return;
            }

            _E.Cast(sender);
        }

        #region Setup

        public override sealed void _SetupSpells()
        {
            _Q = new Spell.Active(SpellSlot.Q, (uint)Program._Player.GetAutoAttackRange());
            _W = new Spell.Active(SpellSlot.W);
            _E = new Spell.Skillshot(SpellSlot.E, 1050, SkillShotType.Linear, 250, 1400, 130);
            _R = new Spell.Skillshot(SpellSlot.R, 20000, SkillShotType.Linear, 400, 2000, 160);

            QReticles = new List<QRecticle>();
        }

        public override sealed void _SetupMenu()
        {
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q in Combo", true));
            Variables.Config.Add("commbo.w", new CheckBox("Use W in Combo", true));
            Variables.Config.Add("commbo.e", new CheckBox("Use E in Combo", true));
            Variables.Config.Add("commbo.r", new CheckBox("Use R in Combo", true));

            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.e", new CheckBox("Use Q in Harass", false));
            Variables.Config.Add("harass.mana", new Slider("Mana manager (%)", 50, 0, 100));

            Variables.Config.AddGroupLabel("LaneClear");
            Variables.Config.Add("laneclear.q", new CheckBox("Use Q in LaneClear", true));
            Variables.Config.Add("laneclear.w", new CheckBox("Use W in LaneClear", true));
            Variables.Config.Add("laneclear.e", new CheckBox("Use E in LaneClear", false));
            Variables.Config.Add("laneclear.mana", new Slider("Mana manager (%)", 50, 0, 100));

            Variables.Config.AddGroupLabel("Axe Settings");
            Variables.Config.Add("axeSetting.mode", new Slider("Catch Axe on Mode: (Combo| Any | Always)", 2, 1, 3));
            Variables.Config.Add("axeSetting.CatchAxeRange", new Slider("Catch Axe Range", 800, 120, 1500));
            Variables.Config.Add("axeSetting.axe", new Slider("Maximum Axes", 2, 1, 3));
            Variables.Config.Add("axeSetting.UseWForQ", new CheckBox("Use W if Axe Too Far", true));
            Variables.Config.Add("axeSetting.DontCatchUnderTurret", new CheckBox("Don't Catch Axe Under Turret", true));

            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.UseWSetting", new CheckBox("Use W Instantly(When Available)", false));
            Variables.Config.Add("misc.UseEGapcloser", new CheckBox("Use E on Gapcloser", true));
            Variables.Config.Add("misc.UseEInterrupt", new CheckBox("Use E to Interrupt", true));
            Variables.Config.Add("misc.UseWManaPercent", new Slider("Use W Mana Percent", 50, 0, 100));
            Variables.Config.Add("misc.UseWSlow", new CheckBox("Use W if Slowed", true));

            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.e", new CheckBox("Draw E"));
            Variables.Config.Add("draw.DrawAxeLocation", new CheckBox("Draw Axe Location"));
            Variables.Config.Add("draw.DrawAxeRange", new CheckBox("Draw Axe Catch Range"));
        }

        public static float RDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 175, 275, 375 }[_R.Level] + 1.5 * Program._Player.FlatPhysicalDamageMod));
        }

        public static bool IsUnderTurret(Vector3 position)
        {
            return ObjectManager.Get<Obj_AI_Turret>().Any(turret => turret.IsValidTarget(950) && turret.IsEnemy);
        }

        #endregion Setup

        public override void Game_OnTick(EventArgs args)
        {
            var catchOption = Variables.Config["axeSetting.mode"].Cast<Slider>().CurrentValue;

            if (((catchOption == 1 && Variables.ComboMode)
                 || (catchOption == 2 && !Variables.NoneMode)
                 || catchOption == 3) && Environment.TickCount - LastAxeMoveTime >= 50)
            {
                var CatchAxeRange = Variables.Config["axeSetting.CatchAxeRange"].Cast<Slider>().CurrentValue;
                var UseWForQ = Variables.Config["axeSetting.UseWForQ"].Cast<CheckBox>().CurrentValue;
                var DontCatchUnderTurret = Variables.Config["axeSetting.DontCatchUnderTurret"].Cast<CheckBox>().CurrentValue;
                

                var bestReticle =
                    QReticles.Where(
                        x =>
                        x.Object.Position.Distance(Game.CursorPos)
                        < CatchAxeRange)
                        .OrderBy(x => x.Position.Distance(Program._Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                if (bestReticle != null && bestReticle.Object.Position.Distance(Program._Player.ServerPosition) > 110)
                {
                    var eta = 1000 * (Program._Player.Distance(bestReticle.Position) / Program._Player.MoveSpeed);
                    var expireTime = bestReticle.ExpireTime - Environment.TickCount;

                    if (eta >= expireTime && UseWForQ)
                    {
                        _W.Cast();
                    }

                    if (DontCatchUnderTurret)
                    {
                        if (IsUnderTurret(Program._Player.ServerPosition) && IsUnderTurret(bestReticle.Object.Position))
                        {
                            LastAxeMoveTime = Environment.TickCount;

                            Orbwalker.OrbwalkTo(bestReticle.Object.Position);
                        }
                        else if (!IsUnderTurret(bestReticle.Object.Position))
                        {
                            LastAxeMoveTime = Environment.TickCount;

                            Orbwalker.OrbwalkTo(bestReticle.Object.Position);
                        }
                    }
                    else
                    {
                        LastAxeMoveTime = Environment.TickCount;

                        Orbwalker.OrbwalkTo(bestReticle.Object.Position);
                    }
                }
                else
                {
                    Orbwalker.OrbwalkTo(Game.CursorPos);
                }
            }
            else
            {
                //Orbwalker.OrbwalkTo(Game.CursorPos);
               
            }

            var UseWSlow = Variables.Config["misc.UseWSlow"].Cast<CheckBox>().CurrentValue;

            if (_W.IsReady() && UseWSlow && Program._Player.HasBuffOfType(BuffType.Slow))
            {
                _W.Cast();
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
            var useQ = Variables.Config["laneclear.q"].Cast<CheckBox>().CurrentValue;
            var useW = Variables.Config["laneclear.w"].Cast<CheckBox>().CurrentValue;
            var useE = Variables.Config["laneclear.e"].Cast<CheckBox>().CurrentValue;
            var axeSettingAxe = Variables.Config["axeSetting.axe"].Cast<Slider>().CurrentValue;

            var laneclearMana = Variables.Config["laneclear.mana"].Cast<Slider>().CurrentValue;
            if (Player.Instance.ManaPercent < laneclearMana)
            {
                return;
            }

            if (useQ && QCount < axeSettingAxe - 1 && _Q.IsReady()
                && Orbwalker.GetTarget() is Obj_AI_Minion && !Program._Player.Spellbook.IsAutoAttacking)
            {
                _Q.Cast();
            }

            if (useW && _W.IsReady()
                && Program._Player.ManaPercent > laneclearMana)
            {
                if (useW)
                {
                    _W.Cast();
                }
                else
                {
                    if (!Program._Player.HasBuff("dravenfurybuff"))
                    {
                        _W.Cast();
                    }
                }
            }

            if (!useE || !_E.IsReady())
            {
                return;
            }
        }

        private void Harass()
        {
            var harassMana = Variables.Config["harass.mana"].Cast<Slider>().CurrentValue;
            if (Program._Player.ManaPercent < harassMana)
            {
                return;
            }

            var target = TargetSelector.GetTarget(_E.Range, DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }
            var useE = Variables.Config["harass.e"].Cast<CheckBox>().CurrentValue;
            if (useE && _E.IsReady())
            {
                _E.Cast(target);
            }
        }

        private void Combo()
        {
            var target = TargetSelector.GetTarget(_E.Range, DamageType.Physical);

            if (!target.IsValidTarget())
            {
                return;
            }
            var useQ = Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue;
            var useW = Variables.Config["commbo.w"].Cast<CheckBox>().CurrentValue;
            var useE = Variables.Config["commbo.e"].Cast<CheckBox>().CurrentValue;
            var useR = Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue;
            var axeSettingAxe = Variables.Config["axeSetting.axe"].Cast<Slider>().CurrentValue;

            if (useQ && QCount < axeSettingAxe - 1 && _Q.IsReady()
                && Program._Player.IsInAutoAttackRange(target) && !Program._Player.Spellbook.IsAutoAttacking)
            {
                _Q.Cast();
            }

            if (useW && _W.IsReady() && !Program._Player.HasBuff("dravenfurybuff"))
            {
                _W.Cast();
            }

            if (useE && _E.IsReady())
            {
                _E.Cast(target);
            }

            if (useR && _R.IsReady())
            {
                var targetR =
                    HeroManager.Enemies.Where(h => h.IsValidTarget(2000))
                        .FirstOrDefault(
                            h =>
                                RDamage(h) * 2 > h.Health
                                &&
                                (!Program._Player.IsInAutoAttackRange(h) ||
                                 Program._Player.CountEnemiesInRange(_E.Range) > 2));

                if (targetR != null)
                {
                    _R.Cast(targetR);
                }
            }
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            var drawE = Variables.Config["draw.e"].Cast<CheckBox>().CurrentValue;
            var drawAxeLocation = Variables.Config["draw.DrawAxeLocation"].Cast<CheckBox>().CurrentValue;
            var drawAxeRange = Variables.Config["draw.DrawAxeRange"].Cast<CheckBox>().CurrentValue;
            var CatchAxeRange = Variables.Config["axeSetting.CatchAxeRange"].Cast<Slider>().CurrentValue;

            if (!Program._Player.IsDead)
            {
                if (drawE && _E.IsReady())
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _E.Range }.Draw(Player.Instance.Position);
                }
                if (drawAxeLocation)
                {
                    var bestAxe = QReticles.Where(
                        x =>
                        x.Position.Distance(Game.CursorPos) < CatchAxeRange)
                        .OrderBy(x => x.Position.Distance(Program._Player.ServerPosition))
                        .ThenBy(x => x.Position.Distance(Game.CursorPos))
                        .FirstOrDefault();

                    if (bestAxe != null)
                    {
                        new Circle { Color = System.Drawing.Color.LimeGreen, BorderWidth = 1, Radius = 120 }.Draw(bestAxe.Position);
                    }

                    foreach (var axe in
                        QReticles.Where(x => x.Object.NetworkId != (bestAxe == null ? 0 : bestAxe.Object.NetworkId)))
                    {
                        new Circle { Color = System.Drawing.Color.Yellow, BorderWidth = 1, Radius = 120 }.Draw(bestAxe.Position);
                    }
                }
                if (drawAxeRange)
                {
                    new Circle { Color = System.Drawing.Color.DodgerBlue, BorderWidth = 1, Radius = CatchAxeRange }.Draw(Game.CursorPos);
                }
            }
        }
    }

    /// <summary>
    ///     A represenation of a Q circle on Draven.
    /// </summary>
    internal class QRecticle
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="QRecticle" /> class.
        /// </summary>
        /// <param name="rectice">The rectice.</param>
        /// <param name="expireTime">The expire time.</param>
        public QRecticle(GameObject rectice, int expireTime)
        {
            this.Object = rectice;
            this.ExpireTime = expireTime;
        }

        #endregion Constructors and Destructors

        #region Public Properties

        /// <summary>
        ///     Gets or sets the expire time.
        /// </summary>
        /// <value>
        ///     The expire time.
        /// </value>
        public int ExpireTime { get; set; }

        /// <summary>
        ///     Gets or sets the object.
        /// </summary>
        /// <value>
        ///     The object.
        /// </value>
        public GameObject Object { get; set; }

        /// <summary>
        ///     Gets the position.
        /// </summary>
        /// <value>
        ///     The position.
        /// </value>
        public Vector3 Position
        {
            get
            {
                return this.Object.Position;
            }
        }

        #endregion Public Properties
    }
}
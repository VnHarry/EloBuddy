using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Enumerations;
using EloBuddy.SDK.Menu.Values;
using EloBuddy.SDK.Rendering;
using System;
using System.Linq;
using VnHarry_AIO.Internal;
using VnHarry_AIO.Utilities;

namespace VnHarry_AIO.Marksman
{
    internal class Kalista : PluginBase
    {
        private static Spell.Skillshot _Q;
        private static Spell.Active _W;
        private static Spell.Active _E;
        private static Spell.Active _R;

        private static readonly float[] RRD = { 19, 29, 39, 49, 59 };
        private static readonly float[] RRDM = { 0.6f, 0.6f, 0.6f, 0.6f, 0.6f };
        private static readonly float[] RRPS = { 10, 14, 19, 25, 32 };
        private static readonly float[] RRPSM = { 0.2f, 0.225f, 0.25f, 0.275f, 0.3f };

        public static string[] Marksman = { "Kalista", "Jinx", "Lucian", "Quinn", "Draven",  "Varus", "Graves", "Vayne",
                                            "Caitlyn","Urgot", "Ezreal", "KogMaw", "Ashe", "MissFortune", "Tristana", "Teemo",
                                            "Sivir","Twitch", "Corki"};

        public static string[] tankySupport = { "Alistar", "Braum", "Leona", "Nunu", "Tahm Kench", "Taric", "Thresh" };

        public static string[] nontankySupport = { "Nami","Soraka","Janna","Sona","Lulu","Kayle","Bard","Karma","Lux","Morgana",
                                                 "Zilean","Zyra"};

        public Kalista()
        {
            _SetupMenu();
            _SetupSpells();

            DamageIndicator.Initialize(GetTotalDamage);
            Game.OnTick += Game_OnTick;
            Obj_AI_Base.OnProcessSpellCast += OnProcessSpellCast;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        public override sealed void _SetupSpells()
        {
            _Q = new Spell.Skillshot(SpellSlot.Q, 1150, SkillShotType.Linear, 250, 1200, 40);
            _W = new Spell.Active(SpellSlot.W, 5000);
            _E = new Spell.Active(SpellSlot.E, 1000);
            _R = new Spell.Active(SpellSlot.R, 1500);
        }

        public override sealed void _SetupMenu()
        {
            //VN
            //Variables.Config.AddGroupLabel("Combo");
            //Variables.Config.Add("commbo.q", new CheckBox("Sử dụng Q trong Combo"));
            //Variables.Config.Add("commbo.e", new CheckBox("Sử dụng E trong Combo"));
            //Variables.Config.Add("commbo.r", new CheckBox("Sử dụng R trong Combo"));
            //Variables.Config.Add("commbo.rCount", new Slider("R on x Enemy", 3, 0, 10));
            //Variables.Config.AddGroupLabel("KillSteal");
            //Variables.Config.Add("killsteal.q", new CheckBox("Sử dụng Q trong KillSteal"));
            //Variables.Config.Add("killsteal.e", new CheckBox("Sử dụng E trong KillSteal"));
            //Variables.Config.AddGroupLabel("LaneClear");
            //Variables.Config.Add("laneclear.e", new CheckBox("Sử dụng E trong  LaneClear"));
            //Variables.Config.Add("LaneClear.eClearCount", new Slider("If Can Kill Minion >= ", 2, 1, 5));
            //Variables.Config.Add("LaneClear.mana", new Slider("LaneClear Mana Manager ", 20, 0, 100));
            //Variables.Config.AddGroupLabel("Harass");
            //Variables.Config.Add("harass.q", new CheckBox("Sử dụng Q trong  Harass", true));
            //Variables.Config.Add("harass.r", new CheckBox("Sử dụng R trong in Harass"));
            //Variables.Config.Add("harass.eSpearCount", new Slider("If Enemy Spear Count >= ", 3, 0, 10));
            //Variables.Config.Add("harass.mana", new Slider("Harass Mana Manager ", 20, 0, 100));
            //Variables.Config.AddGroupLabel("Misc");
            //Variables.Config.Add("misc.qImmobile", new CheckBox("Auto Q to Immobile Target"));
            //Variables.Config.Add("misc.saveSupport", new CheckBox("Save Support [R]"));
            //Variables.Config.Add("misc.savePercent", new Slider("Save Support Health Percent", 10, 0, 100));
            //Variables.Config.AddGroupLabel("Draw");
            //Variables.Config.Add("draw.q", new CheckBox("Vẽ Q"));
            //Variables.Config.Add("draw.w", new CheckBox("Vẽ W"));
            //Variables.Config.Add("draw.e", new CheckBox("Vẽ E"));
            //Variables.Config.Add("draw.r", new CheckBox("Vẽ R"));
            //Variables.Config.Add("draw.ePercent", new CheckBox("E % On Enemy"));
            //Variables.Config.Add("draw.signal", new CheckBox("Support Signal"));
            //EN
            Variables.Config.AddGroupLabel("Combo");
            Variables.Config.Add("commbo.q", new CheckBox("Use Q"));
            Variables.Config.Add("commbo.e", new CheckBox("Use E"));
            Variables.Config.Add("commbo.r", new CheckBox("Use R"));
            Variables.Config.Add("commbo.rCount", new Slider("R on x Enemy", 3, 0, 10));
            Variables.Config.AddGroupLabel("KillSteal");
            Variables.Config.Add("killsteal.q", new CheckBox("Use Q"));
            Variables.Config.Add("killsteal.e", new CheckBox("Use E"));
            Variables.Config.AddGroupLabel("LaneClear");
            Variables.Config.Add("laneclear.e", new CheckBox("Use E"));
            Variables.Config.Add("LaneClear.eClearCount", new Slider("If Can Kill Minion >= ", 2, 1, 5));
            Variables.Config.Add("LaneClear.mana", new Slider("LaneClear Mana Manager ", 20, 0, 100));
            Variables.Config.AddGroupLabel("Harass");
            Variables.Config.Add("harass.q", new CheckBox("Use Q", true));
            Variables.Config.Add("harass.e", new CheckBox("Use E"));
            Variables.Config.Add("harass.eSpearCount", new Slider("If Enemy Spear Count >= ", 3, 0, 10));
            Variables.Config.Add("harass.mana", new Slider("Harass Mana Manager ", 20, 0, 100));
            Variables.Config.AddGroupLabel("Misc");
            Variables.Config.Add("misc.qImmobile", new CheckBox("Auto Q to Immobile Target"));
            Variables.Config.Add("misc.saveSupport", new CheckBox("Save Support [R]"));
            Variables.Config.Add("misc.savePercent", new Slider("Save Support Health Percent", 10, 0, 100));
            Variables.Config.AddGroupLabel("Draw");
            Variables.Config.Add("draw.q", new CheckBox("Draw Q"));
            Variables.Config.Add("draw.w", new CheckBox("Draw W"));
            Variables.Config.Add("draw.e", new CheckBox("Draw E"));
            Variables.Config.Add("draw.r", new CheckBox("Draw R"));
            Variables.Config.Add("draw.ePercent", new CheckBox("E % On Enemy"));
            Variables.Config.Add("draw.signal", new CheckBox("Support Signal"));
        }

        private static void OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe)
            {
                // E - Rend
                if (args.SData.Name == "KalistaExpungeWrapper")
                {
                    // Make the orbwalker attack again, might get stuck after casting E
                    Core.DelayAction(Orbwalker.ResetAutoAttack, 250);
                }
            }
        }

        public static float QDamage(Obj_AI_Base target)
        {
            return Program._Player.CalculateDamageOnUnit(target, DamageType.Physical,
                (float)(new[] { 10, 70, 130, 190, 250 }[_Q.Level - 1] + 1 * Program._Player.FlatPhysicalDamageMod));
        }

        public static float GetTotalDamage(Obj_AI_Base target)
        {
            var damage = 0f;

            if (_E.IsReady())
            {
                damage += CustomCalculator(target);
            }
            var stacz = CustomCalculator(target);
            float edamagedraw = stacz * 100 / target.Health;
            int edraw = (int)Math.Ceiling(edamagedraw);
            if (edraw >= 100)
            {
                var yx = Drawing.WorldToScreen(target.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.Yellow, "STACK OVERLOAD - FUCK THEM ALL !");
            }
            if (edraw < 100)
            {
                var yx = Drawing.WorldToScreen(target.Position);
                Drawing.DrawText(yx[0], yx[1], System.Drawing.Color.Yellow, "E Stack on Enemy HP %" + edraw);
            }
            return (float)damage;
        }

        public static float CustomCalculator(Obj_AI_Base target, int customStacks = -1)
        {
            int buff = target.GetBuffCount("KalistaExpungeMarker");

            if (buff > 0 || customStacks > -1)
            {
                var tDamage = (RRD[_E.Level - 1] + RRDM[_E.Level - 1] * Program._Player.TotalAttackDamage) +
                       ((customStacks < 0 ? buff : customStacks) - 1) *
                       (RRPS[_E.Level - 1] + RRPSM[_E.Level - 1] * Program._Player.TotalAttackDamage);

                return (float)Program._Player.CalculateDamageOnUnit(target, DamageType.Physical, tDamage);
            }

            return 0;
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
                Clear();
                Jungle();
            }
            immobileQ();
            KillSteal();
        }

        private void Jungle()
        {
            //code here
        }

        private void Clear()
        {
            var manaSlider = Variables.Config["harass.mana"].Cast<Slider>().CurrentValue;
            var eClearCount = Variables.Config["harass.eClearCount"].Cast<Slider>().CurrentValue;
            var useE = Variables.Config["laneclear.e"].Cast<CheckBox>().CurrentValue;
            var mns = EntityManager.GetLaneMinions(EntityManager.UnitTeam.Enemy, Program._Player.ServerPosition.To2D(), _E.Range);
            if (mns.Count == 0)
            {
                return;
            }
            if (Program._Player.ManaPercent >= manaSlider)
            {
                if (_E.IsReady() && useE)
                {
                    var mir = mns.Where(m => _E.IsInRange(m)).ToArray();
                    var killableNum = 0;
                    foreach (var m in mir)
                    {
                        if (m.Health < GetTotalDamage(m))
                        {
                            // Increase kill number
                            killableNum++;

                            // Cast on condition met
                            if (killableNum >= eClearCount)
                            {
                                _E.Cast();
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Harass()
        {
            var manaSlider = Variables.Config["harass.mana"].Cast<Slider>().CurrentValue;
            var eSpearCount = Variables.Config["harass.eSpearCount"].Cast<Slider>().CurrentValue;
            var useQ = Variables.Config["harass.q"].Cast<CheckBox>().CurrentValue;
            var useE = Variables.Config["harass.e"].Cast<CheckBox>().CurrentValue;

            if (Program._Player.ManaPercent >= manaSlider)
            {
                if (_Q.IsReady() && useQ)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(_Q.Range)))
                    {
                        _Q.Cast(enemy);
                    }
                }
                if (_E.IsReady() && useE)
                {
                    foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(_E.Range)))
                    {
                        int enemyStack = enemy.Buffs.FirstOrDefault(x => x.DisplayName == "KalistaExpungeMarker").Count;
                        if (enemyStack > eSpearCount)
                        {
                            _E.Cast();
                        }
                    }
                }
            }
        }

        private void Combo()
        {
            var useQ = Variables.Config["commbo.q"].Cast<CheckBox>().CurrentValue;
            var useE = Variables.Config["commbo.e"].Cast<CheckBox>().CurrentValue;
            var useR = Variables.Config["commbo.r"].Cast<CheckBox>().CurrentValue;
            var supportPercent = Variables.Config["misc.savePercent"].Cast<Slider>().CurrentValue;
            var rCount = Variables.Config["commbo.rCount"].Cast<Slider>().CurrentValue;
            var rAbleTarget = TargetSelector2.GetTarget(_E.Range, DamageType.Physical);
            float rEnemyCount = Program._Player.CountEnemiesInRange(_R.Range);

            var QTarget = TargetSelector2.GetTarget(_Q.Range, DamageType.Physical);

            if (_Q.IsReady() && useQ && QTarget.IsValidTarget())
            {
                _Q.Cast(QTarget);
            }
            if (_E.IsReady() && useE)
            {
                foreach (var enemy in HeroManager.Enemies.Where(hero => hero.IsValidTarget(_E.Range) &&
                    !undyBuff(hero) && !hero.HasBuffOfType(BuffType.SpellShield)))
                {
                    if (enemy.Health < GetTotalDamage(enemy))
                    {
                        _E.Cast();
                    }
                }
            }
            if (_R.IsReady() && useR)
            {
                foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValidTarget(_R.Range) && ally.HasBuff("kalistacoopstrikeally")))
                {
                    if (ally.HealthPercent < supportPercent) // Save Support
                    {
                        _R.Cast();
                    }

                    if (rAbleTarget.Distance(Program._Player.Position) > _E.Range && rAbleTarget.Distance(Program._Player.Position) < _R.Range
                        && ally.HealthPercent > supportPercent && rEnemyCount <= 2 && nontankySupport.Contains(ally.ChampionName)) // Non-Tanky Champs
                    {
                        _R.Cast();
                    }

                    if (rAbleTarget.Distance(Program._Player.Position) > _E.Range && rAbleTarget.Distance(Program._Player.Position) < _R.Range
                        && rEnemyCount <= 2 && Marksman.Contains(ally.ChampionName)
                        && ally.HealthPercent > supportPercent) // Adc Focus R
                    {
                        _R.Cast();
                    }

                    if (rAbleTarget.Distance(Program._Player.Position) > _E.Range && rAbleTarget.Distance(Program._Player.Position) < _R.Range
                        && rEnemyCount >= rCount && tankySupport.Contains(ally.ChampionName) // Tanky vs Everyone
                        && ally.HealthPercent > supportPercent)
                    {
                        _R.Cast();
                    }
                }
            }
        }

        public static void immobileQ()
        {
            var useQ = Variables.Config["misc.qImmobile"].Cast<CheckBox>().CurrentValue;
            if (_Q.IsReady() && useQ)
            {
                var heroes = HeroManager.Enemies.Where(t => t.IsFeared || t.IsCharmed || t.IsTaunted || t.IsRecalling);
                if (heroes != null)
                {
                    foreach (var enemy in heroes)
                    {
                        var pred = _Q.GetPrediction(enemy);
                        _Q.Cast(pred.CastPosition);
                    }
                }
            }
        }

        private static void KillSteal()
        {
            var qKS = Variables.Config["killsteal.q"].Cast<CheckBox>().CurrentValue;
            var eKS = Variables.Config["killsteal.e"].Cast<CheckBox>().CurrentValue;
            if (_Q.IsReady() && qKS)
            {
                foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
                {
                    if (target.Distance(Program._Player.Position) < _Q.Range
                        && QDamage(target) < target.Health)
                    {
                        _Q.Cast(target);
                    }
                }
            }
            if (_E.IsReady() && eKS)
            {
                foreach (var target in HeroManager.Enemies.Where(hero => hero.IsValidTarget(_E.Range)))
                {
                    if (GetTotalDamage(target) > target.Health)
                    {
                        _E.Cast();
                    }
                }
            }
        }

        public static bool undyBuff(AIHeroClient target)
        {
            if (target.ChampionName == "Tryndamere" &&
                target.Buffs.Any(b => b.Caster.NetworkId == target.NetworkId && b.IsValid && b.DisplayName == "Undying Rage"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "Chrono Shift"))
            {
                return true;
            }
            if (target.Buffs.Any(b => b.IsValid() && b.DisplayName == "JudicatorIntervention"))
            {
                return true;
            }
            if (target.ChampionName == "Poppy")
            {
                if (HeroManager.Allies.Any(o =>
                    !o.IsMe &&
                    o.Buffs.Any(b => b.Caster.NetworkId == target.NetworkId && b.IsValid() && b.DisplayName == "PoppyDITarget")))
                {
                    return true;
                }
            }
            return false;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            if (!Program._Player.IsDead)
            {
                if (Variables.Config["draw.q"].Cast<CheckBox>().CurrentValue && _Q.IsReady() && _Q.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _Q.Range }.Draw(Player.Instance.Position);
                }
                if (Variables.Config["draw.w"].Cast<CheckBox>().CurrentValue && _W.IsReady() && _W.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _W.Range }.Draw(Player.Instance.Position);
                }
                if (Variables.Config["draw.e"].Cast<CheckBox>().CurrentValue && _W.IsReady() && _E.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _E.Range }.Draw(Player.Instance.Position);
                }
                if (Variables.Config["draw.r"].Cast<CheckBox>().CurrentValue && _R.IsReady() && _R.Level >= 1)
                {
                    new Circle { Color = System.Drawing.Color.Red, BorderWidth = 1, Radius = _R.Range }.Draw(Player.Instance.Position);
                }
            }
        }
    }
}
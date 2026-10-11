using System.Collections.Generic;
using UnityEngine;

namespace NeonRelay
{
    public sealed partial class NeonRelayGame
    {
        struct Spark { public Vector2 Position, Velocity; public float Life, Duration; public Color Color; }
        readonly List<Spark> particles = new List<Spark>(128);
        readonly float[] sectorTimes = new float[LevelCatalog.Count];
        readonly int[] sectorRetries = new int[LevelCatalog.Count];
        float stageRestartTime, visualTime;
        int stageRestartDeaths;
        bool reducedMotion;
        ScreenState helpReturn;

        void UpdatePresentation()
        {
            if (screen != ScreenState.Playing && screen != ScreenState.Title && screen != ScreenState.Finished) return;
            float dt = Mathf.Min(UnityEngine.Time.unscaledDeltaTime, .05f);
            if (!reducedMotion) visualTime += dt;
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i]; p.Life -= dt;
                if (p.Life <= 0) { particles.RemoveAt(i); continue; }
                p.Position += p.Velocity * dt; p.Velocity.y -= 5 * dt; particles[i] = p;
            }
        }

        void Burst(Vector2 origin, Color color, int count)
        {
            if (reducedMotion) return;
            for (int i = 0; i < count && particles.Count < 128; i++)
            {
                float angle = i * 2.39996f;
                float speed = 1.1f + i % 4 * .55f;
                particles.Add(new Spark { Position = origin, Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) + .8f) * speed,
                    Life = .55f + i % 3 * .12f, Duration = .79f, Color = color });
            }
        }

        void DrawParticles()
        {
            foreach (var p in particles)
            {
                Vector2 at = Point(p.Position); Color c = p.Color; c.a = p.Life / p.Duration;
                float size = 2 + 3 * c.a; Box(at.x - size / 2, at.y - size / 2, size, size, c);
            }
        }

        void DrawSectorScenery()
        {
            // Decorative circuitry occupies the sky, leaving all collision surfaces legible.
            Color dim = new Color(Accent.r, Accent.g, Accent.b, .15f);
            float pulse = .75f + Mathf.Sin(visualTime * 1.3f) * .2f;
            for (int i = 0; i < 3; i++)
            {
                float x = 830 + i * 110;
                Box(x, 250 - i * 28, 2, 126 + i * 28, dim);
                Box(x, 250 - i * 28, 66, 2, dim);
                Blob(x + 61, 246 - i * 28, 10, dim);
                if (i <= stage) Blob(x - 3, 315 + Mathf.Sin(visualTime + i) * 20, 8, new Color(Accent.r, Accent.g, Accent.b, .35f * pulse));
            }
            if (stage == 0)
            {
                // Rooftop antenna field.
                for (int i = 0; i < 4; i++)
                {
                    float x = 70 + i * 300;
                    Box(x, 540, 3, 70, dim); Box(x - 17, 549, 37, 2, dim);
                    Blob(x - 2, 535, 7, dim);
                }
            }
            else if (stage == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    float x = 80 + i * 250;
                    Box(x, 527, 12, 85, dim); Box(x, 527, 78, 7, dim); Box(x + 70, 490, 8, 44, dim);
                    for (int j = 0; j < 3; j++) Box(x - 3, 554 + j * 13, 18, 3, dim);
                }
            }
            else
            {
                Blob(900, 165, 145, dim); Blob(918, 183, 109, ink);
                Blob(945, 210, 55, new Color(Accent.r, Accent.g, Accent.b, .18f * pulse));
                Text("CENTRAL REACTOR", 870, 323, 210, 22, 11, mutedText, TextAnchor.MiddleCenter);
            }
            for (int i = 0; i < 3; i++)
            {
                float x = 66 + i * 205;
                Color c = i < stage || screen == ScreenState.Finished ? Accent : new Color(.19f, .27f, .35f);
                Box(x, 302, 181, 3, c);
                Text("0" + (i + 1) + "  " + new[] { "ROOFTOPS", "PULSE WORKS", "THE CORE" }[i], x, 314, 185, 22, 11, c);
            }
            // The city's windows light up as each sector is restored.
            for (int i = 0; i < 15; i++)
            {
                if (i % 3 >= stage && screen != ScreenState.Finished) continue;
                float h = 55 + Mathf.Sin(i * 7.3f) * 24 + (i % 3) * 21;
                for (int j = 0; j < 3; j++) Box(i * 92 + 5 + j * 18, 643 - h, 5, 5, Accent);
            }
        }

        void DrawObjective()
        {
            string task = "REACH THE FINISH  >  NEXT STAGE  /  BONUS CELLS OPTIONAL";
            Text(task, 63, 352, 670, 24, 13, Accent, TextAnchor.UpperLeft, true);
            if (stage == 0 && sim.Position.x < 8)
                Pill("SPACE  /  JUMP AGAIN IN THE AIR", 68, 390, 284, Accent);
            if (stage > 0 && sim.Position.x > 19 && sim.Position.x < 27)
                Pill("SAVE YOUR SECOND JUMP FOR THE GAP", 662, 386, 320, Accent);
        }

        void OpenHelp()
        {
            helpReturn = screen == ScreenState.Playing ? ScreenState.Paused : screen;
            SetScreen(ScreenState.Help);
        }

        void ToggleFullscreen()
        {
            Screen.fullScreenMode = Screen.fullScreen ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow;
        }

        internal void ToggleMotion()
        {
            reducedMotion = !reducedMotion; particles.Clear(); flash = 0;
            PlayerPrefs.SetInt("NeonRelay.ReducedMotion", reducedMotion ? 1 : 0); PlayerPrefs.Save();
        }

        void DrawHelp()
        {
            Box(214, 133, 852, 514, panel); Box(214, 133, 852, 3, Accent);
            Text("FIELD MANUAL", 250, 157, 780, 40, 30, white, TextAnchor.MiddleCenter, true);
            Text("AUTO-RUN  >  JUMP THE OBSTACLES  >  FINISH TO ADVANCE", 250, 208, 780, 25, 14, Accent, TextAnchor.MiddleCenter, true);
            string[] actions = { "AUTO-RUN", "DOUBLE JUMP", "STAGE FINISH", "PAUSE / RESTART", "DISPLAY / HELP" };
            string[] keys = { "Forward movement is automatic. Focus on jump timing.", "Space / W / Up / click. Release early for a short hop.", "Cross the finish. The next stage loads automatically.", "Esc pauses. R resets the sector. Time still counts.", "F11 fullscreen. H opens this manual. M toggles sound." };
            for (int i = 0; i < actions.Length; i++)
            {
                float y = 259 + i * 38;
                Text(actions[i], 260, y, 185, 26, 12, Accent, TextAnchor.MiddleLeft, true);
                Text(keys[i], 446, y, 575, 26, 14, white, TextAnchor.MiddleLeft);
            }
            Text("Pass the checkpoint flag to save your position. Unlimited retries.\nBonus cells are optional. Tap twice for long gaps; hold for a higher jump.", 260, 460, 760, 52, 14, mutedText, TextAnchor.MiddleLeft);
            if (Button(muted ? "SOUND: OFF" : "SOUND: ON", 260, 537, 233, 40, false, 12)) ToggleMute();
            if (Button(reducedMotion ? "EFFECTS: REDUCED" : "EFFECTS: FULL", 507, 537, 233, 40, false, 12)) ToggleMotion();
            if (Button(Screen.fullScreen ? "DISPLAY: FULLSCREEN" : "DISPLAY: WINDOWED", 754, 537, 265, 40, false, 12)) ToggleFullscreen();
            if (Button("BACK  /  ESC", 454, 594, 372, 34, true, 13)) SetScreen(helpReturn);
        }

        void DrawResults()
        {
            Box(251, 139, 778, 505, panel); Box(251, 139, 778, 3, Accent);
            Text("TRANSMISSION COMPLETE  /  ALL SYSTEMS ONLINE", 280, 163, 720, 24, 12, Accent, TextAnchor.MiddleCenter, true);
            Text("YOU LIT THE CITY.", 280, 201, 720, 61, 42, white, TextAnchor.MiddleCenter, true);
            Text("3 stages cleared. " + (bankCells+sim.CellCount) + " / 9 bonus cells recovered.", 280, 270, 720, 27, 17, mutedText, TextAnchor.MiddleCenter);
            Text("TOTAL  " + Clock(TotalTime) + "     /     RETRIES  " + TotalDeaths, 280, 315, 720, 32, 23, Accent, TextAnchor.MiddleCenter, true);
            for (int i = 0; i < LevelCatalog.Count; i++)
            {
                float y = 366 + i * 39;
                Box(305, y, 670, 32, new Color(.09f, .13f, .2f));
                Text("0" + (i + 1) + "  " + LevelCatalog.Create(i).Name, 321, y, 320, 32, 13, white, TextAnchor.MiddleLeft);
                Text(Clock(sectorTimes[i]) + "   /   " + sectorRetries[i] + " retries", 640, y, 316, 32, 13, mutedText, TextAnchor.MiddleRight);
            }
            Text("PERSONAL BEST  " + Clock(PlayerPrefs.GetFloat("NeonRelay.RunnerBest", TotalTime)), 300, 491, 680, 25, 13, Accent, TextAnchor.MiddleCenter);
            if (Button("ANOTHER RUN  /  ENTER", 305, 536, 427, 48, true, 15)) StartRun();
            if (Button("TITLE SCREEN", 751, 536, 224, 48, false, 14)) SetScreen(ScreenState.Title);
            Text("NEON RELAY  /  Design, art and sound generated in Unity  /  v1.2", 290, 606, 700, 20, 11, mutedText, TextAnchor.MiddleCenter);
        }
    }
}

using UnityEngine;

namespace LanternGarden
{
    public sealed partial class GardenGame
    {
        readonly Color ink = new Color(.08f, .17f, .18f, .96f);
        readonly Color paper = new Color(.96f, .96f, .85f);
        readonly Color mint = new Color(.67f, .91f, .7f);
        readonly Color gold = new Color(1, .78f, .35f);
        GUIStyle label, button;

        void OnGUI()
        {
            if (label == null)
            {
                Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                label = new GUIStyle(GUI.skin.label) { font = font, wordWrap = true };
                button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 18, fontStyle = FontStyle.Bold };
                button.normal.background = Texture2D.whiteTexture; button.hover.background = Texture2D.whiteTexture;
                button.active.background = Texture2D.whiteTexture;
                button.normal.textColor = button.hover.textColor = button.active.textColor = ink;
            }
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            Matrix4x4 original = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280 * scale) / 2, (Screen.height - 720 * scale) / 2), Quaternion.identity, Vector3.one * scale);
            if (State == GardenState.Title) DrawTitle();
            else
            {
                DrawHud();
                if (State == GardenState.Paused) DrawPause();
                if (State == GardenState.Won) DrawWin();
            }
            if (State == GardenState.Help) DrawHelp();
            GUI.matrix = original;
        }

        void DrawTitle()
        {
            Panel(58, 60, 454, 598);
            Text("A LITTLE ADVENTURE AT DUSK", 90, 85, 392, 28, 13, mint, true);
            Text("LANTERN\nGARDEN", 86, 122, 398, 150, 58, paper, true);
            Fill(new Rect(92, 295, 60, 4), gold);
            Text("Bring a quiet garden back to light.", 90, 320, 382, 56, 24, paper);
            Text("Find eight sun crystals. Explore at your own pace.\nThen walk through the glowing arch.", 90, 385, 374, 75, 17, new Color(.8f, .86f, .8f));
            if (Button("EXPLORE THE GARDEN  /  ENTER", 90, 492, 390, 52)) StartGame();
            if (Button("HOW TO PLAY", 90, 557, 250, 40, false)) OpenHelp();
            if (Button("QUIT", 352, 557, 128, 40, false)) Application.Quit();
            Text("No enemies. No time limit. Just discovery.", 90, 614, 390, 24, 13, mint);
            Panel(842, 595, 376, 63);
            Text("W A S D / ARROWS   move     SPACE   jump", 858, 610, 350, 36, 16, paper);
        }

        void DrawHud()
        {
            Panel(28, 26, 356, 112);
            Text("LANTERN GARDEN", 48, 39, 320, 24, 13, mint, true);
            Text(Collected + " / " + Total + " sun crystals", 48, 67, 310, 34, 26, paper, true);
            for (int i = 0; i < Total; i++) Fill(new Rect(49 + i * 38, 112, 28, 5), i < Collected ? gold : new Color(.27f, .37f, .35f));
            Panel(400, 26, 528, 64);
            Text(Collected == Total ? "THE ARCH IS OPEN  /  follow the green marker" : "EXPLORE  /  follow the golden crystals", 420, 44, 490, 36, 17, paper, true);
            DrawMap();
            Panel(28, 650, 1224, 44);
            Text("WASD / arrows   move       SPACE   jump       ESC   pause       H   help       M   sound       F11   fullscreen", 48, 660, 1180, 25, 15, paper);
            if (NoticeTime > 0 && IsPlaying)
            {
                Panel(270, 582, 740, 50);
                Text(Notice, 285, 592, 710, 34, 17, gold, false, TextAnchor.MiddleCenter);
            }
        }

        void DrawMap()
        {
            Panel(1052, 26, 200, 228);
            Text("GARDEN MAP", 1070, 39, 165, 22, 12, mint, true);
            Rect map = new Rect(1073, 70, 158, 158);
            Fill(map, new Color(.18f, .3f, .28f));
            foreach (var crystal in crystals)
                if (!crystal.Collected) Dot(MapPoint(crystal.transform.position, map), gold, 7);
            Dot(MapPoint(gate.transform.position, map), gate.IsOpen ? mint : new Color(.65f, .73f, .68f), 11);
            Dot(MapPoint(player.transform.position, map), Color.white, 9);
            Text("YOU: WHITE   /   CRYSTALS: GOLD", 1065, 230, 177, 16, 9, paper);
        }
        static Vector2 MapPoint(Vector3 point, Rect rect) { return new Vector2(rect.x + Mathf.InverseLerp(-17, 17, point.x) * rect.width, rect.yMax - Mathf.InverseLerp(-17, 17, point.z) * rect.height); }
        void Dot(Vector2 point, Color color, float size) { Fill(new Rect(point.x - size / 2, point.y - size / 2, size, size), color); }

        void DrawPause()
        {
            Dim(); Panel(390, 148, 500, 415);
            Text("TAKE A BREATHER", 430, 182, 420, 54, 31, paper, true);
            Text("Your crystals are safe. Continue when ready.", 430, 243, 420, 45, 18, mint);
            if (Button("CONTINUE  /  ESC", 430, 312, 420, 48)) TogglePause();
            if (Button("HELP & OPTIONS", 430, 375, 420, 44, false)) OpenHelp();
            if (Button("START A NEW WALK", 430, 433, 420, 44, false)) StartGame();
            if (Button("QUIT GAME", 430, 491, 420, 36, false)) Application.Quit();
        }

        void DrawHelp()
        {
            Dim(); Panel(250, 75, 780, 570);
            Text("A FEW SIMPLE THINGS", 294, 104, 690, 46, 32, paper, true);
            Text("WASD or arrow keys", 294, 178, 260, 30, 19, gold, true);
            Text("Move in the direction you see on screen.", 572, 178, 410, 45, 18, paper);
            Text("Space", 294, 230, 250, 28, 19, gold, true);
            Text("Jump onto low objects. Stairs work by walking.", 572, 230, 410, 46, 18, paper);
            Text("Collect & explore", 294, 287, 250, 30, 19, gold, true);
            Text("Walk into all 8 crystals, then visit the arch.\nGold map dots show the crystals you still need.", 572, 283, 410, 66, 18, paper);
            Text("Take your time", 294, 365, 250, 30, 19, gold, true);
            Text("No enemies or countdown. If you fall, you\nreturn to the path and keep your crystals.", 572, 359, 410, 60, 18, paper);
            if (Button(Muted ? "SOUND: OFF" : "SOUND: ON", 294, 454, 208, 42, false)) ToggleSound();
            if (Button(Shadows ? "SHADOWS: ON" : "SHADOWS: OFF", 518, 454, 224, 42, false)) ToggleShadows();
            if (Button(Screen.fullScreen ? "WINDOWED" : "FULLSCREEN", 758, 454, 224, 42, false)) ToggleFullscreen();
            Text("A new walk resets progress. Closing the game does not save your current walk.", 294, 513, 690, 34, 14, mint);
            if (Button("BACK  /  ESC", 454, 564, 372, 46)) CloseHelp();
        }

        void DrawWin()
        {
            Dim(); Panel(350, 120, 580, 470);
            Text("YOU BROUGHT THE LIGHT BACK", 390, 153, 500, 30, 15, mint, true, TextAnchor.MiddleCenter);
            Text("Garden restored.", 390, 212, 500, 66, 45, paper, true, TextAnchor.MiddleCenter);
            Text(Total + " SUN CRYSTALS  /  ONE LITTLE ADVENTURE", 390, 301, 500, 32, 15, gold, true, TextAnchor.MiddleCenter);
            Text("Every path explored brings a little more light.\nThanks for taking a walk in Lantern Garden.", 390, 358, 500, 72, 20, paper, false, TextAnchor.MiddleCenter);
            if (Button("EXPLORE AGAIN  /  ENTER", 400, 469, 480, 48)) StartGame();
            if (Button("QUIT", 400, 535, 480, 30, false)) Application.Quit();
        }
        void Dim() { Fill(new Rect(0, 0, 1280, 720), new Color(.025f, .08f, .09f, .6f)); }
        void Panel(float x, float y, float w, float h) { Fill(new Rect(x, y, w, h), ink); Fill(new Rect(x, y, 3, h), new Color(.5f, .69f, .53f, .8f)); }
        void Fill(Rect area, Color color) { GUI.color = color; GUI.DrawTexture(area, Texture2D.whiteTexture); GUI.color = Color.white; }
        void Text(string value, float x, float y, float w, float h, int size, Color color, bool bold = false, TextAnchor alignment = TextAnchor.UpperLeft)
        {
            label.fontSize = size; label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; label.normal.textColor = color; label.alignment = alignment;
            GUI.Label(new Rect(x, y, w, h), value, label);
        }
        bool Button(string text, float x, float y, float w, float h, bool primary = true)
        {
            GUI.backgroundColor = primary ? mint : new Color(.8f, .86f, .76f);
            bool result = GUI.Button(new Rect(x, y, w, h), text, button); GUI.backgroundColor = Color.white; return result;
        }
    }
}

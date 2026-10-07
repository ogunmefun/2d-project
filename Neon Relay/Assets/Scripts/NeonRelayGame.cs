using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NeonRelay
{
    public sealed class NeonRelayGame : MonoBehaviour
    {
        enum ScreenState { Title, Playing, Paused, Circuit, Clear, Finished }
        GameSimulation sim;
        ScreenState screen;
        int stage, bankDeaths;
        float accumulator, bankTime, flash, noticeTime;
        string notice = "", circuitMessage = "";
        bool pendingJump, muted;
        Texture2D disc, triangle, diamond;
        AudioSource speaker;
        AudioClip jumpClip, cellClip, deathClip, clearClip, buttonClip;
        GUIStyle label;
        readonly Color ink = new Color(.035f, .055f, .10f);
        readonly Color panel = new Color(.064f, .09f, .15f);
        readonly Color mutedText = new Color(.53f, .63f, .74f);
        readonly Color white = new Color(.9f, .95f, 1);
        readonly Color coral = new Color(1, .34f, .43f);
        Color Accent => sim.Level.Accent;
        float TotalTime => bankTime + sim.Time;
        int TotalDeaths => bankDeaths + sim.Deaths;
        public GameSimulation Simulation => sim;
        public string CurrentScreen => screen.ToString();

        void Awake()
        {
            Application.targetFrameRate = 120;
            sim = new GameSimulation(0);
            screen = ScreenState.Title;
            muted = PlayerPrefs.GetInt("NeonRelay.Muted", 0) == 1;
            disc = Shape((x, y) => x*x+y*y <= 1);
            triangle = Shape((x, y) => y >= -1 && y <= 1 && Mathf.Abs(x) <= (1-y)*.5f);
            diamond = Shape((x, y) => Mathf.Abs(x) + Mathf.Abs(y) <= 1);
            speaker = gameObject.AddComponent<AudioSource>();
            speaker.playOnAwake = false;
            jumpClip = Tone(420, 720, .09f);
            cellClip = Tone(780, 1260, .15f);
            deathClip = Tone(180, 45, .22f);
            clearClip = Tone(520, 1040, .34f);
            buttonClip = Tone(520, 620, .06f);
        }

        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            flash = Mathf.Max(0, flash - UnityEngine.Time.unscaledDeltaTime * 3);
            noticeTime = Mathf.Max(0, noticeTime - UnityEngine.Time.unscaledDeltaTime);
            if (k.mKey.wasPressedThisFrame) ToggleMute();
            if (k.escapeKey.wasPressedThisFrame)
            {
                if (screen == ScreenState.Playing) SetScreen(ScreenState.Paused);
                else if (screen == ScreenState.Paused || screen == ScreenState.Circuit) SetScreen(ScreenState.Playing);
            }
            if (screen == ScreenState.Title) { if (k.enterKey.wasPressedThisFrame) StartRun(); return; }
            if (screen == ScreenState.Clear) { if (k.enterKey.wasPressedThisFrame) NextStage(); return; }
            if (screen == ScreenState.Finished) { if (k.enterKey.wasPressedThisFrame) StartRun(); return; }
            if (screen == ScreenState.Paused) return;
            if (screen == ScreenState.Circuit)
            {
                if (k.digit1Key.wasPressedThisFrame) Toggle(0);
                if (k.digit2Key.wasPressedThisFrame) Toggle(1);
                if (k.digit3Key.wasPressedThisFrame) Toggle(2);
                if (k.enterKey.wasPressedThisFrame) Submit();
                return;
            }
            if (k.rKey.wasPressedThisFrame) { RestartStage(); return; }
            if (k.eKey.wasPressedThisFrame && sim.CanUseTerminal && !sim.RelayPowered) { circuitMessage = ""; SetScreen(ScreenState.Circuit); return; }
            float move = (k.dKey.isPressed || k.rightArrowKey.isPressed ? 1 : 0) - (k.aKey.isPressed || k.leftArrowKey.isPressed ? 1 : 0);
            pendingJump |= k.spaceKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame;
            bool held = k.spaceKey.isPressed || k.wKey.isPressed || k.upArrowKey.isPressed;
            accumulator += Mathf.Min(UnityEngine.Time.unscaledDeltaTime, .1f);
            while (accumulator >= GameSimulation.Step && screen == ScreenState.Playing)
            {
                int deaths = sim.Deaths, cells = sim.CellCount, jumps = sim.JumpEvents;
                bool checkpoint = sim.CheckpointReached;
                sim.Tick(move, pendingJump, held);
                pendingJump = false;
                accumulator -= GameSimulation.Step;
                if (sim.JumpEvents != jumps) Play(jumpClip, .18f);
                if (sim.CellCount != cells) { Play(cellClip, .3f); Notice("ENERGY CELL RECOVERED"); }
                if (!checkpoint && sim.CheckpointReached) { Play(cellClip, .3f); Notice("CHECKPOINT ONLINE  /  progress saved"); }
                if (sim.Deaths != deaths) { Play(deathClip, .3f); flash = 1; Notice("Back on your feet. Cells and relay progress kept."); }
                if (sim.Won)
                {
                    Play(clearClip, .35f);
                    SetScreen(stage == LevelCatalog.Count - 1 ? ScreenState.Finished : ScreenState.Clear);
                    if (screen == ScreenState.Finished)
                    {
                        float best = PlayerPrefs.GetFloat("NeonRelay.Best", float.MaxValue);
                        if (TotalTime < best) PlayerPrefs.SetFloat("NeonRelay.Best", TotalTime);
                        PlayerPrefs.Save();
                    }
                }
            }
        }

        void OnApplicationFocus(bool focused) { if (!focused && screen == ScreenState.Playing) SetScreen(ScreenState.Paused); }
        void SetScreen(ScreenState value) { screen = value; accumulator = 0; pendingJump = false; }
        void StartRun() { stage = bankDeaths = 0; bankTime = 0; sim = new GameSimulation(0); SetScreen(ScreenState.Playing); Notice(sim.Level.Brief, 6); }
        void NextStage() { bankTime += sim.Time; bankDeaths += sim.Deaths; stage++; sim = new GameSimulation(stage); SetScreen(ScreenState.Playing); Notice(sim.Level.Brief, 6); }
        void RestartStage() { bankTime += sim.Time; bankDeaths += sim.Deaths; sim = new GameSimulation(stage); SetScreen(ScreenState.Playing); Notice("Stage restarted."); }
        void Notice(string value, float duration = 2.5f) { notice = value; noticeTime = duration; }
        void Toggle(int index) { sim.ToggleSwitch(index); circuitMessage = ""; Play(buttonClip, .2f); }
        void Submit()
        {
            if (sim.SubmitRelay()) { Play(clearClip, .3f); SetScreen(ScreenState.Playing); Notice(sim.CellCount == 3 ? "RELAY RESTORED  /  the exit is open" : "RELAY RESTORED  /  recover the remaining cells"); }
            else { circuitMessage = "That routing does not match the clue. Try another combination."; Play(deathClip, .12f); }
        }
        void ToggleMute() { muted = !muted; PlayerPrefs.SetInt("NeonRelay.Muted", muted ? 1 : 0); PlayerPrefs.Save(); }
        void Play(AudioClip clip, float volume) { if (!muted) speaker.PlayOneShot(clip, volume); }

        void OnGUI()
        {
            if (sim == null) return;
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280*scale)/2, (Screen.height - 720*scale)/2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            label = new GUIStyle(GUI.skin.label) { fontSize = 18, richText = false, padding = new RectOffset(0, 0, 0, 0), wordWrap = false };
            DrawBackground(); DrawWorld(); DrawHUD();
            if (screen == ScreenState.Playing)
            {
                if (noticeTime > 0) { Box(270, 116, 740, 36, new Color(.055f, .09f, .14f, .96f)); Text(notice, 280, 123, 720, 24, 15, Accent, TextAnchor.MiddleCenter); }
                if (sim.CanUseTerminal && !sim.RelayPowered) Pill("E  /  CONNECT RELAY", 915, 493, 270, Accent);
                if (Vector2.Distance(sim.Position, sim.Level.Exit) < 1.8f && !(sim.CellCount == 3 && sim.RelayPowered))
                    Pill(sim.CellCount < 3 ? "RECOVER ALL 3 CELLS" : "RESTORE THE RELAY", 930, 451, 270, coral);
            }
            if (flash > 0) Box(0, 0, 1280, 720, new Color(coral.r, coral.g, coral.b, flash * .12f));
            if (screen != ScreenState.Playing) DrawOverlay();
            GUI.matrix = Matrix4x4.identity;
        }

        void DrawBackground()
        {
            Box(0, 0, 1280, 720, ink);
            for (int i = 0; i < 32; i++) Box(40 + i*37.5f, 110, 1, 526, new Color(.11f, .17f, .24f, .35f));
            for (int i = 0; i < 14; i++) Box(40, 110+i*37.5f, 1200, 1, new Color(.11f, .17f, .24f, .35f));
            Blob(947, 142, 145, new Color(Accent.r, Accent.g, Accent.b, .045f));
            Blob(964, 159, 111, new Color(Accent.r, Accent.g, Accent.b, .055f));
            for (int i = 0; i < 15; i++)
            {
                float h = 55 + Mathf.Sin(i*7.3f)*24 + (i%3)*21;
                Box(i*92-10, 630-h, 74, h, new Color(.065f, .105f, .17f));
                for (int j=0;j<3;j++) Box(i*92+5+j*18, 643-h, 5, 5, new Color(.16f,.27f,.34f,.55f));
            }
            for (int i=0;i<32;i++) Blob(50+(i*173)%1180, 155+(i*97)%280, i%3==0?3:2, new Color(.5f,.7f,.85f,.3f));
            Text("SECTOR  /  0" + (stage+1), 62, 172, 600, 50, 13, mutedText);
            Text(sim.Level.Name, 60, 198, 720, 55, 39, new Color(.18f,.25f,.33f), TextAnchor.UpperLeft, true);
            Text(sim.Level.Subtitle, 62, 252, 650, 30, 16, mutedText);
        }

        const float Unit = 37.5f;
        Rect World(Rect b) => new Rect(40 + b.x * Unit, 638 - b.yMax * Unit, b.width * Unit, b.height * Unit);
        Vector2 Point(Vector2 p) => new Vector2(40 + p.x*Unit, 638-p.y*Unit);
        void DrawWorld()
        {
            foreach (var p in sim.Level.Platforms)
            {
                Rect b = World(p.At(sim.Time));
                Fill(b, new Color(.10f,.17f,.23f));
                Box(b.x,b.y,b.width,3,p.Travel!=0?new Color(.73f,.61f,1):Accent);
                for(float x=b.x+11;x<b.xMax-5;x+=28) Box(x,b.y+8,10,2,new Color(.25f,.36f,.43f));
                if (p.Travel != 0) { Box(b.x+8,b.y+11,b.width-16,2,new Color(.5f,.4f,.75f)); }
                if (p.Bounds.height > .8f) Box(b.x,b.y+25,b.width,13,new Color(.07f,.11f,.17f));
            }
            foreach (var d in sim.Level.Dangers)
            {
                Rect b = World(d.At(sim.Time));
                if (d.Laser)
                {
                    bool active = d.Active(sim.Time), warning = d.Warning(sim.Time);
                    Color c = active ? coral : warning ? new Color(1,.76f,.3f) : new Color(.2f,.34f,.39f);
                    Box(b.x-9,b.y-8,b.width+18,8,c); Box(b.x-9,b.yMax,b.width+18,8,c);
                    if(active) { Box(b.x-7,b.y,b.width+14,b.height,new Color(c.r,c.g,c.b,.12f)); Fill(b,c); Box(b.x+3,b.y,3,b.height,white); }
                    else for(float y=b.y;y<b.yMax;y+=16) Box(b.x+3,y,3,warning?10:4,c);
                    Text(active?"LIVE":warning?"WAIT":"SAFE",b.x-28,b.y-29,65,20,10,c,TextAnchor.MiddleCenter,true);
                }
                else if (d.Saw)
                {
                    Blob(b.x-6,b.y-6,b.width+12,new Color(coral.r,coral.g,coral.b,.13f));
                    var saved = GUI.matrix;
                    GUIUtility.RotateAroundPivot(sim.Time*150,new Vector2(b.center.x,b.center.y));
                    for(int i=0;i<8;i++) { GUIUtility.RotateAroundPivot(45,new Vector2(b.center.x,b.center.y)); DrawTex(triangle,new Rect(b.center.x-5,b.y-5,10,14),coral); }
                    GUI.matrix=saved;
                    Blob(b.x,b.y,b.width,coral); Blob(b.x+7,b.y+7,b.width-14,ink); Blob(b.x+11,b.y+11,5,white);
                }
                else for(float x=b.x;x<b.xMax-2;x+=14) DrawTex(triangle,new Rect(x,b.y,14,b.height),coral);
            }
            for(int i=0;i<sim.Level.Cells.Length;i++)
            {
                if(sim.Collected[i]) continue;
                var p=Point(sim.Level.Cells[i]); float bob=Mathf.Sin(sim.Time*3+i)*4;
                Blob(p.x-20,p.y-20+bob,40,new Color(Accent.r,Accent.g,Accent.b,.09f));
                DrawTex(diamond,new Rect(p.x-10,p.y-13+bob,20,26),Accent);
                DrawTex(diamond,new Rect(p.x-4,p.y-7+bob,8,14),white);
            }
            var cp=Point(sim.Level.Checkpoint);
            Box(cp.x,cp.y-38,3,38,sim.CheckpointReached?Accent:mutedText);
            Box(cp.x+3,cp.y-38,22,13,sim.CheckpointReached?Accent:mutedText);
            Text("CHECKPOINT",cp.x-39,cp.y+10,100,18,9,sim.CheckpointReached?Accent:mutedText,TextAnchor.MiddleCenter);
            var terminal=Point(sim.Level.Terminal);
            Box(terminal.x-14,terminal.y-16,28,36,new Color(.12f,.21f,.28f));
            Box(terminal.x-10,terminal.y-12,20,14,sim.RelayPowered?Accent:coral);
            Box(terminal.x-4,terminal.y+20,8,8,mutedText);
            Text("RELAY",terminal.x-31,terminal.y-36,65,16,10,sim.RelayPowered?Accent:mutedText,TextAnchor.MiddleCenter);
            var exit=Point(sim.Level.Exit); bool open=sim.CellCount==3 && sim.RelayPowered;
            Color gate=open?Accent:new Color(.29f,.35f,.44f);
            Box(exit.x-22,exit.y-39,44,65,new Color(gate.r,gate.g,gate.b,.1f));
            Box(exit.x-24,exit.y-41,4,70,gate); Box(exit.x+20,exit.y-41,4,70,gate); Box(exit.x-24,exit.y-41,48,4,gate);
            Text(open?">>":"x",exit.x-18,exit.y-22,36,40,25,gate,TextAnchor.MiddleCenter,true);
            Text("EXIT",exit.x-28,exit.y-64,56,18,11,gate,TextAnchor.MiddleCenter);
            DrawPlayer();
        }

        void DrawPlayer()
        {
            Rect b=World(sim.PlayerBounds);
            Color c=sim.Invulnerability>0 && Mathf.FloorToInt(sim.Time*16)%2==0?white:Accent;
            Blob(b.x-8,b.yMax-3,b.width+16,new Color(Accent.r,Accent.g,Accent.b,.07f));
            Box(b.x-3,b.y+9,5,16,new Color(.2f,.48f,.52f));
            Box(b.x,b.y+3,b.width,b.height-7,c);
            Box(b.x+3,b.y,b.width-6,5,c);
            Box(b.x+3,b.y+8,b.width-6,10,ink);
            float eye=sim.FacingRight?b.x+13:b.x+5;
            Box(eye,b.y+11,5,4,white);
            Box(b.x+5,b.y+24,b.width-10,3,ink);
            float walk=sim.Grounded && Mathf.Abs(sim.Velocity.x)>1?Mathf.Sin(sim.Time*22)*2:0;
            Box(b.x+2,b.yMax-5,7,6+walk,c); Box(b.xMax-9,b.yMax-5,7,6-walk,c);
            Box(b.center.x-1,b.y-5,2,6,c); Blob(b.center.x-3,b.y-8,6,coral);
            if(!sim.Grounded && sim.Velocity.y>0) DrawTex(triangle,new Rect(b.center.x-4,b.yMax+3,8,10),new Color(1,.75f,.35f));
        }

        void DrawHUD()
        {
            Box(0,0,1280,99,new Color(.045f,.069f,.116f));
            Box(40,98,1200,1,new Color(.19f,.28f,.36f));
            DrawTex(diamond,new Rect(42,28,27,37),Accent);
            Text("NEON",82,22,100,28,22,white,TextAnchor.UpperLeft,true);
            Text("RELAY",82,49,120,24,17,Accent,TextAnchor.UpperLeft,true);
            Text("0"+(stage+1)+" / 03",231,25,100,25,18,Accent,TextAnchor.UpperLeft,true);
            Text("RESTORE THE GRID",231,53,200,20,10,mutedText);
            for(int i=0;i<3;i++)
            {
                DrawTex(diamond,new Rect(515+i*29,32,17,23),sim.Collected[i]?Accent:new Color(.22f,.3f,.37f));
            }
            Text(sim.CellCount+" / 3 CELLS",509,63,140,18,10,mutedText);
            Blob(667,37,9,sim.RelayPowered?Accent:coral);
            Text(sim.RelayPowered?"RELAY ONLINE":"RELAY OFFLINE",684,31,166,26,12,sim.RelayPowered?Accent:mutedText);
            Text("RETRIES",890,26,100,18,10,mutedText); Text(TotalDeaths.ToString("00"),890,47,100,27,20,white);
            Text("RUN TIME",1000,26,100,18,10,mutedText); Text(Clock(TotalTime),1000,47,130,27,20,white);
            if(Button(muted?"SOUND OFF":"SOUND ON",1131,20,111,26,false,11)) ToggleMute();
            if(Button(screen==ScreenState.Paused?"RESUME":"PAUSE  II",1131,53,111,26,false,11) && (screen==ScreenState.Playing || screen==ScreenState.Paused)) SetScreen(screen==ScreenState.Paused?ScreenState.Playing:ScreenState.Paused);
            Box(40,664,1200,1,new Color(.19f,.28f,.36f));
            Text("A D / ARROWS   move     SPACE / W / UP   double jump     E   connect",42,680,850,22,12,mutedText);
            Text("R  restart    ESC  pause    M  sound",835,680,380,22,12,mutedText,TextAnchor.MiddleRight);
            for(int i=0;i<2;i++) Blob(1205+i*17,686,8,i<sim.JumpsLeft?Accent:new Color(.2f,.25f,.3f));
        }

        void DrawOverlay()
        {
            Box(0,100,1280,564,new Color(.02f,.035f,.075f,.84f));
            if(screen==ScreenState.Circuit) { DrawCircuit(); return; }
            Box(266,150,748,465,panel); Box(266,150,748,3,Accent);
            string kicker=screen==ScreenState.Title?"A SMALL ROBOT. A CITY WITHOUT POWER.":screen==ScreenState.Paused?"TAKE A BREATHER":screen==ScreenState.Clear?"SECTOR 0"+(stage+1)+" / RESTORED":"ALL SYSTEMS ONLINE";
            Text(kicker,310,184,660,30,12,Accent,TextAnchor.MiddleCenter,true);
            string title=screen==ScreenState.Title?"NEON RELAY":screen==ScreenState.Paused?"PAUSED":screen==ScreenState.Clear?"NICE CONNECTION.":"YOU LIT THE CITY.";
            Text(title,290,225,700,65,45,white,TextAnchor.MiddleCenter,true);
            string desc=screen==ScreenState.Title?"Cross the rooftops. Outsmart the hazards. Restore the grid.":screen==ScreenState.Paused?"Your run is frozen. The next jump can wait.":screen==ScreenState.Clear?"Three cells recovered. One more piece of the city is awake.":"Three sectors restored. Every leap made a difference.";
            Text(desc,303,297,674,50,17,mutedText,TextAnchor.MiddleCenter);
            if(screen==ScreenState.Title)
            {
                SmallCard("01", "JUMP", "Tap again in the air.",309,365);
                SmallCard("02", "THINK", "Route power with clues.",534,365);
                SmallCard("03", "RETRY", "Checkpoints keep it fair.",759,365);
                if(Button("START THE RUN   >",449,490,382,55,true,18)) StartRun();
                float best=PlayerPrefs.GetFloat("NeonRelay.Best",-1);
                Text(best<0?"3 sectors  /  no lives limit  /  keyboard controls":"PERSONAL BEST  /  "+Clock(best),310,560,660,26,12,mutedText,TextAnchor.MiddleCenter);
            }
            else if(screen==ScreenState.Paused)
            {
                if(Button("CONTINUE",449,365,382,50,true,18)) SetScreen(ScreenState.Playing);
                if(Button("RESTART THIS SECTOR",449,431,382,45,false,15)) RestartStage();
                if(Button("BACK TO TITLE",449,491,382,45,false,15)) { SetScreen(ScreenState.Title); }
            }
            else
            {
                Text("TIME  "+Clock(TotalTime)+"      /      RETRIES  "+TotalDeaths,315,372,650,35,21,Accent,TextAnchor.MiddleCenter,true);
                Text(screen==ScreenState.Finished?"PERSONAL BEST  "+Clock(PlayerPrefs.GetFloat("NeonRelay.Best",TotalTime)):LevelCatalog.Create(stage+1).Brief,315,422,650,40,14,mutedText,TextAnchor.MiddleCenter);
                if(Button(screen==ScreenState.Finished?"PLAY AGAIN   >":"NEXT SECTOR   >",449,493,382,55,true,18)) { if(screen==ScreenState.Finished) StartRun(); else NextStage(); }
                Text("Press ENTER to continue",310,563,660,22,12,mutedText,TextAnchor.MiddleCenter);
            }
        }

        void DrawCircuit()
        {
            Box(263,143,754,480,panel); Box(263,143,754,3,Accent);
            Text("RELAY 0"+(stage+1)+"  /  POWER ROUTING",300,172,680,28,12,Accent,TextAnchor.MiddleCenter,true);
            Text("MAKE THE CONNECTION",294,216,692,47,32,white,TextAnchor.MiddleCenter,true);
            Text(sim.Level.Hint,295,279,690,52,17,mutedText,TextAnchor.MiddleCenter);
            for(int i=0;i<3;i++)
            {
                bool on=(sim.SwitchMask&(1<<i))!=0;
                float x=366+i*190;
                Box(x+61,341,3,23,on?Accent:mutedText);
                Blob(x+53,328,20,on?Accent:new Color(.22f,.28f,.35f));
                if(Button((i==0?"LEFT":i==1?"MIDDLE":"RIGHT")+"\n"+(on?"ON":"OFF"),x,372,126,73,on,17)) Toggle(i);
                Text("[ "+(i+1)+" ]",x,454,126,24,12,mutedText,TextAnchor.MiddleCenter);
            }
            Text(circuitMessage.Length==0?"Toggle the lines, then send power. Wrong answers are safe.":circuitMessage,296,492,688,30,13,circuitMessage.Length==0?mutedText:coral,TextAnchor.MiddleCenter);
            if(Button("SEND POWER  /  ENTER",446,542,388,48,true,16)) Submit();
            if(Button("X",960,159,35,32,false,18)) SetScreen(ScreenState.Playing);
        }

        void SmallCard(string number,string title,string detail,float x,float y)
        {
            Box(x,y,211,90,new Color(.09f,.13f,.2f));
            Text(number,x+15,y+13,40,24,12,Accent);
            Text(title,x+48,y+11,150,28,16,white,TextAnchor.UpperLeft,true);
            Text(detail,x+15,y+51,185,27,12,mutedText);
        }
        bool Button(string text,float x,float y,float w,float h,bool primary,int size)
        {
            var rect=new Rect(x,y,w,h);
            bool hover=rect.Contains(Event.current.mousePosition);
            Fill(rect,primary?(hover?Color.Lerp(Accent,Color.white,.2f):Accent):(hover?new Color(.18f,.25f,.34f):new Color(.115f,.165f,.24f)));
            Text(text,x+6,y,w-12,h,size,primary?ink:white,TextAnchor.MiddleCenter,true);
            return GUI.Button(rect,GUIContent.none,GUIStyle.none);
        }
        void Pill(string text,float x,float y,float w,Color c) { Box(x,y,w,29,panel); Text(text,x,y,w,29,12,c,TextAnchor.MiddleCenter,true); }
        void Text(string text,float x,float y,float w,float h,int size,Color c,TextAnchor align=TextAnchor.UpperLeft,bool bold=false)
        {
            label.fontSize=size; label.normal.textColor=c; label.alignment=align; label.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            GUI.Label(new Rect(x,y,w,h),text,label);
        }
        void Box(float x,float y,float w,float h,Color c) => Fill(new Rect(x,y,w,h),c);
        void Fill(Rect r,Color c) { GUI.color=c; GUI.DrawTexture(r,Texture2D.whiteTexture); GUI.color=Color.white; }
        void Blob(float x,float y,float d,Color c) => DrawTex(disc,new Rect(x,y,d,d),c);
        void DrawTex(Texture2D texture,Rect r,Color c) { GUI.color=c; GUI.DrawTexture(r,texture); GUI.color=Color.white; }
        public static string Clock(float time) => ((int)time/60).ToString("00")+":"+((int)time%60).ToString("00")+"."+((int)(time*10)%10);
        static Texture2D Shape(Func<float,float,bool> inside)
        {
            const int n=64; var t=new Texture2D(n,n,TextureFormat.RGBA32,false); var pixels=new Color[n*n];
            for(int y=0;y<n;y++) for(int x=0;x<n;x++) pixels[y*n+x]=inside((x+.5f)/n*2-1,(y+.5f)/n*2-1)?Color.white:Color.clear;
            t.SetPixels(pixels); t.Apply(); t.filterMode=FilterMode.Bilinear; t.wrapMode=TextureWrapMode.Clamp; return t;
        }
        static AudioClip Tone(float start,float end,float duration)
        {
            const int rate=22050; var samples=new float[(int)(duration*rate)]; float phase=0;
            for(int i=0;i<samples.Length;i++) { float t=(float)i/samples.Length; phase+=Mathf.Lerp(start,end,t)*2*Mathf.PI/rate; samples[i]=Mathf.Sin(phase)*Mathf.Sin(Mathf.PI*t)*.45f; }
            var clip=AudioClip.Create("Synth",samples.Length,1,rate,false); clip.SetData(samples,0); return clip;
        }
        void OnDestroy()
        {
            foreach(var item in new UnityEngine.Object[] { disc,triangle,diamond,jumpClip,cellClip,deathClip,clearClip,buttonClip }) if(item!=null) Destroy(item);
        }
    }
}

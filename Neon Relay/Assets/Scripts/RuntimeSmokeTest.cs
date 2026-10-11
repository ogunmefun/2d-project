using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NeonRelay
{
    // Opt-in executable QA. In normal play this component is never created.
    public sealed class RuntimeSmokeTest : MonoBehaviour
    {
        Keyboard testKeyboard;
        string output;
        bool failed;
        bool hadBest;
        float savedBest;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested()
        {
            var args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"-smokeTest");
            if(index<0 || index+1>=args.Length) return;
            var runner=new GameObject("Runtime QA").AddComponent<RuntimeSmokeTest>();
            runner.output=args[index+1];
        }
        IEnumerator Start()
        {
            Directory.CreateDirectory(output);
            hadBest=PlayerPrefs.HasKey("NeonRelay.Best");
            savedBest=PlayerPrefs.GetFloat("NeonRelay.Best",0);
            Application.logMessageReceived+=OnLog;
            Application.runInBackground=true;
            testKeyboard=InputSystem.AddDevice<Keyboard>();
            // A hidden Windows player does not render. Begin visual QA only when shown.
            while(!Application.isFocused) yield return null;
            yield return new WaitForSecondsRealtime(2);
            var game=FindAnyObjectByType<NeonRelayGame>();
            Check(game!=null && game.CurrentScreen=="Title","Title screen loads");
            yield return Capture("title.png");
            yield return KeyPress(Key.H);
            Check(game.CurrentScreen=="Help","H opens the field manual from title");
            float helpTime=game.Simulation.Time;
            yield return new WaitForSecondsRealtime(.2f);
            Check(game.Simulation.Time==helpTime,"Field manual freezes simulation");
            yield return Capture("help.png");
            int originalSound=PlayerPrefs.GetInt("NeonRelay.Muted",0);
            yield return KeyPress(Key.M);
            Check(PlayerPrefs.GetInt("NeonRelay.Muted",0)!=originalSound,"M changes the saved sound option");
            yield return KeyPress(Key.M);
            Check(PlayerPrefs.GetInt("NeonRelay.Muted",0)==originalSound,"Sound option can be restored");
            // Exercise the same handler used by the clickable reduced-effects option.
            int originalMotion=PlayerPrefs.GetInt("NeonRelay.ReducedMotion",0);
            game.ToggleMotion();
            Check(PlayerPrefs.GetInt("NeonRelay.ReducedMotion",0)!=originalMotion,"Reduced effects option is saved");
            game.ToggleMotion();
            Check(PlayerPrefs.GetInt("NeonRelay.ReducedMotion",0)==originalMotion,"Effects option can be restored");
            yield return KeyPress(Key.F11);
            yield return new WaitForSecondsRealtime(.5f);
            Check(Screen.fullScreen,"F11 enters fullscreen");
            yield return KeyPress(Key.F11);
            yield return new WaitForSecondsRealtime(.5f);
            Check(!Screen.fullScreen,"F11 returns to windowed mode");
            yield return KeyPress(Key.Escape);
            Check(game.CurrentScreen=="Title","Escape returns from manual to title");
            yield return KeyPress(Key.Enter);
            yield return new WaitForSecondsRealtime(.2f);
            Check(game.CurrentScreen=="Playing","Enter starts the game");
            var start=game.Simulation.Position;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.D));
            yield return new WaitForSecondsRealtime(.2f);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            yield return null;
            Check(game.Simulation.Position.x>start.x+.5f,"D moves the player right");
            yield return KeyPress(Key.Space,.12f);
            Check(game.Simulation.Position.y>start.y+.3f,"Space jumps");
            yield return KeyPress(Key.Space,.12f);
            Check(game.Simulation.JumpEvents==2,"Second Space press double jumps");
            yield return Capture("gameplay.png");
            yield return KeyPress(Key.Escape);
            Check(game.CurrentScreen=="Paused","Escape pauses");
            float paused=game.Simulation.Time;
            yield return new WaitForSecondsRealtime(.3f);
            Check(Mathf.Abs(game.Simulation.Time-paused)<.001f,"Pause freezes gameplay time");
            yield return Capture("paused.png");
            yield return KeyPress(Key.Escape);
            Check(game.CurrentScreen=="Playing","Escape resumes");
            yield return KeyPress(Key.H);
            yield return KeyPress(Key.Escape);
            Check(game.CurrentScreen=="Paused","Closing in-game help keeps the run safely paused");
            yield return KeyPress(Key.Escape);
            yield return KeyPress(Key.R);
            Check(game.Simulation.CellCount==0 && game.Simulation.Position.x<2,"R restarts the sector");
            // Isolate the terminal UI after the normal-input route is proven by editor checks.
            game.Simulation.Position=new Vector2(28.7f,1);
            game.Simulation.Velocity=Vector2.zero;
            yield return KeyPress(Key.E);
            Check(game.CurrentScreen=="Circuit","E opens a nearby relay");
            yield return Capture("circuit.png");
            yield return KeyPress(Key.Enter);
            Check(game.CurrentScreen=="Circuit" && !game.Simulation.RelayPowered,"Wrong routing stays in the puzzle");
            yield return KeyPress(Key.Digit1);
            yield return KeyPress(Key.Digit3);
            yield return KeyPress(Key.Enter);
            Check(game.CurrentScreen=="Playing" && game.Simulation.RelayPowered,"Keyboard routing solves the puzzle");
            // Isolate stage progression and results; GameChecks proves traversal separately.
            for(int sector=0;sector<LevelCatalog.Count;sector++)
            {
                if(sector>0)
                {
                    game.Simulation.Position=new Vector2(28.7f,1);
                    game.Simulation.Velocity=Vector2.zero;
                    yield return KeyPress(Key.E);
                    for(int bit=0;bit<3;bit++)
                        if((game.Simulation.Level.Target & (1<<bit))!=0)
                            yield return KeyPress(bit==0?Key.Digit1:bit==1?Key.Digit2:Key.Digit3);
                    yield return Capture("circuit-sector-"+(sector+1)+".png");
                    yield return KeyPress(Key.Enter);
                    Check(game.Simulation.RelayPowered,"Sector "+(sector+1)+" circuit can be solved with keyboard input");
                }
                for(int i=0;i<game.Simulation.Collected.Length;i++) game.Simulation.Collected[i]=true;
                game.Simulation.Position=game.Simulation.Level.Exit;
                game.Simulation.Velocity=Vector2.zero;
                yield return new WaitForSecondsRealtime(.15f);
                Check(game.CurrentScreen==(sector==2?"Finished":"Clear"),"Sector "+(sector+1)+" reaches the correct completion screen");
                yield return Capture(sector==2?"results.png":"sector-clear-"+(sector+1)+".png");
                yield return KeyPress(Key.Enter);
                Check(game.CurrentScreen=="Playing" && game.Simulation.CellCount==0 && !game.Simulation.RelayPowered,
                    sector==2?"Replay starts a clean run":"Next sector resets objectives");
                if(sector<2) yield return Capture("sector-"+(sector+2)+".png");
            }
            File.AppendAllText(Path.Combine(output,"runtime-checks.txt"),failed?"\nFAILED\n":"\nALL RUNTIME CHECKS PASSED\n");
            yield return new WaitForSecondsRealtime(.3f);
            Application.Quit(failed?1:0);
        }
        IEnumerator KeyPress(Key key,float duration=.08f)
        {
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(key));
            yield return new WaitForSecondsRealtime(duration);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            yield return new WaitForSecondsRealtime(.08f);
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(output,name));
            yield return new WaitForSecondsRealtime(.25f);
        }
        void Check(bool condition,string message)
        {
            if(!condition) failed=true;
            File.AppendAllText(Path.Combine(output,"runtime-checks.txt"),(condition?"PASS: ":"FAIL: ")+message+"\n");
        }
        void OnLog(string message,string stack,LogType type)
        {
            if(type==LogType.Error || type==LogType.Exception || type==LogType.Assert)
            { failed=true; File.AppendAllText(Path.Combine(output,"runtime-checks.txt"),"RUNTIME ERROR: "+message+"\n"); }
        }
        void OnDestroy()
        {
            Application.logMessageReceived-=OnLog;
            if(testKeyboard!=null) InputSystem.RemoveDevice(testKeyboard);
            // Synthetic completion must never overwrite the player's real personal best.
            if(hadBest) PlayerPrefs.SetFloat("NeonRelay.Best",savedBest); else PlayerPrefs.DeleteKey("NeonRelay.Best");
            PlayerPrefs.Save();
        }
    }
}

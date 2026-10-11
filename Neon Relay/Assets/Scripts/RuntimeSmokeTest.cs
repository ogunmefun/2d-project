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
            hadBest=PlayerPrefs.HasKey("NeonRelay.RunnerBest");
            savedBest=PlayerPrefs.GetFloat("NeonRelay.RunnerBest",0);
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
            yield return new WaitForSecondsRealtime(1.3f);
            Check(game.CurrentScreen=="Playing","Enter starts the game");
            var start=game.Simulation.Position;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            yield return new WaitForSecondsRealtime(.2f);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            yield return null;
            Check(game.Simulation.Position.x>start.x+.5f,"Player runs forward without movement input");
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
            yield return KeyPress(Key.E);
            Check(game.CurrentScreen=="Playing","No connector puzzle opens on E");
            // Traverse each complete course with real Input System jump events, without teleporting.
            for(int sector=0;sector<LevelCatalog.Count;sector++)
            {
                Check(game.CurrentStage==sector,"Correct stage loaded automatically: "+(sector+1));
                yield return Capture("sector-"+(sector+1)+".png");
                yield return RunCourse(game);
                // COURSE_OUTCOME
                Check(game.Simulation.Won && game.Simulation.Deaths==0,"Stage "+(sector+1)+" completed using keyboard jump timing");
                Check(game.CurrentScreen==(sector==2?"Finished":"Clear"),"Stage "+(sector+1)+" shows completion screen");
                yield return Capture(sector==2?"results.png":"sector-clear-"+(sector+1)+".png");
                if(sector<2) {
                    yield return new WaitForSecondsRealtime(1.15f);
                    Check(game.CurrentScreen=="Playing" && game.CurrentStage==sector+1,"Finish automatically advances without Enter");
                }
            }
            // REPLAY_CHECK
            yield return KeyPress(Key.Enter);
            Check(game.CurrentStage==0 && game.CurrentScreen=="Playing" && game.Simulation.CellCount==0,"Replay starts a clean run");
            while(game.Simulation.ReadyTime>0) yield return null;
            game.Simulation.Position=game.Simulation.Level.Exit+Vector2.up*3;
            yield return new WaitForSecondsRealtime(.1f);
            Check(game.CurrentScreen=="Clear" && game.Simulation.CellCount==0,"Finish accepts zero bonus cells while airborne");
            File.AppendAllText(Path.Combine(output,"runtime-checks.txt"),failed?"\nFAILED\n":"\nALL RUNTIME CHECKS PASSED\n");
            yield return new WaitForSecondsRealtime(.3f);
            Application.Quit(failed?1:0);
        }
        IEnumerator RunCourse(NeonRelayGame game)
        {
            var sim=game.Simulation;
            float[] jumps={8.2f,15.2f,sim.Level.Platforms[0].Bounds.xMax-1.2f,33.2f,54.2f,62.8f,75.2f,84.2f};
            int next=0; float secondAt=float.MaxValue;
            float deadline=Time.realtimeSinceStartup+35;
            while(game.CurrentScreen=="Playing" && sim.Deaths==0 && Time.realtimeSinceStartup<deadline)
            {
                bool press=sim.ReadyTime<=0 && sim.Time>=secondAt;
                if(press) secondAt=float.MaxValue;
                if(sim.ReadyTime<=0 && next<jumps.Length && sim.Position.x>=jumps[next])
                {
                    press=true;
                    if(next==2 || next==5) secondAt=sim.Time+.375f;
                    next++;
                }
                if(press) {
                    InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
                    yield return null;
                    InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.Space));
                }
                yield return null;
            }
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
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
            if(hadBest) PlayerPrefs.SetFloat("NeonRelay.RunnerBest",savedBest); else PlayerPrefs.DeleteKey("NeonRelay.RunnerBest");
            PlayerPrefs.Save();
        }
    }
}

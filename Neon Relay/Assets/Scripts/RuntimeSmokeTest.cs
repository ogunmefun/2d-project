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
            Application.logMessageReceived+=OnLog;
            Application.runInBackground=true;
            testKeyboard=InputSystem.AddDevice<Keyboard>();
            yield return new WaitForSecondsRealtime(2);
            var game=FindAnyObjectByType<NeonRelayGame>();
            Check(game!=null && game.CurrentScreen=="Title","Title screen loads");
            yield return Capture("title.png");
            yield return KeyPress(Key.Enter);
            yield return new WaitForSecondsRealtime(.2f);
            Check(game.CurrentScreen=="Playing","Enter starts the game");
            var start=game.Simulation.Position;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(Key.D));
            yield return new WaitForSecondsRealtime(.35f);
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());
            yield return null;
            Check(game.Simulation.Position.x>start.x+1,"D moves the player right");
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
        void OnDestroy() { Application.logMessageReceived-=OnLog; if(testKeyboard!=null) InputSystem.RemoveDevice(testKeyboard); }
    }
}

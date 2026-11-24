using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace LanternGarden
{
    // Opt-in release verification. No test component exists during a normal launch.
    public sealed class GardenSmokeTest : MonoBehaviour
    {
        public string OutputDirectory;
        readonly List<string> results = new List<string>();
        readonly List<string> errors = new List<string>();
        GardenGame game;
        ExplorerController player;

        IEnumerator Start()
        {
            Directory.CreateDirectory(OutputDirectory);
            Application.logMessageReceived += CaptureError;
            game = GardenGame.Instance; player = game.player;
            yield return Guard(Run());
            if (errors.Count > 0) results.AddRange(errors.Select(e => "FAIL: " + e));
            File.WriteAllLines(Path.Combine(OutputDirectory, "runtime-checks.txt"), results);
            Application.logMessageReceived -= CaptureError;
            Application.Quit(results.Any(r => r.StartsWith("FAIL")) ? 1 : 0);
        }
        void CaptureError(string condition, string trace, LogType kind) { if (kind == LogType.Exception || kind == LogType.Error || kind == LogType.Assert) errors.Add(condition); }
        void Check(bool ok, string name) { if (!ok) throw new Exception(name); results.Add("PASS: " + name); }
        IEnumerator Guard(IEnumerator routine)
        {
            var stack = new Stack<IEnumerator>(); stack.Push(routine);
            while (stack.Count > 0)
            {
                object current = null; bool moved = false; Exception failure = null;
                try { moved = stack.Peek().MoveNext(); if (moved) current = stack.Peek().Current; }
                catch (Exception e) { failure = e; }
                if (failure != null) { results.Add("FAIL: " + failure.Message); yield break; }
                if (!moved) { stack.Pop(); continue; }
                if (current is IEnumerator nested) stack.Push(nested);
                else yield return current;
            }
        }
        IEnumerator Screenshot(string name)
        {
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(Path.Combine(OutputDirectory, name + ".png"));
            yield return new WaitForSecondsRealtime(.3f);
        }
        IEnumerator Walk(Vector2 destination, float tolerance = .4f)
        {
            float start = Time.realtimeSinceStartup;
            while (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), destination) > tolerance)
            {
                if (game.State == GardenState.Won) { player.SetTestInput(Vector2.zero); yield break; }
                if (Time.realtimeSinceStartup - start > 16) throw new Exception("Walk blocked on route to " + destination + " from " + player.transform.position);
                Vector2 delta = destination - new Vector2(player.transform.position.x, player.transform.position.z);
                player.SetTestInput(delta.normalized); yield return null;
            }
            player.SetTestInput(Vector2.zero); yield return new WaitForSeconds(.16f);
        }
        IEnumerator Run()
        {
            yield return new WaitForSeconds(.8f);
            Check(game.State == (game.QuickStart ? GardenState.Playing : GardenState.Title), game.QuickStart ? "Desktop quick-start opens directly in gameplay." : "Player opens on the title screen.");
            Check(game.Total == 8, "Eight crystals are present in the Windows player.");
            Check(RenderSettings.sun && RenderSettings.sun.shadows == LightShadows.Soft, "Sunlight casts real soft shadows in the player.");
            Check(QualitySettings.shadows == ShadowQuality.All, "Shadows are enabled by default.");
            Check(FindObjectsByType<Light>().Count(l => l.type == LightType.Point && l.shadows == LightShadows.Soft) >= 6, "Lantern point lights cast soft shadows.");
            yield return Screenshot(game.QuickStart ? "01-quick-start" : "01-title");
            game.OpenHelp(); Check(game.State == GardenState.Help, "Help opens from the initial screen.");
            yield return Screenshot("02-help");
            game.CloseHelp(); Check(game.State == (game.QuickStart ? GardenState.Paused : GardenState.Title), "Help returns safely to the previous screen or pauses a walk.");
            game.StartGame(); yield return new WaitForSeconds(.5f);
            Check(game.IsPlaying && game.Collected == 0, "Starting creates a fresh walk.");
            Check(player.Grounded, "Explorer settles on solid ground.");
            Check(!game.TryFinish() && !game.gate.IsOpen, "Exit cannot finish before collecting every crystal.");
            Vector3 startPosition = player.transform.position;
            player.SetTestInput(Vector2.up); yield return new WaitForSeconds(.4f); player.SetTestInput(Vector2.zero);
            Check(player.transform.position.z > startPosition.z + 1, "Directional input moves the CharacterController.");
            float ground = player.transform.position.y;
            player.SetTestInput(Vector2.zero, true); yield return new WaitForSeconds(.18f);
            Check(player.transform.position.y > ground + .35f, "Jump input lifts the explorer.");
            yield return new WaitForSeconds(.8f); Check(player.Grounded, "Gravity lands the explorer safely.");
            game.TogglePause(); Vector3 pausedPosition = player.transform.position; float pausedTime = game.Elapsed;
            player.SetTestInput(Vector2.right); yield return new WaitForSeconds(.3f);
            Check(game.State == GardenState.Paused && Vector3.Distance(pausedPosition, player.transform.position) < .01f && Mathf.Abs(game.Elapsed - pausedTime) < .01f, "Pause freezes movement and walk time.");
            player.SetTestInput(Vector2.zero); game.OpenHelp(); game.CloseHelp();
            Check(game.State == GardenState.Paused, "Closing in-game help safely returns to pause.");
            game.TogglePause(); Check(game.IsPlaying, "Resume returns to gameplay.");
            bool mute = game.Muted; game.ToggleSound(); Check(game.Muted != mute, "Sound toggle works."); game.ToggleSound();
            yield return Screenshot("03-garden-shadows");
            game.ToggleShadows(); Check(QualitySettings.shadows == ShadowQuality.Disable, "Shadow option disables shadows.");
            yield return Screenshot("04-garden-no-shadows");
            game.ToggleShadows(); Check(QualitySettings.shadows == ShadowQuality.All, "Shadow option restores shadows.");

            // The complete winning route uses movement and trigger collisions, with no teleports or direct pickup calls.
            int[] route = { 7, 0, 5, 4, 3, 2, 1, 6 };
            foreach (int id in route)
            {
                if (id == 3) yield return Walk(new Vector2(0, 10));
                if (id == 6) { yield return Walk(new Vector2(0, -4)); yield return Walk(new Vector2(0, -1.5f)); }
                var crystal = game.crystals.Single(c => c.crystalId == id);
                yield return Walk(new Vector2(crystal.transform.position.x, crystal.transform.position.z));
                Check(crystal.Collected, "Walking route collects " + crystal.locationName + " through its trigger.");
                if (id == 5) yield return Screenshot("05-west-garden");
            }
            Check(game.Collected == 8 && game.gate.IsOpen && !game.gate.seal.activeSelf, "All eight crystals open the arch and remove its barrier.");
            Check(!game.Collect(game.crystals[0]) && game.Collected == 8, "A collected crystal cannot be counted twice.");
            Check(!game.TryFinish(), "Finishing also requires reaching the arch.");
            yield return Screenshot("06-shrine-and-open-gate");
            yield return Walk(new Vector2(0, 10));
            yield return Walk(new Vector2(0, 13.5f));
            yield return new WaitForSeconds(.3f);
            Check(game.State == GardenState.Won, "The full walking route reaches the victory screen.");
            yield return Screenshot("07-garden-restored");
            game.StartGame(); yield return new WaitForSeconds(.3f);
            Check(game.Collected == 0 && !game.gate.IsOpen && game.crystals.All(c => !c.Collected), "Replay resets crystals and relocks the arch.");
            yield return Walk(new Vector2(0, -5)); Check(game.Collected == 1, "Collectibles work again after replay.");
            player.Teleport(new Vector3(19, -6, 0)); yield return new WaitForSeconds(.3f);
            Check(game.Rescues == 1 && game.Collected == 1 && Vector3.Distance(player.transform.position, player.Spawn) < 1, "An isolated fall test returns to spawn and preserves crystals.");
            game.StartGame(); yield return new WaitForSeconds(.2f);
            // This route drives into a solid crate and checks that collision blocks entry.
            yield return Walk(new Vector2(3, -11));
            player.SetTestInput(Vector2.right); yield return new WaitForSeconds(1); player.SetTestInput(Vector2.zero);
            Check(player.transform.position.x < 4.5f, "Solid crate collision prevents walking through objects.");
            Check(errors.Count == 0, "No runtime exceptions or errors were reported.");
        }
    }
}

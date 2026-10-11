using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonRelay.Editor
{
    public static class GameChecks
    {
        static readonly List<string> passed = new List<string>();
        static void Check(bool condition, string message)
        {
            if (!condition) throw new Exception("FAIL: " + message);
            passed.Add("PASS: " + message);
        }
        static void Idle(GameSimulation sim, int frames = 120) { for(int i=0;i<frames;i++) sim.Tick(0,false,false); }

        public static string Run()
        {
            passed.Clear();
            var sim = new GameSimulation(0);
            Idle(sim);
            Check(sim.Grounded && Mathf.Abs(sim.Position.y - 1) < .01f, "Player spawns and lands on a solid floor");
            sim.Tick(0,true,true);
            for(int i=0;i<25;i++) sim.Tick(0,false,true);
            float before=sim.Position.y;
            sim.Tick(0,true,true);
            Check(sim.Velocity.y > 10 && sim.JumpsLeft == 0 && sim.Position.y > before, "Second jump works in midair");
            int jumps=sim.JumpEvents;
            sim.Tick(0,true,true);
            Check(sim.JumpEvents == jumps, "Third air jump is rejected");
            Idle(sim,240);
            Check(sim.Grounded && sim.JumpsLeft==2, "Landing restores both jumps");
            var shortJump=new GameSimulation(0); Idle(shortJump);
            shortJump.Tick(0,true,true); shortJump.Tick(0,false,false);
            Check(shortJump.Velocity.y<5.6f, "Releasing jump cuts ascent for precise control");
            var collision=new GameSimulation(0); Idle(collision);
            for(int i=0;i<300;i++) collision.Tick(-1,false,false);
            Check(collision.Position.x>=GameSimulation.Width*.5f, "World boundary blocks walking out of the level");
            collision.Position=new Vector2(12.5f,1.05f);
            collision.Tick(0,false,false);
            Check(collision.Deaths==1 && collision.Position.x<2, "Spikes trigger a safe respawn");
            collision.Position=new Vector2(8.7f,-3);
            collision.Tick(0,false,false);
            Check(collision.Deaths==2, "Falling into a gap triggers a respawn");

            for(int level=0;level<LevelCatalog.Count;level++)
            {
                var game=new GameSimulation(level);
                Idle(game);
                // Integration route uses only actual movement and jump input; no position overrides.
                JumpTo(game,5.6f,2.5f);
                Check(game.Collected[0], "Sector "+(level+1)+": first cell reachable by normal jumps");
                JumpTo(game,10.5f,3.7f);
                JumpTo(game,15.5f,4.7f);
                Check(game.Collected[1], "Sector "+(level+1)+": middle cell reachable through the obstacle route");
                WalkTo(game,17);
                Check(game.CheckpointReached, "Sector "+(level+1)+": checkpoint activates through normal movement");
                if(level>0) WaitForLaser(game,game.Level.Dangers[3],.25f);
                JumpTo(game,21.3f,3.5f);
                if(level==2) WaitForLaser(game,game.Level.Dangers[4],.3f);
                JumpTo(game,26.3f,2.5f);
                Check(game.CellCount==3, "Sector "+(level+1)+": all three cells collected using normal movement");
                WalkTo(game,28.7f); Idle(game,120);
                Check(game.CanUseTerminal, "Sector "+(level+1)+": relay terminal reachable");
                Check(!game.SubmitRelay(), "Sector "+(level+1)+": wrong circuit answer is rejected");
                for(int bit=0;bit<3;bit++) if((game.Level.Target&(1<<bit))!=0) game.ToggleSwitch(bit);
                Check(game.SubmitRelay(), "Sector "+(level+1)+": matching circuit answer powers relay");
                int cells=game.CellCount;
                game.Kill();
                Check(game.CellCount==cells && game.RelayPowered && Mathf.Abs(game.Position.x-17)<.1f,
                    "Sector "+(level+1)+": checkpoint death preserves cells and solved relay");
                // Repeat the last two jumps after a checkpoint respawn.
                Idle(game,120);
                if(level>0) WaitForLaser(game,game.Level.Dangers[3],.25f);
                JumpTo(game,21.3f,3.5f);
                if(level==2) WaitForLaser(game,game.Level.Dangers[4],.3f);
                JumpTo(game,26.3f,2.5f);
                WalkTo(game,30.6f);
                Idle(game,180);
                Check(game.Won, "Sector "+(level+1)+": exit can be reached and completed after a checkpoint retry");
            }
            var laser=LevelCatalog.Create(1).Dangers[3];
            Check(!laser.Active(.5f) && laser.Warning(1.5f) && laser.Active(2), "Laser has safe, warning, and harmful phases");
            var platform=LevelCatalog.Create(1).Platforms[4];
            Check(Mathf.Abs(platform.At(0).x-platform.At(1).x)>.1f, "Moving platform changes its position");
            var locked=new GameSimulation(0); locked.Position=locked.Level.Exit; locked.Tick(0,false,false);
            Check(!locked.Won, "Exit stays locked without energy cells and relay power");
            var distant=new GameSimulation(0);
            distant.ToggleSwitch(0); distant.ToggleSwitch(2);
            Check(!distant.SubmitRelay(), "A correct circuit cannot be submitted remotely");
            distant.ToggleSwitch(-1); distant.ToggleSwitch(3);
            Check(distant.SwitchMask==5, "Invalid relay switch indices cannot alter the circuit");
            var carried=new GameSimulation(1);
            var moving=carried.Level.Platforms[4].At(carried.Time);
            carried.Position=new Vector2(moving.center.x,moving.yMax+.01f);
            Idle(carried,15);
            Check(carried.Grounded,"Robot lands on the moving platform");
            float relativeX=carried.Position.x-carried.Level.Platforms[4].At(carried.Time).x;
            Idle(carried,60);
            Check(carried.Grounded && Mathf.Abs(carried.Position.x-carried.Level.Platforms[4].At(carried.Time).x-relativeX)<.01f,
                "Moving platform carries a standing robot without sliding");
            var floor=new GameSimulation(0); Idle(floor);
            floor.Position=new Vector2(15.5f,4.7f); Idle(floor,15);
            Check(floor.Deaths==0 && Mathf.Abs(floor.Position.x-15.5f)<.01f && floor.Grounded,
                "Exact floor contact never becomes a horizontal side collision");
            return string.Join("\n",passed)+"\n\n"+passed.Count+" checks passed.\n";
        }

        static float Steer(GameSimulation game,float target)
        {
            float error=target-game.Position.x;
            // Brake early enough that the character settles near the requested platform centre.
            if(Mathf.Abs(error)<.045f && Mathf.Abs(game.Velocity.x)<.3f) return 0;
            return Mathf.Clamp(error*8-game.Velocity.x*.55f,-1,1);
        }
        static void WalkTo(GameSimulation game,float x)
        {
            int deaths=game.Deaths;
            for(int i=0;i<600 && !game.Won;i++)
            {
                var previous=game.Position;
                game.Tick(Steer(game,x),false,false);
                if(game.Deaths!=deaths) throw new Exception("Walk route died in "+game.Level.Name+" from "+previous+" at target "+x+" time="+game.Time);
                if(Mathf.Abs(game.Position.x-x)<.08f && Mathf.Abs(game.Velocity.x)<.5f) return;
            }
            if(!game.Won) throw new Exception("Walk timed out: "+game.Position+" target="+x);
        }
        static void JumpTo(GameSimulation game,float x,float top)
        {
            int deaths=game.Deaths;
            for(int i=0;i<480;i++)
            {
                game.Tick(Steer(game,x),i==0 || i==40,true);
                if(game.Deaths!=deaths) throw new Exception("Jump route died: target="+x+" tick="+i+" time="+game.Time);
                if(i>45 && game.Grounded && Mathf.Abs(game.Position.x-x)<.18f && Mathf.Abs(game.Position.y-top)<.1f) return;
            }
            throw new Exception("Jump timed out: "+game.Position+" target="+x+", "+top);
        }
        static void WaitForLaser(GameSimulation game,Danger laser,float phase)
        {
            for(int i=0;i<500;i++)
            {
                if(laser.Cycle(game.Time)>=phase && laser.Cycle(game.Time)<phase+.03f) return;
                game.Tick(0,false,false);
            }
            throw new Exception("Could not find safe laser phase");
        }
    }
}

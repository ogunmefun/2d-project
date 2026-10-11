using System;
using System.Text;
using UnityEngine;
namespace NeonRelay.Editor
{
    public static class GameChecks
    {
        static StringBuilder report;
        static int count;
        static void Check(bool value,string message) { if(!value) throw new Exception("FAIL: "+message); count++; report.AppendLine("PASS: "+message); }
        static void Ready(GameSimulation s) { while(s.ReadyTime>0) s.Tick(false,false); }
        public static string Run()
        {
            report=new StringBuilder(); count=0;
            var s=new GameSimulation(0); var spawn=s.Position;
            for(int i=0;i<30;i++) s.Tick(false,false);
            Check(s.Position==spawn && s.Time==0,"Ready countdown gives the player preparation time");
            Ready(s); for(int i=0;i<30;i++) s.Tick(false,false);
            Check(s.Position.x>spawn.x+1 && s.Grounded,"Runs forward without movement input and stays on floor");
            s.Tick(true,true); float first=s.Position.y;
            for(int i=0;i<12;i++) s.Tick(false,true);
            s.Tick(true,true); s.Tick(true,true);
            Check(s.JumpEvents==2 && s.JumpsLeft==0 && s.Position.y>first,"Double jump works and a third air jump is rejected");
            var high=new GameSimulation(0); var low=new GameSimulation(0); Ready(high); Ready(low);
            high.Tick(true,true); low.Tick(true,true);
            for(int i=0;i<20;i++) { high.Tick(false,true); low.Tick(false,false); }
            Check(high.Position.y>low.Position.y+.5f,"Releasing jump early produces a short hop");
            s=new GameSimulation(0); Ready(s); for(int i=0;i<240 && s.Deaths==0;i++) s.Tick(false,false);
            Check(s.Deaths==1 && s.Position.x==s.Level.Spawn.x && s.ReadyTime>0,"Missing the first spike causes a retry with preparation time");
            s=new GameSimulation(0); Ready(s); s.Position=new Vector2(48,4); s.Tick(false,false);
            Check(s.CheckpointReached,"Crossing the checkpoint while airborne saves progress");
            s.Collected[0]=true; s.Kill();
            Check(s.Position.x==48 && s.CellCount==1 && s.Deaths==1,"Retry keeps checkpoint and optional cells");
            Ready(s); s.Position=new Vector2(60,-3); s.Tick(false,false);
            Check(s.Deaths==2 && s.Position.x==48,"Pit death returns to checkpoint");
            for(int stage=0;stage<LevelCatalog.Count;stage++)
            {
                for(int variant=0;variant<3;variant++)
                {
                    s=new GameSimulation(stage); Ready(s);
                    float offset=(variant-1)*.15f;
                    float[] jumps={8.2f,15.2f,s.Level.Platforms[0].Bounds.xMax-1.2f,33.2f,54.2f,62.8f,75.2f,84.2f};
                    int next=0, secondAt=-1;
                    for(int tick=0;tick<4000 && !s.Won && s.Deaths==0;tick++)
                    {
                        bool jump=tick==secondAt;
                        if(next<jumps.Length && s.Position.x>=jumps[next]+offset)
                        { jump=true; if(next==2 || next==5) secondAt=tick+45; next++; }
                        s.Tick(jump,true);
                    }
                    // EXPECT_ROUTE
                    Check(s.Won && s.Deaths==0,"Stage "+(stage+1)+" completes using jump inputs only, timing variant "+variant);
                    Check(s.CheckpointReached,"Stage "+(stage+1)+" route crosses the checkpoint");
                }
                s=new GameSimulation(stage); Ready(s); s.Position=new Vector2(s.Level.Exit.x,5); s.Tick(false,false);
                // EXPECT_FINISH
                Check(s.Won && s.CellCount==0 && s.Progress==1,"Stage "+(stage+1)+" accepts an airborne finish without any cells or puzzles");
                float time=s.Time; var end=s.Position; s.Tick(true,true); s.Kill();
                Check(s.Position==end && s.Time==time && s.Deaths==0,"Completed stage freezes until transition");
            }
            return report.ToString()+"\nALL "+count+" GAMEPLAY CHECKS PASSED\n";
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
namespace NeonRelay
{
    public sealed class Platform
    {
        public Rect Bounds;
        public float Travel, Speed = 1, Phase;
        public Platform(float x, float y, float w, float h) { Bounds = new Rect(x,y,w,h); }
        public Rect At(float time) { var b=Bounds; b.x+=Mathf.Sin(time*Speed+Phase)*Travel; return b; }
    }
    public sealed class Danger
    {
        public Rect Bounds;
        public bool Laser, Saw;
        public float Travel, Speed = 1, Phase;
        public Danger(float x,float y,float w,float h) { Bounds=new Rect(x,y,w,h); }
        public Rect At(float time) { var b=Bounds; b.x+=Mathf.Sin(time*Speed+Phase)*Travel; return b; }
        public float Cycle(float time) => Mathf.Repeat(time+Phase,3.6f);
        public bool Active(float time) => !Laser || Cycle(time)>=1.8f;
        public bool Warning(float time) => Laser && Cycle(time)>=1.3f && Cycle(time)<1.8f;
    }
    public sealed class Level
    {
        public string Name, Subtitle, Brief;
        public Platform[] Platforms;
        public Danger[] Dangers;
        public Vector2[] Cells;
        public Vector2 Spawn = new Vector2(1.5f,1.03f);
        public Vector2 Checkpoint = new Vector2(48,1.03f);
        public Vector2 Exit = new Vector2(94.5f,1.7f);
        public float Length = 96, RunSpeed;
        public Color Accent;
    }
    public static class LevelCatalog
    {
        public const int Count=3;
        public static Level Create(int index)
        {
            if(index<0 || index>=Count) throw new System.ArgumentOutOfRangeException(nameof(index));
            float gapStart=index==0?24:index==1?23:24;
            float gapEnd=index==0?27:index==1?27:29.5f;
            float secondEnd=index==0?67:index==1?68:69.5f;
            var dangers=new List<Danger> {
                new Danger(10,1,index==0?.8f:1.3f,.65f),
                new Danger(17,1,index==0?.8f:1.4f,.65f),
                new Danger(35,1,index==0?1:2.2f,.65f),
                new Danger(56,1,index==0?1:1.8f,.65f),
                new Danger(77,1,index==0?1:2,.65f),
                new Danger(86,1,index==0?1:1.8f,.65f)
            };
            if(index>0) dangers[3]=new Danger(56,1,.95f,.95f) { Saw=true,Travel=.3f,Speed=2 };
            if(index==2) dangers[5]=new Danger(86,1,1.15f,1.15f) { Saw=true,Travel=.35f,Speed=2.4f };
            return new Level {
                Name=new[]{"THE ROOFTOPS","PULSE WORKS","THE CORE"}[index],
                Subtitle=new[]{"Find your rhythm.","Keep moving. Trust your timing.","Longer gaps. One final sprint."}[index],
                Brief=new[]{"Auto-run is on. Tap Space to jump; tap again in the air.","Clear the spinning saw and wider gaps. Keep a jump in reserve.","Use both jumps for the long gaps. Reach the finish."}[index],
                RunSpeed=6+index*.6f,
                Accent=new[]{new Color(.28f,.94f,.83f),new Color(.68f,.52f,1),new Color(1,.73f,.34f)}[index],
                Platforms=new[]{new Platform(0,0,gapStart,1),new Platform(gapEnd,0,64-gapEnd,1),new Platform(secondEnd,0,96-secondEnd,1)},
                Dangers=dangers.ToArray(),
                Cells=new[]{new Vector2(17.5f,3.2f),new Vector2(36,3.2f),new Vector2(78,3.2f)}
            };
        }
    }
}

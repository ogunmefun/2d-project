using UnityEngine;

namespace NeonRelay
{
    public sealed class Platform
    {
        public Rect Bounds;
        public float Travel, Speed, Phase;
        public Platform(float x, float y, float w, float h, float travel = 0, float speed = 1)
        { Bounds = new Rect(x, y, w, h); Travel = travel; Speed = speed; }
        public Rect At(float time)
        {
            var b = Bounds;
            b.x += Mathf.Sin(time * Speed + Phase) * Travel;
            return b;
        }
    }

    public sealed class Danger
    {
        public Rect Bounds;
        public bool Laser, Saw;
        public float Travel, Speed = 1, Phase;
        public Danger(float x, float y, float w, float h) { Bounds = new Rect(x, y, w, h); }
        public Rect At(float time)
        {
            var b = Bounds;
            b.x += Mathf.Sin(time * Speed + Phase) * Travel;
            return b;
        }
        public float Cycle(float time) => Mathf.Repeat(time + Phase, 3.6f);
        public bool Active(float time) => !Laser || Cycle(time) >= 1.8f;
        public bool Warning(float time) => Laser && Cycle(time) >= 1.3f && Cycle(time) < 1.8f;
    }

    public sealed class Level
    {
        public string Name, Subtitle, Hint, Brief;
        public Platform[] Platforms;
        public Danger[] Dangers;
        public Vector2[] Cells;
        public Vector2 Spawn = new Vector2(1.5f, 1.03f);
        public Vector2 Checkpoint = new Vector2(17, 4.72f);
        public Vector2 Terminal = new Vector2(28.7f, 1.5f);
        public Vector2 Exit = new Vector2(30.65f, 1.7f);
        public int Target;
        public Color Accent;
    }

    public static class LevelCatalog
    {
        public const int Count = 3;
        public static Level Create(int index)
        {
            var level = new Level
            {
                Name = new[] { "THE ROOFTOPS", "PULSE WORKS", "THE CORE" }[index],
                Subtitle = new[] { "Find your rhythm.", "Read the light. Time the leap.", "One last relay. Make it count." }[index],
                Brief = new[] { "Double jump between rooftops. Collect all three cells.", "Amber means a laser is about to fire. Cross while it is dark.", "Two laser cycles, a moving ledge, and a final logic lock." }[index],
                Hint = new[] { "Power the two OUTER lines. Keep the middle line off.", "Power the MIDDLE and RIGHT lines. Keep the left line off.", "Power the LEFT and MIDDLE lines. Keep the right line off." }[index],
                Target = new[] { 5, 6, 3 }[index],
                Accent = new[] { new Color(.28f, .94f, .83f), new Color(.68f, .52f, 1), new Color(1, .73f, .34f) }[index],
                Platforms = new[] {
                    new Platform(0, 0, 7.5f, 1), new Platform(10.5f, 0, 8.5f, 1), new Platform(23, 0, 9, 1),
                    new Platform(4, 2.05f, 3.3f, .45f), new Platform(9, 3.25f, 3.1f, .45f, index > 0 ? .7f : 0, .8f),
                    new Platform(14, 4.25f, 4.5f, .45f), new Platform(20, 3.05f, 2.8f, .45f, index == 2 ? .7f : 0, 1.1f),
                    new Platform(25, 2.05f, 3.2f, .45f)
                },
                Cells = new[] { new Vector2(5.6f, 3.2f), new Vector2(15.4f, 5.4f), new Vector2(26.3f, 3.2f) }
            };
            var hazards = new System.Collections.Generic.List<Danger> {
                new Danger(11.9f, 1, 1.6f, .42f), new Danger(23.2f, 1, 1.4f, .42f),
                new Danger(12.8f, 4.45f, .72f, .72f) { Saw = true, Travel = index == 0 ? .6f : 1.15f, Speed = 1.7f }
            };
            if (index > 0) hazards.Add(new Danger(19.25f, 1.0f, .25f, 5.4f) { Laser = true });
            if (index == 2) hazards.Add(new Danger(24.1f, 1.45f, .25f, 4.6f) { Laser = true, Phase = 1.8f });
            level.Dangers = hazards.ToArray();
            return level;
        }
    }
}

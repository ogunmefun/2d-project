using UnityEngine;

namespace NeonRelay
{
    // Fixed-step kinematic simulation: the same code runs in the game and its regression checks.
    // Position is the player's bottom centre; every solid is an axis-aligned rectangle.
    public sealed class GameSimulation
    {
        public const float Step = 1f / 120f, Width = .64f, Height = .92f;
        public Level Level { get; private set; }
        public Vector2 Position, Velocity;
        public bool Grounded { get; private set; }
        public bool CheckpointReached { get; private set; }
        public bool RelayPowered { get; private set; }
        public bool Won { get; private set; }
        public bool[] Collected { get; private set; }
        public int Deaths { get; private set; }
        public int JumpsLeft { get; private set; } = 2;
        public int JumpEvents { get; private set; }
        public int CollectEvents { get; private set; }
        public int SwitchMask { get; private set; }
        public float Time { get; private set; }
        public float Invulnerability { get; private set; }
        public bool FacingRight = true;
        public int CellCount { get { int count = 0; foreach (bool c in Collected) if (c) count++; return count; } }
        public bool CanUseTerminal => Vector2.Distance(Position + Vector2.up * .4f, Level.Terminal) < 1.65f;
        public Rect PlayerBounds => new Rect(Position.x - Width * .5f, Position.y, Width, Height);
        float coyote, jumpBuffer;
        int support = -1;

        public GameSimulation(int levelIndex) { Level = LevelCatalog.Create(levelIndex); Collected = new bool[Level.Cells.Length]; Respawn(); }

        public void Tick(float move, bool jumpPressed, bool jumpHeld)
        {
            if (Won) return;
            float previousTime = Time;
            Time += Step;
            Invulnerability = Mathf.Max(0, Invulnerability - Step);
            if (Grounded && support >= 0)
                Position.x += Level.Platforms[support].At(Time).x - Level.Platforms[support].At(previousTime).x;
            coyote = Grounded ? .11f : Mathf.Max(0, coyote - Step);
            jumpBuffer = jumpPressed ? .13f : Mathf.Max(0, jumpBuffer - Step);
            if (Mathf.Abs(move) > .01f) FacingRight = move > 0;
            Velocity.x = Mathf.MoveTowards(Velocity.x, Mathf.Clamp(move, -1, 1) * 6.3f, (Grounded ? 60 : 42) * Step);
            if (jumpBuffer > 0 && (coyote > 0 || JumpsLeft > 0))
            {
                // Walking off an edge consumes the ground jump after the grace window.
                JumpsLeft = (Grounded || coyote > 0) ? 1 : JumpsLeft - 1;
                Velocity.y = 11.4f;
                Grounded = false; support = -1; coyote = 0; jumpBuffer = 0; JumpEvents++;
            }
            if (!Grounded && coyote <= 0 && JumpsLeft == 2) JumpsLeft = 1;
            if (!jumpHeld && Velocity.y > 5.5f) Velocity.y = 5.5f;
            Velocity.y = Mathf.Max(-20, Velocity.y - 27 * Step);

            Position.x += Velocity.x * Step;
            Position.x = Mathf.Clamp(Position.x, Width * .5f, 32 - Width * .5f);
            foreach (var platform in Level.Platforms)
            {
                Rect solid = platform.At(Time);
                // Mono may retain extra precision in Rect.yMax. Treat a foot touching the
                // top as floor contact, not a side hit that ejects the player sideways.
                if (Position.y >= solid.yMax - .002f || Position.y + Height <= solid.yMin + .002f) continue;
                if (!PlayerBounds.Overlaps(solid)) continue;
                if (Velocity.x > 0) Position.x = solid.xMin - Width * .5f;
                else if (Velocity.x < 0) Position.x = solid.xMax + Width * .5f;
                Velocity.x = 0;
            }
            float oldBottom = Position.y;
            Position.y += Velocity.y * Step;
            Grounded = false; support = -1;
            for (int i = 0; i < Level.Platforms.Length; i++)
            {
                Rect solid = Level.Platforms[i].At(Time);
                if (!PlayerBounds.Overlaps(solid)) continue;
                if (Velocity.y <= 0 && oldBottom >= solid.yMax - .06f)
                { Position.y = solid.yMax; Velocity.y = 0; Grounded = true; JumpsLeft = 2; support = i; }
                else if (Velocity.y > 0)
                { Position.y = solid.yMin - Height; Velocity.y = 0; }
            }

            if (Position.y < -2) { Kill(); return; }
            if (Invulnerability <= 0)
                foreach (var danger in Level.Dangers)
                    if (danger.Active(Time) && PlayerBounds.Overlaps(danger.At(Time))) { Kill(); return; }
            for (int i = 0; i < Collected.Length; i++)
                if (!Collected[i] && Vector2.Distance(Position + Vector2.up * .48f, Level.Cells[i]) < .83f)
                { Collected[i] = true; CollectEvents++; }
            if (!CheckpointReached && Vector2.Distance(Position, Level.Checkpoint) < .85f)
                CheckpointReached = true;
            if (CellCount == Collected.Length && RelayPowered && Vector2.Distance(Position + Vector2.up * .4f, Level.Exit) < 1)
                Won = true;
        }

        public void ToggleSwitch(int index) { if (index >= 0 && index < 3 && !RelayPowered) SwitchMask ^= 1 << index; }
        public bool SubmitRelay()
        {
            if (!CanUseTerminal || SwitchMask != Level.Target) return false;
            RelayPowered = true;
            return true;
        }
        public void Kill() { Deaths++; Respawn(); }
        void Respawn()
        {
            Position = CheckpointReached ? Level.Checkpoint + Vector2.up * .03f : Level.Spawn;
            Velocity = Vector2.zero; Grounded = false; JumpsLeft = 2; support = -1;
            coyote = 0; jumpBuffer = 0; Invulnerability = .75f;
        }
    }
}

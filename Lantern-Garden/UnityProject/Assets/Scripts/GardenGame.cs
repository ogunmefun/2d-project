using System;
using UnityEngine;

namespace LanternGarden
{
    public enum GardenState { Title, Playing, Paused, Help, Won }

    public sealed partial class GardenGame : MonoBehaviour
    {
        public static GardenGame Instance { get; private set; }
        public ExplorerController player;
        public GardenGate gate;
        public CrystalPickup[] crystals;
        public GardenState State { get; private set; } = GardenState.Title;
        public bool IsPlaying => State == GardenState.Playing;
        public int Collected { get; private set; }
        public int Total => crystals.Length;
        public int Rescues { get; private set; }
        public float Elapsed { get; private set; }
        public bool Automated { get; private set; }
        public bool QuickStart { get; private set; }
        public bool Muted { get; private set; }
        public bool Shadows { get; private set; } = true;
        public string Notice { get; private set; }
        public float NoticeTime { get; private set; }
        AudioSource speaker;
        AudioClip pickupClip, jumpClip, openClip;
        GardenState helpReturn;

        void Awake()
        {
            Instance = this;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            string[] args = Environment.GetCommandLineArgs();
            QuickStart = Array.IndexOf(args, "-quickStart") >= 0;
            int smokeIndex = Array.IndexOf(args, "-gardenSmokeTest");
            Automated = smokeIndex >= 0 && smokeIndex + 1 < args.Length;
            Application.runInBackground = Automated;
            Muted = PlayerPrefs.GetInt("LanternGarden.Muted", 0) == 1;
            speaker = gameObject.AddComponent<AudioSource>(); speaker.playOnAwake = false;
            pickupClip = Tone(650, 1100, .18f); jumpClip = Tone(240, 420, .11f); openClip = Tone(330, 880, .7f);
            if (Automated) gameObject.AddComponent<GardenSmokeTest>().OutputDirectory = args[smokeIndex + 1];
        }
        void Start() { gate.SetOpen(false); if (QuickStart) StartGame(); }
        void Update()
        {
            if (IsPlaying) Elapsed += Time.deltaTime;
            NoticeTime = Mathf.Max(0, NoticeTime - Time.unscaledDeltaTime);
            if (Input.GetKeyDown(KeyCode.F11)) ToggleFullscreen();
            if (Input.GetKeyDown(KeyCode.M)) ToggleSound();
            if (Input.GetKeyDown(KeyCode.H)) { if (State == GardenState.Help) CloseHelp(); else if (State != GardenState.Won) OpenHelp(); }
            if (Input.GetKeyDown(KeyCode.Escape)) { if (State == GardenState.Help) CloseHelp(); else TogglePause(); }
            if (Input.GetKeyDown(KeyCode.Return) && (State == GardenState.Title || State == GardenState.Won)) StartGame();
        }
        public void StartGame()
        {
            foreach (var crystal in crystals) crystal.MarkCollected(false);
            Collected = Rescues = 0; Elapsed = 0;
            gate.SetOpen(false); player.ReturnToSpawn(); State = GardenState.Playing;
            ShowNotice("Collect 8 sun crystals. Take your time; the garden is yours.", 6);
        }
        public bool Collect(CrystalPickup crystal)
        {
            if (!IsPlaying || crystal == null || crystal.Collected || Array.IndexOf(crystals, crystal) < 0) return false;
            crystal.MarkCollected(true); Collected++; Play(pickupClip, .35f);
            ShowNotice(crystal.locationName + "  /  " + Collected + " of " + Total + " crystals found");
            if (Collected == Total) { gate.SetOpen(true); Play(openClip, .3f); ShowNotice("The garden is glowing. Walk to the arch to finish!", 8); }
            return true;
        }
        public bool TryFinish()
        {
            if (!IsPlaying || Collected != Total || !gate.IsOpen) return false;
            Vector3 delta = player.transform.position - gate.transform.position; delta.y = 0;
            if (delta.magnitude > 2.25f) return false;
            State = GardenState.Won; Play(openClip, .35f); return true;
        }
        public void RescuePlayer()
        {
            if (!IsPlaying) return;
            player.ReturnToSpawn(); Rescues++; ShowNotice("Back on the path. You kept every crystal — keep exploring.");
        }
        public void TogglePause() { if (IsPlaying) State = GardenState.Paused; else if (State == GardenState.Paused) State = GardenState.Playing; }
        public void OpenHelp() { helpReturn = IsPlaying ? GardenState.Paused : State; State = GardenState.Help; }
        public void CloseHelp() { State = helpReturn; }
        public void ToggleSound() { Muted = !Muted; if (!Automated) { PlayerPrefs.SetInt("LanternGarden.Muted", Muted ? 1 : 0); PlayerPrefs.Save(); } }
        public void ToggleShadows() { Shadows = !Shadows; QualitySettings.shadows = Shadows ? ShadowQuality.All : ShadowQuality.Disable; }
        public void ToggleFullscreen() { Screen.fullScreenMode = Screen.fullScreen ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow; }
        public void ShowNotice(string text, float duration = 4) { Notice = text; NoticeTime = duration; }
        public void PlayJump() { Play(jumpClip, .2f); }
        void Play(AudioClip clip, float volume) { if (!Muted) speaker.PlayOneShot(clip, volume); }
        void OnApplicationFocus(bool focused) { if (!focused && !Automated && IsPlaying) State = GardenState.Paused; }
        void OnDestroy() { if (Instance == this) Instance = null; }
        static AudioClip Tone(float start, float end, float duration)
        {
            const int sampleRate = 22050;
            var samples = new float[(int)(sampleRate * duration)];
            float phase = 0;
            for (int i = 0; i < samples.Length; i++) { float t = i / (float)samples.Length; phase += Mathf.Lerp(start, end, t) * 2 * Mathf.PI / sampleRate; samples[i] = Mathf.Sin(phase) * Mathf.Sin(t * Mathf.PI) * .35f; }
            var clip = AudioClip.Create("Garden tone", samples.Length, 1, sampleRate, false); clip.SetData(samples, 0); return clip;
        }
    }
}

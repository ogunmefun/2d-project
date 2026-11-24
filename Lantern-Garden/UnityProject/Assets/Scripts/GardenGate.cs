using UnityEngine;

namespace LanternGarden
{
    public sealed class GardenGate : MonoBehaviour
    {
        public GameObject seal;
        public Light beacon;
        public Renderer[] runes;
        public Material lockedMaterial, openMaterial;
        public bool IsOpen { get; private set; }
        float nextHint;

        public void SetOpen(bool value)
        {
            IsOpen = value;
            if (seal) seal.SetActive(!value);
            if (beacon) { beacon.color = value ? new Color(.55f, 1, .78f) : new Color(1, .64f, .25f); beacon.intensity = value ? 3 : 1.2f; }
            foreach (var rune in runes) if (rune) rune.sharedMaterial = value ? openMaterial : lockedMaterial;
        }
        void Update()
        {
            var game = GardenGame.Instance;
            if (!game || !game.IsPlaying || !game.player) return;
            Vector3 delta = game.player.transform.position - transform.position; delta.y = 0;
            if (delta.magnitude > 2.2f) return;
            if (IsOpen) game.TryFinish();
            else if (Time.time > nextHint) { game.ShowNotice("Find the remaining crystals to light the garden arch."); nextHint = Time.time + 5; }
        }
    }
}

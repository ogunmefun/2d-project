using UnityEngine;

namespace LanternGarden
{
    public sealed class CrystalPickup : MonoBehaviour
    {
        [Tooltip("Unique number for this crystal in the garden.")] public int crystalId;
        public string locationName = "Garden";
        public Transform gem;
        public GameObject glow;
        public bool Collected { get; private set; }
        Vector3 home;

        void Awake() { home = gem.localPosition; }
        void Update()
        {
            if (Collected || gem == null) return;
            gem.localPosition = home + Vector3.up * (Mathf.Sin(Time.time * 2 + crystalId) * .12f);
            gem.Rotate(Vector3.up, Time.deltaTime * 45, Space.Self);
        }
        void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<ExplorerController>() != null && GardenGame.Instance != null)
                GardenGame.Instance.Collect(this);
        }
        public void MarkCollected(bool value)
        {
            Collected = value;
            if (gem) gem.gameObject.SetActive(!value);
            if (glow) glow.SetActive(!value);
            GetComponent<SphereCollider>().enabled = !value;
        }
    }
}

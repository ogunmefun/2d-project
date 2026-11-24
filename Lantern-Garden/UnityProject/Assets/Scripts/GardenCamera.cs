using UnityEngine;

namespace LanternGarden
{
    public sealed class GardenCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 offset = new Vector3(10, 19, -17);
        Vector3 focus, velocity;
        void Start() { Snap(); }
        public void Snap() { focus = DesiredFocus(); velocity = Vector3.zero; Apply(); }
        Vector3 DesiredFocus()
        {
            if (!target) return Vector3.zero;
            var game = GardenGame.Instance;
            if (game && game.State == GardenState.Title) return new Vector3(1, 0, 1);
            return new Vector3(Mathf.Clamp(target.position.x * .7f, -7, 7), .4f, Mathf.Clamp(target.position.z * .7f, -7, 8));
        }
        void LateUpdate() { focus = Vector3.SmoothDamp(focus, DesiredFocus(), ref velocity, .28f, 100, Time.unscaledDeltaTime); Apply(); }
        void Apply() { transform.position = focus + offset; transform.LookAt(focus); }
    }
}

using UnityEngine;

namespace LanternGarden
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class ExplorerController : MonoBehaviour
    {
        [Header("Beginner-friendly movement")]
        [Tooltip("Walking speed in metres per second.")] public float moveSpeed = 5.5f;
        public float jumpHeight = 1.35f;
        public float gravity = -24f;
        public Transform model;
        public Vector3 Spawn { get; private set; }
        public bool Grounded => controller != null && controller.isGrounded;
        public float VerticalSpeed => verticalSpeed;
        CharacterController controller;
        float verticalSpeed, graceTime, jumpBuffer;
        Vector2 testMove;
        bool testJump, automation;

        void Awake() { controller = GetComponent<CharacterController>(); Spawn = transform.position; }

        void Update()
        {
            var game = GardenGame.Instance;
            if (game == null || !game.IsPlaying) return;
            float dt = Mathf.Min(Time.deltaTime, .05f);
            Vector3 move;
            bool jump;
            if (automation) { move = new Vector3(testMove.x, 0, testMove.y); jump = testJump; testJump = false; }
            else
            {
                float x = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
                float z = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
                Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize();
                Vector3 right = Camera.main.transform.right; right.y = 0; right.Normalize();
                move = forward * z + right * x;
                jump = Input.GetKeyDown(KeyCode.Space);
            }
            move = Vector3.ClampMagnitude(move, 1);
            // Grace time and buffered input forgive slightly early or late jump presses.
            graceTime = Grounded ? .12f : Mathf.Max(0, graceTime - dt);
            jumpBuffer = jump ? .14f : Mathf.Max(0, jumpBuffer - dt);
            if (Grounded && verticalSpeed < 0) verticalSpeed = -2;
            if (jumpBuffer > 0 && graceTime > 0)
            {
                verticalSpeed = Mathf.Sqrt(jumpHeight * -2 * gravity); graceTime = 0; jumpBuffer = 0;
                game.PlayJump();
            }
            verticalSpeed = Mathf.Max(-30, verticalSpeed + gravity * dt);
            controller.Move((move * moveSpeed + Vector3.up * verticalSpeed) * dt);
            if (model && move.sqrMagnitude > .01f) model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(move), dt * 14);
            if (model) model.localPosition = Vector3.up * (Grounded ? Mathf.Sin(Time.time * 11) * move.magnitude * .035f : 0);
            if (transform.position.y < -5) game.RescuePlayer();
        }

        public void ReturnToSpawn() { Teleport(Spawn); }
        public void Teleport(Vector3 position)
        {
            controller.enabled = false; transform.position = position; controller.enabled = true;
            verticalSpeed = graceTime = jumpBuffer = 0; testMove = Vector2.zero; testJump = false;
        }
        // Only the opt-in test runner can activate this input source; movement still uses CharacterController.Move.
        public void SetTestInput(Vector2 direction, bool jump = false)
        {
            if (GardenGame.Instance == null || !GardenGame.Instance.Automated) return;
            automation = true; testMove = Vector2.ClampMagnitude(direction, 1); testJump |= jump;
        }
    }
}

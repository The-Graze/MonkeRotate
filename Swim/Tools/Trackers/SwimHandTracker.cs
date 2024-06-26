using UnityEngine;

namespace MonkeRotate.Tools.Trackers
{
    public class SwimHandTracker : ObjectTracker
    {
        private static Transform bodyOffset = null;

        private Averages.AverageDirection smoothedDirection = Averages.AverageDirection.Zero;

        private InputController controller = null;

        public InputController Controller {
            set { controller = value; }
        }

        public void Awake()
        {
            if(bodyOffset == null) { 
                bodyOffset = GorillaLocomotion.Player.Instance.turnParent.gameObject.transform; 
            }
        }

        public override void OnEnable()
        {
            lastPosition = gameObject.transform.position - bodyOffset.position;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            smoothedDirection = Averages.AverageDirection.Zero;
        }

       public override void LateUpdate()
        {
            // Debug.Log("SwimHandTracker: LateUpdate was called");
            Vector3 currentHandPos = gameObject.transform.position - bodyOffset.position;
            rawDirection = lastPosition - currentHandPos;
            lastPosition = currentHandPos;
            speed = 0f;

            if (controller != null) {
                InputState inputState = controller.PrimaryAction;

               // Debug.Log(string.Format("SwimHandTracker: \nwasPressed: {0} \nisActive: {1}, wasReleased: {2}", inputState.wasPressed, inputState.isActive, inputState.wasReleased));

                if (inputState.wasPressed) {
                    // Debug.Log("SwimHandTracker: TriggerWasPressed");
                    speed = rawDirection.magnitude;
                    speed = speed > 0f ? speed / Time.deltaTime : 0f;
                    smoothedDirection += new Averages.AverageDirection(rawDirection, 0f);
                    direction = rawDirection.normalized;

                    // Debug.Log("SwimHandTracker: Speed: " + speed);

                } else if (inputState.isActive) {
                    // Debug.Log("SwimHandTracker: TriggerIsHeld");
                    speed = rawDirection.magnitude;
                    speed = speed > 0f ? speed / Time.deltaTime : 0f;
                    smoothedDirection += new Averages.AverageDirection(rawDirection * 0.5f, 0f);
                    direction = smoothedDirection.Vector.normalized;

                    // Debug.Log("SwimHandTracker: Speed: " + speed);

                } else if (inputState.wasReleased) {
                    // Debug.Log("TriggerWasReleased");
                    speed = 0f;
                    smoothedDirection = Averages.AverageDirection.Zero;
                    direction = Vector3.zero;
                }
            
            } else {
                // Debug.Log("SwimHandTracker: controller is null, how did this happen?");
            }
        }

    }
}
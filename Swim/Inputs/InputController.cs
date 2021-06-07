using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MonkeSwim.Inputs
{
    public struct InputState
    {
        public InputHelpers.Button button;

        public bool isActive;
        public bool wasPressed;
        public bool wasReleased;

    }

    public struct InputStickState
    {
        public Vector2 state;
        public InputFeatureUsage<Vector2> joyStick;
    }

    public class InputController : XRController
    {
        public InputState PrimaryActionState { get { return firstAction; } private set { } }
        public InputState SecondaryActionState { get { return secondAction; } private set { } }
        public InputState ThirdActionState { get { return thirdAction; } private set { } }
        public InputState FourthActionState { get { return fourthAction; } private set { } }

        public InputHelpers.Button PrimaryActionButton { get { return firstAction.button; } set { firstAction.button = value; } }
        public InputHelpers.Button SecondaryActionButton { get { return secondAction.button; } set { secondAction.button = value; } }
        public InputHelpers.Button ThirdActionButton { get { return thirdAction.button; } set { thirdAction.button = value; } }
        public InputHelpers.Button FourthActionButton { get { return fourthAction.button; } set { fourthAction.button = value; } }
        public InputHelpers.Button FithActionButton { get { return fithAction.button; } set { fithAction.button = value; } }
        public InputHelpers.Button SixthActionButton { get { return sixthAction.button; } set { sixthAction.button = value; } }
        
        private InputState firstAction = new InputState { button = InputHelpers.Button.TriggerPressed, isActive = false, wasPressed = false, wasReleased = false };
        private InputState secondAction = new InputState { button = InputHelpers.Button.GripPressed, isActive = false, wasPressed = false, wasReleased = false };
        private InputState thirdAction = new InputState { button = InputHelpers.Button.PrimaryButton, isActive = false, wasPressed = false, wasReleased = false }; 
        private InputState fourthAction = new InputState { button = InputHelpers.Button.SecondaryButton, isActive = false, wasPressed = false, wasReleased = false };
        private InputState fithAction = new InputState { button = InputHelpers.Button.Primary2DAxisClick, isActive = false, wasPressed = false, wasReleased = false };
        private InputState sixthAction = new InputState { button = InputHelpers.Button.Secondary2DAxisClick, isActive = false, wasPressed = false, wasReleased = false };

        private InputStickState primaryStickAxis = new InputStickState { joyStick = CommonUsages.primary2DAxis, state = Vector2.zero };
        private InputStickState secoundaryStickAxis = new InputStickState { joyStick = CommonUsages.secondary2DAxis, state = Vector2.zero };

        protected override void Awake()
        {
            base.Awake();

            // making sure inputs are enabled so UpdateInput gets called
            base.enableInputActions = true;
            base.enableInputTracking = false;
        }

        protected override void UpdateInput(XRControllerState controllerState)
        {
            // base.UpdateInput(controllerState);
            if (!inputDevice.isValid) return;
            if (!(controllerNode == XRNode.LeftHand || controllerNode == XRNode.RightHand)) return;

            ProccessInputs(firstAction);
            ProccessInputs(secondAction);
            ProccessInputs(thirdAction);
            ProccessInputs(fourthAction);
            ProccessInputs(fithAction);
            ProccessInputs(sixthAction);

            ProccessInputs(primaryStickAxis);
            ProccessInputs(secoundaryStickAxis);
        
        }

        private void ProccessInputs(InputState input)
        {
            // looking at isPRessed in dnspy, it returns false if no input or no device
            bool buttonState = false;
            inputDevice.IsPressed(input.button, out buttonState);

            if (buttonState) {
                if (!input.isActive) input.wasPressed = true;
                else input.wasPressed = false;

            } else if (input.isActive) {
                input.wasReleased = true;
                input.wasPressed = false;

            } else {
                input.wasReleased = false;
                input.wasPressed = false;
            }

            input.isActive = buttonState;
           
        }

        private void ProccessInputs(InputStickState input)
        {
            Vector2 axisValue = Vector2.zero;
            if (!inputDevice.TryGetFeatureValue(input.joyStick, out axisValue)) input.state = Vector2.zero;
        }
    }
}

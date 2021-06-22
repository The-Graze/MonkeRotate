using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MonkeSwim.Utils
{
    public struct InputState
    {
        public bool isActive;
        public bool wasPressed;
        public bool wasReleased;
    }

    public struct InputStateButton
    {
        public InputState state;
        public InputHelpers.Button button;
    }

    public struct InputStateStick
    {
        public Vector2 state;
        public InputFeatureUsage<Vector2> joyStick;
    }

    public class InputController : XRController
    {
        public InputState PrimaryAction { get { return firstAction.state; } private set { } }
        public InputState SecondaryAction { get { return secondAction.state; } private set { } }
        public InputState ThirdAction { get { return thirdAction.state; } private set { } }
        public InputState FourthAction { get { return fourthAction.state; } private set { } }
        public InputState FitfhAction { get { return fithAction.state; } private set { }  }
        public InputState SixthAction { get { return sixthAction.state; } private set { } }

        public Vector2 PrimaryAxis { get { return primaryStickAxis.state; } private set { } }
        public Vector2 SecoundaryAxis { get { return secoundaryStickAxis.state; } private set { } }

        public InputHelpers.Button PrimaryActionButton { get { return firstAction.button; } set { firstAction.button = value; } }
        public InputHelpers.Button SecondaryActionButton { get { return secondAction.button; } set { secondAction.button = value; } }
        public InputHelpers.Button ThirdActionButton { get { return thirdAction.button; } set { thirdAction.button = value; } }
        public InputHelpers.Button FourthActionButton { get { return fourthAction.button; } set { fourthAction.button = value; } }
        public InputHelpers.Button FithActionButton { get { return fithAction.button; } set { fithAction.button = value; } }
        public InputHelpers.Button SixthActionButton { get { return sixthAction.button; } set { sixthAction.button = value; } }
        
        public InputFeatureUsage<Vector2> PrimaryStickAxis { get { return primaryStickAxis.joyStick; } set { primaryStickAxis.joyStick = value; } }
        public InputFeatureUsage<Vector2> SecoundaryStickAxis { get { return secoundaryStickAxis.joyStick; } set { secoundaryStickAxis.joyStick = value; } }

        private InputStateButton firstAction = new InputStateButton { button = InputHelpers.Button.TriggerPressed, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton secondAction = new InputStateButton { button = InputHelpers.Button.GripPressed, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton thirdAction = new InputStateButton { button = InputHelpers.Button.PrimaryButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } }; 
        private InputStateButton fourthAction = new InputStateButton { button = InputHelpers.Button.SecondaryButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton fithAction = new InputStateButton { button = InputHelpers.Button.Primary2DAxisClick, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton sixthAction = new InputStateButton { button = InputHelpers.Button.Secondary2DAxisClick, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };

        private InputStateStick primaryStickAxis = new InputStateStick { joyStick = CommonUsages.primary2DAxis, state = Vector2.zero };
        private InputStateStick secoundaryStickAxis = new InputStateStick { joyStick = CommonUsages.secondary2DAxis, state = Vector2.zero };

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


            // mutable structs are bad mmmkay.. this is supposed to be naughty
            // for this use case though i don't think its bad at all, i want the value type behaviour
            // just interally i need it to act like a reference type when having on function to modify
            // all of them internally, if the function was publicly accessible then doing this is a bad idea
            ProccessInputs(ref firstAction);
            ProccessInputs(ref secondAction);
            ProccessInputs(ref thirdAction);
            ProccessInputs(ref fourthAction);
            ProccessInputs(ref fithAction);
            ProccessInputs(ref sixthAction);

            ProccessInputs(ref primaryStickAxis);
            ProccessInputs(ref secoundaryStickAxis);
        
        }

        private void ProccessInputs(ref InputStateButton input)
        {
            // looking at isPRessed in dnspy, it returns false if no input or no device
            bool buttonState = false;
            inputDevice.IsPressed(input.button, out buttonState);

            if (buttonState) {
                if (!input.state.isActive) input.state.wasPressed = true;
                else input.state.wasPressed = false;

            } else if (input.state.isActive) {
                input.state.wasReleased = true;
                input.state.wasPressed = false;

            } else {
                input.state.wasReleased = false;
                input.state.wasPressed = false;
            }

            input.state.isActive = buttonState;
           
        }

        private void ProccessInputs(ref InputStateStick input)
        {
            Vector2 axisValue = Vector2.zero;
            if (!inputDevice.TryGetFeatureValue(input.joyStick, out axisValue)) input.state = Vector2.zero;
            else input.state = axisValue;
        }
    }
}
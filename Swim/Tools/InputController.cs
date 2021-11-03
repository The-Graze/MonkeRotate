using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace MonkeSwim.Tools
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
        public InputFeatureUsage<bool> button;
    }

    public struct InputStateStick
    {
        public Vector2 state;
        public InputFeatureUsage<Vector2> joyStick;
    }

    public class InputController : MonoBehaviour
    {
        public InputState PrimaryAction { get { return firstAction.state; } private set { } }
        public InputState SecondaryAction { get { return secondAction.state; } private set { } }
        public InputState ThirdAction { get { return thirdAction.state; } private set { } }
        public InputState FourthAction { get { return fourthAction.state; } private set { } }
        public InputState FitfhAction { get { return fithAction.state; } private set { }  }
        public InputState SixthAction { get { return sixthAction.state; } private set { } }

        public Vector2 PrimaryAxis { get { return primaryStickAxis.state; } private set { } }
        public Vector2 SecoundaryAxis { get { return secoundaryStickAxis.state; } private set { } }

        public InputFeatureUsage<bool> PrimaryActionButton { get { return firstAction.button; } set { firstAction.button = value; } }
        public InputFeatureUsage<bool> SecondaryActionButton { get { return secondAction.button; } set { secondAction.button = value; } }
        public InputFeatureUsage<bool> ThirdActionButton { get { return thirdAction.button; } set { thirdAction.button = value; } }
        public InputFeatureUsage<bool> FourthActionButton { get { return fourthAction.button; } set { fourthAction.button = value; } }
        public InputFeatureUsage<bool> FithActionButton { get { return fithAction.button; } set { fithAction.button = value; } }
        public InputFeatureUsage<bool> SixthActionButton { get { return sixthAction.button; } set { sixthAction.button = value; } }
        
        public InputFeatureUsage<Vector2> PrimaryStickAxis { get { return primaryStickAxis.joyStick; } set { primaryStickAxis.joyStick = value; } }
        public InputFeatureUsage<Vector2> SecoundaryStickAxis { get { return secoundaryStickAxis.joyStick; } set { secoundaryStickAxis.joyStick = value; } }

        public XRNode ControllerNode { get { return controllerNode; } set { controllerNode = value; } }
        public InputDevice inputDevice {
            get {
                if (!controller.isValid) {
                    return controller = InputDevices.GetDeviceAtXRNode(controllerNode);
                }

                return controller;
            }
        }

        private InputStateButton firstAction = new InputStateButton { button = CommonUsages.triggerButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton secondAction = new InputStateButton { button = CommonUsages.gripButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton thirdAction = new InputStateButton { button = CommonUsages.primaryButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } }; 
        private InputStateButton fourthAction = new InputStateButton { button = CommonUsages.secondaryButton, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton fithAction = new InputStateButton { button = CommonUsages.primary2DAxisClick, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };
        private InputStateButton sixthAction = new InputStateButton { button = CommonUsages.secondary2DAxisClick, state = new InputState { isActive = false, wasPressed = false, wasReleased = false } };

        private InputStateStick primaryStickAxis = new InputStateStick { joyStick = CommonUsages.primary2DAxis, state = Vector2.zero };
        private InputStateStick secoundaryStickAxis = new InputStateStick { joyStick = CommonUsages.secondary2DAxis, state = Vector2.zero };

        private InputDevice controller;
        private XRNode controllerNode;


        public void Update()
        {
            InputFeatureUsage<bool> usage = CommonUsages.triggerButton;

            // Debug.Log("InputController: UpdateInput was called");
            // Debug.Log("InputController: device name: " + inputDevice.name);
            // base.UpdateInput(controllerState);
            if (!inputDevice.isValid) return;
            if ((controllerNode != XRNode.LeftHand && controllerNode != XRNode.RightHand)) return;


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
            bool buttonState;
            inputDevice.TryGetFeatureValue(input.button, out buttonState);

            if (buttonState) {
                if (!input.state.isActive) {
                    // Debug.Log("InputController: button was pressed");
                    input.state.wasPressed = true;

                } else {
                    // Debug.Log("InputController: button is held");
                    input.state.wasPressed = false;
                }

            } else {
                if (input.state.isActive) {
                    input.state.wasReleased = true;
                    input.state.wasPressed = false;

                } else {
                    input.state.wasReleased = false;
                    input.state.wasPressed = false;
                }
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
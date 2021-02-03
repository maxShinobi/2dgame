using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cuby
{
    public class InputManager : MonoBehaviour
    {
        public Action onJumpPressed;
        public Action onJumpReleased;
        public Action onAttackPressed;
        public Action onAttackReleassed;

        internal bool isJumpPressed;
        internal bool isSprintPressed;
        internal bool isLeftPressed;
        internal bool isRightPressed;

        internal float xAxis; //controller and keyboard left and right movement axis
        internal MobileControls mobileControls;

        void Update()
        {
            //checks both joystick axis, as well as keyboard
            xAxis = Input.GetAxisRaw(Axis.HORIZONTAL);

            if (xAxis < 0 || (mobileControls != null && mobileControls.isLeftBtnDown))
            {
                isLeftPressed = true;
                isRightPressed = false;
            }
            else if (xAxis > 0 || (mobileControls != null && mobileControls.isRightBtnDown))
            {
                isRightPressed = true;
                isLeftPressed = false;
            }
            else
            {
                isLeftPressed = false;
                isRightPressed = false;
            }

            if (Input.GetButtonDown(InputType.JUMP))
            {
                if (onJumpPressed != null)
                {
                    onJumpPressed();
                }
            }

            if (Input.GetButtonUp(InputType.JUMP))
            {
                if (onJumpReleased != null)
                {
                    onJumpReleased();
                }
            }

            if (Input.GetButtonDown(InputType.ATTACK))
            {
                if (onAttackPressed != null)
                {
                    onAttackPressed();
                }
            }

            if (Input.GetButtonUp(InputType.ATTACK))
            {
                if (onAttackPressed != null)
                {
                    onAttackPressed();
                }
            }

            if (Input.GetButton(InputType.JUMP) || (mobileControls != null && mobileControls.isPressed_Jump))
            {
                isJumpPressed = true;
            }
            else
            {
                isJumpPressed = false;
            }

            if (Input.GetButton(InputType.SPRINT) || (mobileControls != null && mobileControls.isPressed_Sprint))
            {
                isSprintPressed = true;
            }
            else
            {
                isSprintPressed = false;
            }
        }
    }
}

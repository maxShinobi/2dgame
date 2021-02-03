using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cuby
{
    public class MobileControls : MonoBehaviour
    {
        [SerializeField] GameObject player = null;

        [SerializeField]
        GameObject buttons = null;

        InputManager playerInputManager = null;

        internal bool isLeftBtnDown;
        internal bool isRightBtnDown;
        internal bool isPressed_Sprint;
        internal bool isPressed_Jump;

        private void Start()
        {
            if (player != null)
            {
                playerInputManager = player.GetComponent<InputManager>();
                playerInputManager.mobileControls = this;
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                buttons.SetActive(false);
            }
        }

        // Mobile Controls 
        public void OnPressLeft()
        {
            isLeftBtnDown = true;
        }

        public void OnReleaseLeft()
        {
            isLeftBtnDown = false;
        }

        public void OnPressRight()
        {
            isRightBtnDown = true;
        }

        public void OnReleaseRight()
        {
            isRightBtnDown = false;
        }

        public void OnPress_B()
        {
            isPressed_Sprint = true;
        }

        public void OnRelease_B()
        {
            isPressed_Sprint = false;
        }

        public void OnPress_Jump()
        {
            isPressed_Jump = true;
            if (playerInputManager != null)
            {
                playerInputManager.onJumpPressed();
            }
        }

        public void OnRelease_Jump()
        {
            isPressed_Jump = false;
            if (playerInputManager != null)
            {
                playerInputManager.onJumpReleased();
            }
        }

    }
}

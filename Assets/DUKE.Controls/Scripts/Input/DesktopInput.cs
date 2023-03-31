/******************************************************************************
 * File        : DesktopInput.cs
 * Version     : 1.0
 * Author      : Miika Puljujärvi (miika.puljujarvi@lapinamk.fi)
 * Copyright   : Lapland University of Applied Sciences
 * Licence     : MIT-Licence
 * 
 * Copyright (c) 2022 Lapland University of Applied Sciences
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 *****************************************************************************/


using System;
using UnityEngine;
using UnityEngine.InputSystem;


namespace DUKE.Controls {

    /// <summary>
    /// Input modes for desktop.
    /// </summary>
    public enum DesktopInputMode {
        Default,
        AlphanumericInput
    }

    /// <summary>
    /// Interaction modifiers for desktop.
    /// </summary>
    [System.Flags]
    public enum DesktopInteractionModifier {
        LeftShift = (1 << 0),
        LeftControl = (1 << 1),
        LeftAlt = (1 << 2)
    }



    /// <summary>
    /// Attached to a player object in hierarchy.
    /// DesktopInput receives input data through InputActions and converts them into Commands. 
    /// Commands are then sent forward and processed or discarded depending on the target object.
    /// Use VRInput for VR functionality.
    /// </summary>
    public class DesktopInput : Input {

        #region Variables

        #region General Variables

        /// <summary>
        /// <typeparamref name="InputActionAsset"/> containing the input.
        /// </summary>
        public InputActionAsset inputActions;

        /// <summary>
        /// TRUE if required references have been set.
        /// </summary>
        public bool referencesSet = false;


        [Space(10f)]
        [Header("Raycast Settings")]
        //RaycastHit rHit = new RaycastHit();
        Transform mousePoint;
        Transform mpVisual;


        /// <summary>
        /// TRUE if debug gizmos should be drawn.
        /// </summary>
        [Space(10f)]
        [Header("Debug Settings")]
        public bool drawGizmos = false;


        [SerializeField] DesktopInputMode inputMode;
        [SerializeField] Camera cam;

        #endregion
        #region Mouse InputAction Variables

        InputAction mousePosition;
        InputAction mouseX;
        InputAction mouseY;
        InputAction rmb;
        InputAction lmb;
        InputAction mmb;
        InputAction scrollWheel;

        #endregion
        #region Keyboard InputAction Variables

        InputAction kbMovement;
        InputAction kbVertical;
        InputAction kbConfirm;
        InputAction kbEsc;
        InputAction kbLShift;
        InputAction kbLCtrl;
        InputAction kbLAlt;


        #endregion
        #region Click Methods
      
        bool lmbActive = false;
        bool rmbActive = false;
        bool prevLMBHeld = false;
        bool prevRMBHeld = false;

        #endregion
        
        #endregion


        #region Properties

        /// <summary>
        /// <typeparamref name="Transform"/> of the object being currently pointed at through <paramref name="PointerRay"/>.
        /// </summary>
        public override Transform PointerTarget { 
            get { return rHit.transform; } }

        /// <summary>
        /// <typeparamref name="Camera"/> through which the player sees.
        /// </summary>
        public Camera Camera { 
            get { return cam; } }

        /// <summary>
        /// <typeparamref name="Ray"/> through which <typeparamref name="Interactables"/> are detected.
        /// </summary>
        public override Ray PointerRay { 
            get { return RMBHeld ? cam.ScreenPointToRay(SavedMousePosition) : cam.ScreenPointToRay(MousePosition); } }

        /// <summary>
        /// <typeparamref name="Hit"/> information of <paramref name="PointerRay"/>.
        /// </summary>
        public override RaycastHit Hit { 
            get { return rHit; } }

        /// <summary>
        /// Pixel coordinate position of the cursor.
        /// </summary>
        public Vector2 MousePosition { 
            get; 
            private set; }

        /// <summary>
        /// Horizontal movement input.
        /// </summary>
        public Vector2 HorizontalKeyboardMovement {
            get; 
            private set; }

        /// <summary>
        /// Vertical movement input.
        /// </summary>
        public float KeyboardVerticalMovement { 
            get; 
            private set; }

        /// <summary>
        /// World coordinate vector of the current movement input.
        /// </summary>
        public Vector3 KeyboardWorldMovement { 
            get { return new Vector3(HorizontalKeyboardMovement.x, KeyboardVerticalMovement, HorizontalKeyboardMovement.y); } }

        /// <summary>
        /// Recorded pixel coordinate position of the cursor.
        /// </summary>
        public Vector2 SavedMousePosition { 
            get; 
            private set; }

        /// <summary>
        /// World position of mousePoint transform.
        /// </summary>
        public Vector3 MousePointPosition { 
            get { return mousePoint.position; } }

        /// <summary>
        /// TRUE if the primary interaction button is held.
        /// </summary>
        public override bool PrimaryInteractionHeld { 
            get { return LMBHeld; } }

        /// <summary>
        /// TRUE if the cursor's coordinate position is locked.
        /// </summary>
        public bool MousePositionLocked { 
            get; 
            private set; }

        /// <summary>
        /// Most recent recorded depth of the cursor.
        /// </summary>
        public float LastKnownMouseDepth { 
            get; 
            private set; }

        /// <summary>
        /// Current delta of the cursor's X-axis.
        /// </summary>
        public float MouseX { 
            get; 
            private set; }

        /// <summary>
        /// Current delta of the cursor's Y-axis.
        /// </summary>
        public float MouseY { 
            get; 
            private set; }

        /// <summary>
        /// Current value of the scroll wheel.
        /// </summary>
        /// <value></value>
        public int MouseWheel {
            get; 
            private set; }

        /// <summary>
        /// TRUE when right mouse button is being held.
        /// </summary>
        public bool RMBHeld { 
            get; 
            private set; }

        /// <summary>
        /// TRUE when left mouse button is being held.
        /// </summary>
        public bool LMBHeld { 
            get; 
            private set; }

        /// <summary>
        /// TRUE when middle mouse button is being held.
        /// </summary>
        public bool MMBHeld { 
            get; 
            private set; }

        /// <summary>
        /// TRUE when confirm button is being held.
        /// </summary>
        public bool ConfirmPressed { 
            get; 
            private set; }
        
        /// <summary>
        /// TRUE when escape button is being held.
        /// </summary>
        public bool EscPressed { 
            get; 
            private set; }
        
        /// <summary>
        /// TRUE when left shift button is being held.
        /// </summary>
        public bool LeftShiftPressed { 
            get; 
            private set; }
        
        /// <summary>
        /// TRUE when left control button is being held.
        /// </summary>
        public bool LeftCtrlPressed { 
            get; 
            private set; }
        
        /// <summary>
        /// TRUE when left alt button is being held.
        /// </summary>
        public bool LeftAltPressed { 
            get; 
            private set; }

        #endregion


        #region Events

        /// <summary>Called when left mouse button is pressed down.</summary>
        public Action<InputContext> OnLMBDown;

        /// <summary>Called when left mouse button is released.</summary>
        public Action<InputContext> OnLMBUp;

        /// <summary>Called when right mouse button is pressed down.</summary>
        public Action<InputContext> OnRMBDown;
        
        /// <summary>Called when right mouse button is released.</summary>
        public Action<InputContext> OnRMBUp;
        
        /// <summary>Called when middle mouse button is pressed down.</summary>
        public Action<InputContext> OnMMBDown;
        
        /// <summary>Called when middle mouse button is released.</summary>
        public Action<InputContext> OnMMBUp;
        
        /// <summary>Called when confirm button is pressed down.</summary>
        public Action<InputContext> OnConfirmDown;
        
        /// <summary>Called when confirm button is released.</summary>
        public Action<InputContext> OnConfirmUp;
        
        /// <summary>Called when escape button is pressed down.</summary>
        public Action<InputContext> OnEscDown;
        
        /// <summary>Called when escape button is released.</summary>
        public Action<InputContext> OnEscUp;
        
        /// <summary>Called when left shift button is pressed down.</summary>
        public Action<InputContext> OnLeftShiftDown;
        
        /// <summary>Called when left shift button is released.</summary>
        public Action<InputContext> OnLeftShiftUp;
        
        /// <summary>Called when left control button is pressed down.</summary>
        public Action<InputContext> OnLeftCtrlDown;
        
        /// <summary>Called when left control button is released.</summary>
        public Action<InputContext> OnLeftCtrlUp;
        
        /// <summary>Called when left alt button is pressed down.</summary>
        public Action<InputContext> OnLeftAltDown;
        
        /// <summary>Called when left alt button is released.</summary>
        public Action<InputContext> OnLeftAltUp;

        #endregion


        #region Methods

        #region Public Methods

        /// <summary>
        /// Control the input by blocking movement etc. when DesktopInputMode is set to Alphanumeric.
        /// </summary>
        /// <param name="_mode"></param>
        public void SetMode (DesktopInputMode _mode)
        {
            inputMode = _mode;
        }

        /// <summary>
        /// Set this <typeparamref name="DesktopInput"/> inactive.
        /// </summary>
        public override void DisableInput ()
        {
            base.DisableInput();

            HorizontalKeyboardMovement = Vector2.zero;
            KeyboardVerticalMovement = 0f;
        }

        #endregion
        #region MonoBehaviour Methods

        void Awake ()
        {
            FindReferences();
        }

        void Start ()
        {
            EnableInput();
        }

        protected virtual void Update ()
        {
            if (!InputActive) { return; }

            InteractionMode = InteractionMode.Raycast;

            PointerRaycast();
            UpdateMousePoint();

            TargetTransform = Hit.transform;

            /// Left Mouse Button
            LMBHeld = lmbActive;
            if (lmbActive) 
            {
                if (!prevLMBHeld) { OnLMBPressed(); }

                OnLMBHeld();      
            } 
            else if (prevLMBHeld) 
            { 
                OnLMBReleased(); 
            }

            /// Right Mouse Button
            RMBHeld = rmbActive;
            if (rmbActive) 
            {
                if (!prevRMBHeld) { OnRMBPressed(); }

                OnRMBHeld();       
            } 
            else if (prevRMBHeld) 
            { 
                OnRMBReleased(); 
            }

            mpVisual.gameObject.SetActive(interactionLock);
            mpVisual.localScale = (1 + (Vector3.Distance(transform.position, mousePoint.position) / 5)) * 0.015f * Vector3.one;
        }

        protected override void OnEnable ()
        {
            inputActions.Enable();
            base.OnEnable();
        }
        
        protected override void OnDisable ()
        {
            base.OnDisable();
            inputActions.Disable();
        }
        
        void OnDrawGizmos ()
        {
            if (!drawGizmos) return;

            if (null == mousePoint) mousePoint = transform.Find("MousePoint");

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(MousePointPosition, 0.25f);


            Gizmos.color = Color.green;
            Gizmos.DrawRay(PointerRay);

        }

        #endregion
        #region Event Handling Methods

        /// <summary>
        /// Find references to input events.
        /// </summary>
        void FindReferences ()
        {
            /// Mouse InputActions
            mousePosition = inputActions.actionMaps[0].FindAction("MousePosition");
            mouseX = inputActions.actionMaps[0].FindAction("MouseX");
            mouseY = inputActions.actionMaps[0].FindAction("MouseY");
            rmb = inputActions.actionMaps[0].FindAction("RMB");
            lmb = inputActions.actionMaps[0].FindAction("LMB");
            mmb = inputActions.actionMaps[0].FindAction("MMB");
            scrollWheel = inputActions.actionMaps[0].FindAction("ScrollWheel");

            /// Keyboard InputActions
            kbMovement = inputActions.actionMaps[1].FindAction("Movement");
            kbVertical = inputActions.actionMaps[1].FindAction("VerticalMovement");
            kbConfirm = inputActions.actionMaps[1].FindAction("Confirm");
            kbEsc = inputActions.actionMaps[1].FindAction("Escape");
            kbLShift = inputActions.actionMaps[1].FindAction("LeftShift");
            kbLCtrl = inputActions.actionMaps[1].FindAction("LeftCtrl");
            kbLAlt = inputActions.actionMaps[1].FindAction("LeftAlt");

            mousePoint = transform.Find("MousePoint");
            mpVisual = mousePoint.GetChild(0);

            referencesSet = true;
        }
        
        /// <summary>
        /// Subscribe to input events.
        /// </summary>
        protected override void SubscribeToInput ()
        {
            if (!referencesSet) FindReferences();

            mousePosition.performed += UpdateMousePosition;
            mouseX.performed += UpdateMouseXY;
            mouseX.canceled += UpdateMouseXY;
            mouseY.performed += UpdateMouseXY;
            mouseY.canceled += UpdateMouseXY;

            rmb.started += RMBState;
            rmb.canceled += RMBState;
            lmb.started += LMBState;
            lmb.canceled += LMBState;
            mmb.started += MMBState;
            mmb.canceled += MMBState;
            scrollWheel.started += UpdateScrollWheel;
            scrollWheel.canceled += UpdateScrollWheel;

            kbMovement.performed += UpdateKeyboardMovement;
            kbVertical.performed += UpdateKeyboardVerticalMovement;
            kbConfirm.started += ConfirmState;
            kbConfirm.canceled += ConfirmState;
            kbEsc.started += EscState;
            kbEsc.canceled += EscState;
            kbLShift.started += LeftShiftState;
            kbLShift.canceled += LeftShiftState;
            kbLCtrl.started += LeftControlState;
            kbLCtrl.canceled += LeftControlState;
            kbLAlt.started += LeftAltState;
            kbLAlt.canceled += LeftAltState;
        }
        
        /// <summary>
        /// Unsubscribe from input events.
        /// </summary>
        protected override void UnsubscribeFromInput ()
        {
            mousePosition.performed -= UpdateMousePosition;
            mouseX.performed -= UpdateMouseXY;
            mouseX.canceled -= UpdateMouseXY;
            mouseY.performed -= UpdateMouseXY;
            mouseY.canceled -= UpdateMouseXY;

            rmb.started -= RMBState;
            rmb.canceled -= RMBState;
            lmb.started -= LMBState;
            lmb.canceled -= LMBState;
            mmb.started -= MMBState;
            mmb.canceled -= MMBState;
            scrollWheel.started -= UpdateScrollWheel;
            scrollWheel.canceled -= UpdateScrollWheel;

            kbMovement.performed -= UpdateKeyboardMovement;
            kbVertical.performed -= UpdateKeyboardVerticalMovement;
            kbConfirm.started -= ConfirmState;
            kbConfirm.canceled -= ConfirmState;
            kbEsc.started -= EscState;
            kbEsc.canceled -= EscState;
            kbLShift.started -= LeftShiftState;
            kbLShift.canceled -= LeftShiftState;
            kbLCtrl.started -= LeftControlState;
            kbLCtrl.canceled -= LeftControlState;
            kbLAlt.started -= LeftAltState;
            kbLAlt.canceled -= LeftAltState;
        }


        
        /// <summary>
        /// Called when mousePosition is performed.
        /// </summary>
        /// <param name="_context"></param>
        void UpdateMousePosition (InputAction.CallbackContext _context)
        {
            MousePosition = rmbActive ? SavedMousePosition : ClampVector2ToScreen(mousePosition.ReadValue<Vector2>());
        }
        
        /// <summary>
        /// Called when mouseX or mouseY is changed.
        /// </summary>
        /// <param name="_context"></param>
        void UpdateMouseXY (InputAction.CallbackContext _context)
        {
            if (_context.action == mouseX) {

                if (_context.phase == InputActionPhase.Canceled) MouseX = 0;
                else MouseX = _context.ReadValue<float>();

            } else {

                if (_context.phase == InputActionPhase.Canceled) MouseY = 0;
                else MouseY = _context.ReadValue<float>();
            }

            //OnMousePositionChange();
        }
        
        /// <summary>
        /// Called when rmb is started or canceled.
        /// </summary>
        /// <param name="_context"></param>
        void RMBState (InputAction.CallbackContext _context)
        {
            if (_context.phase == InputActionPhase.Started) { 

                rmbActive = true;
                OnRMBDown?.Invoke(GetDefaultContext(InputContextState.Start));

            }  else if (_context.phase == InputActionPhase.Canceled) { 
                
                rmbActive = false;
                OnRMBUp?.Invoke(GetDefaultContext(InputContextState.End));
            }
        }
        
        /// <summary>
        /// Called when lmb is started or canceled.
        /// </summary>
        /// <param name="_context"></param>
        void LMBState (InputAction.CallbackContext _context)
        {
            if (_context.phase == InputActionPhase.Started) { 

                lmbActive = true;
                LMBHeld = true;
                OnLMBDown?.Invoke(GetDefaultContext(InputContextState.Start));

            }  else if (_context.phase == InputActionPhase.Canceled) { 
                
                lmbActive = false;
                OnLMBUp?.Invoke(GetDefaultContext(InputContextState.End));
            }
        }
        
        /// <summary>
        /// Called when mmb is started or canceled.
        /// </summary>
        /// <param name="_context"></param>
        void MMBState (InputAction.CallbackContext _context)
        {
            if (_context.phase == InputActionPhase.Started) { 
                
                MMBHeld = true;
                OnMMBDown?.Invoke(GetDefaultContext(InputContextState.Start));
            
            } else if (_context.phase == InputActionPhase.Canceled) { 
                
                MMBHeld = false;
                OnMMBUp?.Invoke(GetDefaultContext(InputContextState.End));           
            }
        }
        
        /// <summary>
        /// Called when scrollWheel is performed.
        /// </summary>
        /// <param name="_context"></param>
        void UpdateScrollWheel (InputAction.CallbackContext _context)
        {
            MouseWheel = (int)(_context.ReadValue<float>() / 120f);
        }
        
        /// <summary>
        /// Called when kbMovement is performed and 'inputMode' is Default.
        /// </summary>
        /// <param name="_context"></param>
        void UpdateKeyboardMovement (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) { HorizontalKeyboardMovement = kbMovement.ReadValue<Vector2>(); }
        }
        
        /// <summary>
        /// Called when kbVertical is performed and 'inputMode' is Default.
        /// </summary>
        /// <param name="_context"></param>
        void UpdateKeyboardVerticalMovement (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) { KeyboardVerticalMovement = kbVertical.ReadValue<Vector2>().x; }
        }
        
        /// <summary>
        /// Called when kbConfirm is started.
        /// </summary>
        /// <param name="_context"></param>
        void ConfirmState (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) {

                if (_context.started) { 
                    
                    ConfirmPressed = true;
                    OnConfirmDown?.Invoke(GetDefaultContext(InputContextState.Start));

                } else if (_context.canceled) { 
                    
                    ConfirmPressed = false;
                    OnConfirmUp?.Invoke(GetDefaultContext(InputContextState.End));
                }
            }
        }
        /// <summary>
        /// Called when kbEsc is started.
        /// </summary>
        /// <param name="_context"></param>
        void EscState (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) {

                if (_context.started) { 
                    
                    EscPressed = true;
                    OnEscDown?.Invoke(GetDefaultContext(InputContextState.Start));

                } else if (_context.canceled) { 
                    
                    EscPressed = false;
                    OnEscUp?.Invoke(GetDefaultContext(InputContextState.End));
                }
            }
        }
        /// <summary>
        /// Called when kbLShift is started.
        /// </summary>
        /// <param name="_context"></param>
        void LeftShiftState (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) {

                if (_context.started) { 
                    
                    LeftShiftPressed = true;
                    OnLeftShiftDown?.Invoke(GetDefaultContext(InputContextState.Start));

                } else if (_context.canceled) { 
                    
                    LeftShiftPressed = false;
                    OnLeftShiftUp?.Invoke(GetDefaultContext(InputContextState.End));
                }
            }
        }
        /// <summary>
        /// Called when kbLCtrl is started.
        /// </summary>
        /// <param name="_context"></param>
        void LeftControlState (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) 
            {
                if (_context.started) 
                {  
                    LeftCtrlPressed = true;
                    OnLeftCtrlDown?.Invoke(GetDefaultContext(InputContextState.Start));
                } 
                else if (_context.canceled) 
                { 
                    LeftCtrlPressed = false;
                    OnLeftCtrlUp?.Invoke(GetDefaultContext(InputContextState.End));
                }
            }
        }
        /// <summary>
        /// Called when kbLAlt is started.
        /// </summary>
        /// <param name="_context"></param>
        void LeftAltState (InputAction.CallbackContext _context)
        {
            if (inputMode == DesktopInputMode.Default) 
            {
                if (_context.started) 
                {   
                    LeftAltPressed = true;
                    OnLeftAltDown?.Invoke(GetDefaultContext(InputContextState.Start));
                } 
                else if (_context.canceled) 
                {        
                    LeftAltPressed = false;
                    OnLeftAltUp?.Invoke(GetDefaultContext(InputContextState.End));
                }
            }
        }



        /// <summary>
        /// Update the position of MousePoint.
        /// </summary>
        void UpdateMousePoint ()
        {
            if (null != Hit.transform && !interactionLock) 
            { 
                LastKnownMouseDepth = Hit.distance;
                 mousePoint.position = Hit.point; 
            }
            else 
            { 
                mousePoint.position = PointerRay.origin + PointerRay.direction * LastKnownMouseDepth; 
            }

            mousePoint.LookAt(mousePoint.position + PointerRay.direction, cam.transform.up);
        }
        
        /// <summary>
        /// Called on Update when LMBHeld is first set to true.
        /// </summary>
        void OnLMBPressed ()
        {
            prevLMBHeld = true;
            Cursor.visible = false;
        }
        
        /// <summary>
        /// Called on Update-loop when LMBHeld is true.
        /// </summary>
        void OnLMBHeld ()
        {

        }
        
        /// <summary>
        /// Called on Update when LMBHeld is first set to false.
        /// </summary>
        void OnLMBReleased ()
        {
            prevLMBHeld = false;

            if (!RMBHeld) Cursor.visible = true;          
        }
        
        /// <summary>
        /// Called on Update when RMBHeld is first set to true.
        /// </summary>
        void OnRMBPressed ()
        {
            prevRMBHeld = true;

            Cursor.visible = false;

            SavedMousePosition = MousePosition;
            MousePositionLocked = true;
        }
        
        /// <summary>
        /// Called on Update-loop when RMBHeld is true.
        /// </summary>
        void OnRMBHeld ()
        {
           
        }
        
        /// <summary>
        /// Called on Update when RMBHeld is first set to false.
        /// </summary>
        void OnRMBReleased ()
        {
            prevRMBHeld = false;

            if (!LMBHeld) { Cursor.visible = true; }

            MousePosition = SavedMousePosition;
            Mouse.current.WarpCursorPosition(SavedMousePosition);
            MousePositionLocked = false;
        }

        #endregion
        #region Raycast and Spherecast Methods

        /// <summary>
        /// Clamp a Vector2 to stay within the limits of the screen.
        /// </summary>
        /// <param name="_vector"></param>
        /// <returns></returns>
        protected Vector2 ClampVector2ToScreen (Vector2 _vector)
        {
            float x = Mathf.Clamp(_vector.x, 0, Screen.width);
            float y = Mathf.Clamp(_vector.y, 0, Screen.height);
            return new Vector2(x, y);
        }

        #endregion
        
        #endregion
        

    } /// End of Class


} /// End of Namespace
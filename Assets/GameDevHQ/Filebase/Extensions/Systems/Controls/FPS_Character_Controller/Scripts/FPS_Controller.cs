using System.Collections;
using System.Collections.Generic;
using Unity.Netcode; // Multiplayer
using UnityEngine;
using UnityEngine.InputSystem; // Input Code

namespace GameDevHQ.FileBase.Plugins.FPS_Character_Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class FPS_Controller : NetworkBehaviour // Multiplayer
    {
        // Input Code
        [Header("Input")]
        [SerializeField]
        private InputActionAsset _inputActions;

        private InputActionMap _gameplayMap;

        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;
        private InputAction _height1Action;
        private InputAction _height2Action;
        // Input Code



        [Header("Controller Info")]
        [SerializeField ][Tooltip("How fast can the controller walk?")]
        private float _walkSpeed = 3.0f; //how fast the character is walking
        [SerializeField][Tooltip("How fast can the controller run?")]
        private float _runSpeed = 7.0f; // how fast the character is running
        [SerializeField][Tooltip("Set your gravity multiplier")] 
        private float _gravity = 1.0f; //how much gravity to apply 
        [SerializeField][Tooltip("How high can the controller jump?")]
        private float _jumpHeight = 15.0f; //how high can the character jump
        [SerializeField]
        private bool _crouching = false; //bool to display if we are crouched or not

        private CharacterController _controller; //reference variable to the character controller component
        [SerializeField]
        private float _yVelocity = 0.0f; //cache our y velocity
        

        [Header("Headbob Settings")]       
        [SerializeField][Tooltip("Smooth out the transition from moving to not moving")]
        private float _smooth = 20.0f; //smooth out the transition from moving to not moving
        [SerializeField][Tooltip("How quickly the player head bobs")]
        private float _walkFrequency = 4.8f; //how quickly the player head bobs when walking
        [SerializeField][Tooltip("How quickly the player head bobs")]
        private float _runFrequency = 7.8f; //how quickly the player head bobs when running
        [SerializeField][Tooltip("How dramatic the headbob is")][Range(0.0f, 0.2f)]
        private float _heightOffset = 0.05f; //how dramatic the bobbing is
        private float _timer = Mathf.PI / 2; //This is where Sin = 1 -- used to simulate walking forward. 
        private Vector3 _initialCameraPos; //local position where we reset the camera when it's not bobbing

        [Header("Camera Settings")]
        [SerializeField][Tooltip("Control the look sensitivty of the camera")]
        private float _lookSensitivity = 5.0f; //mouse sensitivity 

        [SerializeField]
        private bool _grounded = false; // grounded

        private Camera _fpsCamera;

        private void Awake()
        {
            Debug.Log(
                $"PLAYER CREATED: {gameObject.name} | " +
                $"IsLocalPlayer: {IsLocalPlayer} | " +
                $"IsOwner: {IsOwner}"
            );
        }

        
        // New Input Code
        private void Start()
        {
            _controller = GetComponent<CharacterController>();

            if (!IsOwner)
                return;

            InitializeInput();

            // Reinitialize the CharacterController at the
            // Player's networked spawn position.
            _controller.enabled = false;
            _controller.enabled = true;

            InputManager.Instance.EnterGameplayMode(); // Added for new Input System

            _fpsCamera = GetComponentInChildren<Camera>();

            if (_fpsCamera == null)
            {
                Debug.LogError(
                    $"FPS_Controller on {gameObject.name}: " +
                    "No Camera was found in the Player hierarchy."
                );

                return;
            }

            _initialCameraPos = _fpsCamera.transform.localPosition;
        }

        private void InitializeInput()
        {
            if (_inputActions == null)
            {
                Debug.LogError(
                    $"FPS_Controller on {gameObject.name}: " +
                    "No Input Action Asset has been assigned."
                );

                return;
            }

            _gameplayMap = _inputActions.FindActionMap("PlayerMovement");

            if (_gameplayMap == null)
            {
                Debug.LogError(
                    "FPS_Controller: Could not find the 'Gameplay' Action Map."
                );

                return;
            }
            //_gameplayMap.Enable;
            _gameplayMap?.Enable();

            _moveAction = _gameplayMap.FindAction("Movement");
            _lookAction = _gameplayMap.FindAction("Look");
            _jumpAction = _gameplayMap.FindAction("Jump");
            _sprintAction = _gameplayMap.FindAction("Sprint");
            _crouchAction = _gameplayMap.FindAction("Crouch");
            _height1Action = _gameplayMap.FindAction("Height1");
            _height2Action = _gameplayMap.FindAction("Height2");

            if (_moveAction == null)
                Debug.LogError("FPS_Controller: 'Move' action not found.");

            if (_lookAction == null)
                Debug.LogError("FPS_Controller: 'Look' action not found.");

            if (_jumpAction == null)
                Debug.LogError("FPS_Controller: 'Jump' action not found.");

            if (_sprintAction == null)
                Debug.LogError("FPS_Controller: 'Sprint' action not found.");

            if (_crouchAction == null)
                Debug.LogError("FPS_Controller: 'Crouch' action not found.");

            if (_height1Action == null)
                Debug.LogError("FPS_Controller: 'Height1' action not found.");

            if (_height2Action == null)
                Debug.LogError("FPS_Controller: 'Height2' action not found.");
        }

        private void Update()
        {
            if (_controller == null)
                return;

            _grounded = _controller.isGrounded;

            if (!IsOwner)
                return;

            FPSController();
            CameraController();
            HeadBobbing();
            HandleHeightChanges();
        }

        private void FPSController()
        {
            if (_moveAction == null)
                return;

            Vector2 input = _moveAction.ReadValue<Vector2>();

            Vector3 direction = new Vector3(
                input.x,
                0f,
                input.y
            );

            Vector3 velocity = direction * _walkSpeed;

            HandleCrouching();

            if (_sprintAction != null &&
                _sprintAction.IsPressed() &&
                !_crouching)
            {
                velocity = direction * _runSpeed;
            }

            if (_controller.isGrounded)
            {
                if (_yVelocity < 0f)
                {
                    _yVelocity = -2f;
                }

                if (_jumpAction != null &&
                    _jumpAction.WasPressedThisFrame())
                {
                    _yVelocity = _jumpHeight;
                }
            }
            else
            {
                _yVelocity -= _gravity;
            }

            velocity.y = _yVelocity;

            velocity = transform.TransformDirection(velocity);

            _controller.Move(
                velocity * Time.deltaTime
            );
        }

        private void HandleCrouching()
        {
            if (_crouchAction == null)
                return;

            if (_crouchAction.WasPressedThisFrame())
            {
                _crouching = !_crouching;

                if (_crouching)
                {
                    _controller.height = 2.0f;
                }
                else
                {
                    _controller.height = 1.0f;
                }
            }
        }

        private void HandleHeightChanges()
        {
            if (_height1Action != null &&
                _height1Action.WasPressedThisFrame())
            {
                _controller.height = 2.0f;
            }
            else if (_height2Action != null &&
                     _height2Action.WasPressedThisFrame())
            {
                _controller.height = 1.0f;
            }
        }

        private void CameraController()
        {
            if (_lookAction == null || _fpsCamera == null)
                return;

            Vector2 look = _lookAction.ReadValue<Vector2>();

            float mouseX = look.x;
            float mouseY = look.y;

            Vector3 rot = transform.localEulerAngles;

            rot.y += mouseX * _lookSensitivity;

            transform.localRotation =
                Quaternion.AngleAxis(
                    rot.y,
                    Vector3.up
                );

            Vector3 camRot =
                _fpsCamera.transform.localEulerAngles;

            camRot.x += -mouseY * _lookSensitivity;

            _fpsCamera.transform.localRotation =
                Quaternion.AngleAxis(
                    camRot.x,
                    Vector3.right
                );
        }

        private void HeadBobbing()
        {
            if (_moveAction == null || _fpsCamera == null)
                return;

            Vector2 movement = _moveAction.ReadValue<Vector2>();

            if (movement != Vector2.zero)
            {
                if (_sprintAction != null &&
                    _sprintAction.IsPressed() &&
                    !_crouching)
                {
                    _timer +=
                        _runFrequency *
                        Time.deltaTime;
                }
                else
                {
                    _timer +=
                        _walkFrequency *
                        Time.deltaTime;
                }

                Vector3 headPosition = new Vector3
                (
                    _initialCameraPos.x +
                    Mathf.Cos(_timer) *
                    _heightOffset,

                    _initialCameraPos.y +
                    Mathf.Sin(_timer) *
                    _heightOffset,

                    _initialCameraPos.z
                );

                _fpsCamera.transform.localPosition =
                    headPosition;

                if (_timer > Mathf.PI * 2)
                {
                    _timer = 0;
                }
            }
            else
            {
                _timer = Mathf.PI / 2;

                Vector3 resetHead = new Vector3
                (
                    Mathf.Lerp(
                        _fpsCamera.transform.localPosition.x,
                        _initialCameraPos.x,
                        _smooth * Time.deltaTime
                    ),

                    Mathf.Lerp(
                        _fpsCamera.transform.localPosition.y,
                        _initialCameraPos.y,
                        _smooth * Time.deltaTime
                    ),

                    _initialCameraPos.z
                );

                _fpsCamera.transform.localPosition =
                    resetHead;
            }
        }
        // Input Code

        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestDeathRpc() // FPS_Controller Script; same Script that controls Player movement
        {
            Debug.Log($"RequestDeathRpc fired on Server for Player {OwnerClientId}");

            ulong clientId = OwnerClientId;

            Debug.Log($"Server received death request from Player {clientId}");

            // Tell the Client that owns this PlayerObject to show the death UI.
            ShowDeathUIRpc();

            // Server handles the actual network despawn.
            NetworkObject.Despawn();

            SpectatorSystem spectatorSystem = FindAnyObjectByType<SpectatorSystem>();
            Debug.Log("THIS AINT CAYELLED");//////////
            if (spectatorSystem != null)
            {
                spectatorSystem.UpdateAlivePlayers();
                Debug.Log("THIS AINT CAYELLED22");//////////
            }
        }

        [Rpc(SendTo.Owner)]
        private void ShowDeathUIRpc()
        {
            Debug.Log("ShowDeathUIRpc fired on Client!");

            UIManager uiManager = FindAnyObjectByType<UIManager>();

            if (uiManager != null)
            {
                uiManager.Death(OwnerClientId);
            }
            else
            {
                Debug.LogError("UIManager could not be found on the Client!");
            }
        }


        /*
        // Added Improvement
        [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
        public void RequestDeathRpc()
        {
            Debug.Log("RequestDeathRpc fired!!"); // This isn't being displayed for the Clients, even though the DeathTrigger Debug.Logs are.

            if (!IsServer)
            {
                Debug.Log("It doesn't believe we're in the Server");
                return;
            }

            ulong clientId = OwnerClientId;

            Debug.Log($"Server received death request from Player {clientId}");

            NetworkObject playerObject = NetworkObject;

            UIManager uiManager = FindAnyObjectByType<UIManager>();

            if (uiManager != null)
            {
                uiManager.Death(clientId);
                Debug.Log("Death UI should be triggered");
            }

            Debug.Log(
    $"Despawning: {playerObject.name} | " +
    $"NetworkObjectId: {playerObject.NetworkObjectId} | " +
    $"OwnerClientId: {playerObject.OwnerClientId}"
);

            playerObject.Despawn();
            Debug.Log("Player should be despawned");

            SpectatorSystem spectatorSystem = FindAnyObjectByType<SpectatorSystem>();

            if (spectatorSystem != null)
            {
                spectatorSystem.UpdateAlivePlayers();
                Debug.Log("Spectator should be Updated");
            }
        }
        // Added Improvement
        */



        /*
        private void Start()
        {

            _controller = GetComponent<CharacterController>();

            if (!IsOwner)
                return;

            // Reinitialize the CharacterController at the
            // Player's networked spawn position.
            _controller.enabled = false;
            _controller.enabled = true;

            _fpsCamera = GetComponentInChildren<Camera>();
            _initialCameraPos = _fpsCamera.transform.localPosition;
        }

        private void Update()
        {
            _grounded = _controller.isGrounded; // grounded

            if (!IsOwner) // Multiplayer
                return;

            FPSController();
            CameraController();
            HeadBobbing();

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _controller.height = 2.0f;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _controller.height = 1.0f;
            }
        }

        void FPSController()
        {
            float h = Input.GetAxis("Horizontal"); //horizontal inputs (a, d, leftarrow, rightarrow)
            float v = Input.GetAxis("Vertical"); //veritical inputs (w, s, uparrow, downarrow)

            Vector3 direction = new Vector3(h, 0, v); //direction to move
            Vector3 velocity = direction * _walkSpeed; //velocity is the direction and speed we travel

            if (Input.GetKeyDown(KeyCode.C))
            {
                _crouching = !_crouching;

                if (_crouching == true)
                {
                    _controller.height = 2.0f;
                }
                else
                {
                    _controller.height = 1.0f;
                }
                
            }

            if (Input.GetKey(KeyCode.LeftShift) && _crouching == false) //check if we are holding down left shift
            {
                velocity = direction * _runSpeed; //use the run velocity 
            }

            if (_controller.isGrounded == true) //check if we're grounded
            {
                if (Input.GetKeyDown(KeyCode.Space)) //check for the space key
                {
                    _yVelocity = _jumpHeight; //assign the cache velocity to our jump height
                }
            }
            else //we're not grounded
            {
                _yVelocity -= _gravity; //subtract gravity from our yVelocity 
            }

            velocity.y = _yVelocity; //assign the cached value of our yvelocity

            velocity = transform.TransformDirection(velocity);

            _controller.Move(velocity * Time.deltaTime);//move the controller x meters per second
        }

        void CameraController()
        {
            float mouseX = Input.GetAxis("Mouse X"); //get mouse movement on the x
            float mouseY = Input.GetAxis("Mouse Y"); //get mouse movement on the y

            Vector3 rot = transform.localEulerAngles; //store current rotation
            rot.y += mouseX * _lookSensitivity; //add our mouseX movement to the y axis
            transform.localRotation = Quaternion.AngleAxis(rot.y, Vector3.up); ////rotate along the y axis by movement amount

            Vector3 camRot = _fpsCamera.transform.localEulerAngles; //store the current rotation (line 122)
            camRot.x += -mouseY * _lookSensitivity; //add the mouseY movement to the x axis
            _fpsCamera.transform.localRotation = Quaternion.AngleAxis(camRot.x, Vector3.right); //rotate along the x axis by movement amount
        }

        void HeadBobbing()
        {
            float h = Input.GetAxis("Horizontal"); //horizontal inputs (a, d, leftarrow, rightarrow)
            float v = Input.GetAxis("Vertical"); //veritical inputs (w, s, uparrow, downarrow)

            if (h != 0 || v != 0) // Are we moving?
            {
               
                if (Input.GetKey(KeyCode.LeftShift)) //check if running
                {
                    _timer += _runFrequency * Time.deltaTime; //increment timer for our sin/cos waves when running
                }
                else
                {
                    _timer += _walkFrequency * Time.deltaTime; //increment timer for our sin/cos waves when walking
                }

                Vector3 headPosition = new Vector3 //calculate the head position in our walk cycle
                    (
                        _initialCameraPos.x + Mathf.Cos(_timer) * _heightOffset, //x value
                        _initialCameraPos.y + Mathf.Sin(_timer) * _heightOffset, //y value
                        0 // z value
                    );

                _fpsCamera.transform.localPosition = headPosition; //assign the head position

                if (_timer > Mathf.PI * 2) //reset the timer when we complete a full walk cycle on the unit circle
                {
                    _timer = 0; //completed walk cycle. Reset. 
                }
            }
            else
            {
                _timer = Mathf.PI / 2; //reset timer back to 1 for initial walk cycle 

                Vector3 resetHead = new Vector3 //calculate reset head position back to initial cam pos
                    (
                    Mathf.Lerp(_fpsCamera.transform.localPosition.x, _initialCameraPos.x, _smooth * Time.deltaTime), //x vlaue
                    Mathf.Lerp(_fpsCamera.transform.localPosition.y, _initialCameraPos.y, _smooth * Time.deltaTime), //y value
                    0 //z value
                    );

                _fpsCamera.transform.localPosition = resetHead; //assign the head position back to the initial cam pos
            }
        }
        */



    }
        
}////////////
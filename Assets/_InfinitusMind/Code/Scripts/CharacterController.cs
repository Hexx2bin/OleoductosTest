using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Photon.Pun;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Infinitus
{
    [RequireComponent(typeof(CharacterManager))]
    public partial class CharacterController : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] [BoxGroup("References")]
        public Transform rotatingTransform;

        private float targetAngle;

        [BoxGroup("Settings")] [SerializeField]
        private float speed = 2;

        [BoxGroup("Settings")] [ShowInInspector] [ReadOnly]
        private Vector2 movementInput;

        [BoxGroup("Settings")] [ReadOnly] public Vector2 lastMovementInput;

        public bool interactInput;
        public bool rotateInteractableRightInput;
        public bool rotateInteractableLeftInput;

        private Transform characterTransform;
        private CharacterManager _characterManager;

        public UnityEvent<Vector2> onInputUpdated = new UnityEvent<Vector2>();
        public UnityEvent<float> onTargetAngleUpdated = new UnityEvent<float>();

        public UnityEvent<bool> onInteractionUpdated;

        private void Start()
        {
            _characterManager = GetComponent<CharacterManager>();
            InitializeVariables();
            if (photonView.IsMine)
            {
                CinemachineVirtualCamera vCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();
                vCamera.Follow = this.transform;
                DontDestroyOnLoad(vCamera.gameObject);
                DontDestroyOnLoad(Camera.main.gameObject);
            }
        }

        public float xCuadrado(float x)
        {
            float y = x * x;
            return y;
        }

        private void InitializeVariables()
        {
            characterTransform = transform;
            interactableObjectsAtRange = new List<InteractableObject>();
        }

        private void Update()
        {
            //UpdateInteraction();
            //UpdateInteractableRotation();
            /*if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
            {
                return;
            }*/
            if (photonView.IsMine)
            {
                UpdateInput();
                UpdateMovement();
                //UpdateRotatingTransform();

                UpdateCurrentGridCell();                
            }            
        }

        private void UpdateInput()
        {
            movementInput =
                new Vector2(Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.RightArrow) ? 1 : 0,
                    Input.GetKey(KeyCode.UpArrow) ? 1 : Input.GetKey(KeyCode.DownArrow) ? -1 : 0);
            onInputUpdated?.Invoke(movementInput);
            if (movementInput.magnitude != 0) lastMovementInput = movementInput;

            photonView.RPC("SetInteractValues", RpcTarget.All, Input.GetKeyDown(KeyCode.Space), movementInput);
            //interactInput = Input.GetKeyDown(KeyCode.Space);
            rotateInteractableRightInput = Input.GetKeyDown(KeyCode.A);
            rotateInteractableLeftInput = Input.GetKeyDown(KeyCode.D);
            Debug.Log("valor de interactInput en el update: " + interactInput);
        }

        [PunRPC]
        void SetInteractValues(bool _interactInput, Vector2 _movementInput)
        {
            interactInput = _interactInput;
            movementInput = _movementInput;

            UpdateRotatingTransform();
            UpdateInteraction();
            UpdateInteractableRotation();
        }

        float AngleToUnityAngle(float angle) => Mathf.Repeat(-angle + 90, 360);

        float AngleFromVector2(Vector2 v)
        {
            if (v.x >= 0) return AngleToUnityAngle(0 + Mathf.Atan(v.y / v.x) * Mathf.Rad2Deg);
            else return AngleToUnityAngle(180 + Mathf.Atan(v.y / v.x) * Mathf.Rad2Deg);
        }

        private bool IsMoving(Vector2 movementAmount) => movementAmount.magnitude != 0;

        private void UpdateRotatingTransform()
        {
            if (IsMoving(movementInput))
            {
                targetAngle = AngleFromVector2(movementInput);
                onTargetAngleUpdated?.Invoke(targetAngle);
            }

            rotatingTransform.eulerAngles = new Vector3(0,
                Mathf.LerpAngle(rotatingTransform.eulerAngles.y, targetAngle, 0.1f), 0);
        }

        private void UpdateMovement()
        {
            Vector2 movementAmount = movementInput.normalized * speed * Time.deltaTime;
            Vector3 position = characterTransform.position;
            position += new Vector3(movementAmount.x, position.y, movementAmount.y);
            characterTransform.position = position;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            /*if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(interactInput);
            }
            else
            {
                interactInput = (bool)stream.ReceiveNext();
                Debug.Log("valor de interactInput: " + interactInput);
            }*/
        }
    }
}
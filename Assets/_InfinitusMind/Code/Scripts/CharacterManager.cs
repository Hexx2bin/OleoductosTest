using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Photon.Pun;
using UnityEngine.Assertions;

namespace Infinitus
{
    public class CharacterManager : MonoBehaviourPunCallbacks, IPunObservable
    {
        #region References

        [SerializeField] private Animator characterAnimator;

        [SerializeField] private SkeletonAnimation characterSkeletonAnimation;

        #endregion

        #region Variables

        private Transform characterAnimatorTransform;
        private const string WalkingParameterName = "Walking";
        private Vector2 currentMovementAmount = Vector2.zero;
        private int walkingParameterId;
        private const string HoldingParameterName = "Holding";
        private int holdingParameterId;
        private float targetAngle;

        private Transform characterSkeletonAnimationTransform;
        private float characterSkeletonStartingScale;
        private const string SpineIdleAnimationName = "idle_1";
        private const string SpineWalkingAnimationName = "run";

        #endregion

        private bool wasMoving;

        private GameSettings gameSettings;

        private CharacterController _characterController;

        public static GameObject LocalPlayerInstance;

        public Transform CharacterAnimatorTransform { get => characterAnimatorTransform; set => characterAnimatorTransform = value; }
        public Transform CharacterSkeletonAnimationTransform { get => characterSkeletonAnimationTransform; set => characterSkeletonAnimationTransform = value; }

        private void Awake()
        {
            if (photonView.IsMine)
            {
                LocalPlayerInstance = this.gameObject;
            }
            DontDestroyOnLoad(this.gameObject);
            InitializeVariables();
        }

        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            Assert.IsNotNull(_characterController, "no se ensamblo el character controller para el personaje");
            UpdateCharactersState();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            AddListeners();
        }

        private void InitializeVariables()
        {
            gameSettings = GameSettings.Instance;

            characterAnimatorTransform = characterAnimator.transform;

            characterSkeletonAnimationTransform = characterSkeletonAnimation.transform;
            characterSkeletonStartingScale = characterSkeletonAnimationTransform.localScale.x;

            walkingParameterId = Animator.StringToHash(WalkingParameterName);
            holdingParameterId = Animator.StringToHash(HoldingParameterName);
        }

        public void AddListeners()
        {
            _characterController.onInputUpdated.AddListener(UpdateMovement);
            _characterController.onTargetAngleUpdated.AddListener((angle) => targetAngle = angle);

            _characterController.onInputUpdated.AddListener(UpdateMovement);
            _characterController.onInteractionUpdated.AddListener(OnInteractionUpdated);
            _characterController.onTargetAngleUpdated.AddListener((angle) => targetAngle = angle);
        }

        private void UpdateMovement(Vector2 movementAmount)
        {
            switch (gameSettings.characterType)
            {
                case GameSettings.CharacterType.Both:
                    bool _wasMoving = wasMoving;
                    UpdateModelMovement(movementAmount);
                    wasMoving = _wasMoving;
                    UpdateSpineMovement(movementAmount);
                    break;
                case GameSettings.CharacterType.Model:
                    UpdateModelMovement(movementAmount);
                    break;
                case GameSettings.CharacterType.Spine:
                    UpdateSpineMovement(movementAmount);
                    break;
            }
            currentMovementAmount = movementAmount;
        }

        private void OnInteractionUpdated(bool holding) => characterAnimator.SetBool(holdingParameterId, holding);

        private bool IsMoving(Vector2 movementAmount) => movementAmount.magnitude == 0;

        private void UpdateModelMovement(Vector2 movementAmount)
        {
            if (IsMoving(movementAmount))
            {
                if (wasMoving)
                {
                    characterAnimator.SetBool(walkingParameterId, false);
                    wasMoving = false;
                }
            }
            else
            {
                if (!wasMoving)
                {
                    characterAnimator.SetBool(walkingParameterId, true);
                    wasMoving = true;
                }
            }
        }

        private void UpdateSpineMovement(Vector2 movementAmount)
        {
            if (IsMoving(movementAmount))
            {
                if (wasMoving)
                {
                    characterSkeletonAnimation.state.SetAnimation(0, SpineIdleAnimationName, true);
                    wasMoving = false;
                }
            }
            else
            {
                if (!wasMoving)
                {
                    characterSkeletonAnimation.state.SetAnimation(0, SpineWalkingAnimationName, true);
                    wasMoving = true;
                }

                if (movementAmount.x != 0)
                    characterSkeletonAnimationTransform.localScale = new Vector3(
                        characterSkeletonStartingScale * (movementAmount.x > 0 ? 1 : -1),
                        characterSkeletonStartingScale, characterSkeletonStartingScale);
            }
        }

        private void Update()
        {
            if (photonView.IsMine)
            {
                UpdateRotation();
            }            
        }

        private void UpdateCharactersState()
        {
            switch (gameSettings.characterType)
            {
                case GameSettings.CharacterType.Both:
                    characterAnimator.gameObject.SetActive(true);
                    characterSkeletonAnimation.gameObject.SetActive(true);
                    break;
                case GameSettings.CharacterType.Model:
                    characterAnimator.gameObject.SetActive(true);
                    characterSkeletonAnimation.gameObject.SetActive(false);
                    break;
                case GameSettings.CharacterType.Spine:
                    characterAnimator.gameObject.SetActive(false);
                    characterSkeletonAnimation.gameObject.SetActive(true);
                    break;
            }
        }

        private void UpdateRotation()
        {
            if (gameSettings.characterType == GameSettings.CharacterType.Spine) return;

            characterAnimatorTransform.eulerAngles = new Vector3(0,
                Mathf.LerpAngle(characterAnimatorTransform.eulerAngles.y, targetAngle, 0.1f), 0);
        }

        private void OnDestroy()
        {
            if (_characterController != null)
                _characterController.onInputUpdated.RemoveListener(UpdateMovement);
        }

        void OnLevelWasLoaded(int level)
        {
            this.CalledOnLevelWasLoaded(level);
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadingMode)
        {
            this.CalledOnLevelWasLoaded(scene.buildIndex);
        }

        private void CalledOnLevelWasLoaded(int level)
        {
            // check if we are outside the Arena and if it's the case, spawn around the center of the arena in a safe zone
            if (!Physics.Raycast(transform.position, -Vector3.up, 5f))
            {
                transform.position = new Vector3(0f, 0f, -2.35f);
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                // We own this player: send the others our data
                stream.SendNext(targetAngle);
                stream.SendNext(currentMovementAmount);
            }
            else
            {
                characterAnimatorTransform.eulerAngles = new Vector3(0,
                Mathf.LerpAngle(characterAnimatorTransform.eulerAngles.y, (float)stream.ReceiveNext(), 0.5f), 0);
                UpdateMovement((Vector2)stream.ReceiveNext());
            }
        }
    }
}
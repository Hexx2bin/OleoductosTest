using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Photon.Pun;

namespace Infinitus
{    
    public class InteractableObject : MonoBehaviour
    {
        protected Transform objectTransform;
        protected Transform targetTransform;

        [BoxGroup("Settings")] [SerializeField]
        private GameObject _targetPrefab;

        [BoxGroup("Runtime")] [ReadOnly] public bool picked;

        [BoxGroup("Runtime")] [SerializeField] [ReadOnly]
        protected CharacterController holderCharacterController;

        private Transform[] _anchors;

        public Transform[] Anchors { get => _anchors; }

        protected virtual void Start()
        {
            _anchors = GetComponentsInChildren<Transform>();
            InitializeVariables();
            CreateTargetTransform();
        }

        private void InitializeVariables()
        {
            objectTransform = transform;
        }

        private void CreateTargetTransform()
        {
            //targetTransform = new GameObject($"{gameObject.name} - Target").transform;
            targetTransform = PhotonNetwork.Instantiate($"{gameObject.name} - Target", new Vector3(0, 0, 0), Quaternion.identity, 0).transform;
            targetTransform.position = objectTransform.position;
        }

        private void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            objectTransform.position = Vector3.Lerp(objectTransform.position, targetTransform.position, 0.1f);
        }

        public virtual bool RequestInteraction(CharacterController requestingCharacterController)
        {
            if (picked)
            {
                picked = false;
                Drop(requestingCharacterController);
            }
            else
            {
                picked = true;
                holderCharacterController = requestingCharacterController;
                PickUp(requestingCharacterController);
            }

            return true;
        }

        protected virtual void PickUp(CharacterController requestingCharacterController)
        {
        }

        protected virtual void Drop(CharacterController requestingCharacterController)
        {
        }

        public virtual void OnSpecialRightInteraction()
        {
        }

        public virtual void OnSpecialLeftInteraction()
        {
        }
    }
}
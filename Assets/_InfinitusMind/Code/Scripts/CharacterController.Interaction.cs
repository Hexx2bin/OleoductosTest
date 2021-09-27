using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Photon.Pun;

namespace Infinitus
{
    public partial class CharacterController : MonoBehaviourPunCallbacks
    {
        [BoxGroup("References/Interaction")] public Transform itemHolderTransform;

        private const string InteractableTag = "Interactable";

        [ShowInInspector] [BoxGroup("References/Interaction/Runtime")] [ReadOnly]
        private List<InteractableObject> interactableObjectsAtRange;

        [ShowInInspector] [BoxGroup("References/Interaction/Runtime")] [ReadOnly]
        private InteractableObject grabbedInteractableObject;

        [ShowInInspector] [BoxGroup("References/Interaction/Runtime")] [ReadOnly]
        public GridCell currentGridCell;

        [BoxGroup("Settings/Interaction")] [SerializeField]
        public LayerMask gridLayer;
        [BoxGroup("Settings/Interaction")] [SerializeField]
        private float _interactionDist = 2.0f;
        [BoxGroup("Settings/Interaction")] [SerializeField]
        private float _verticalOffSet = 0.125f;

        private bool IsGrabbingObject => grabbedInteractableObject != null;

        private InteractableObject GetAvailableInteractableObject()
        {
            for (int i = 0; i < interactableObjectsAtRange.Count; i++)
            {
                if (!interactableObjectsAtRange[i].picked)
                    return interactableObjectsAtRange[i];
            }

            return null;
        }

        private void UpdateInteraction()
        {
            if (!interactInput) return;

            if (IsGrabbingObject)
            {
                if (grabbedInteractableObject.RequestInteraction(this))
                {
                    grabbedInteractableObject = null;
                    onInteractionUpdated?.Invoke(false);
                }
            }
            else
            {
                grabbedInteractableObject = GetAvailableInteractableObject();
                if (!grabbedInteractableObject) return;

                if (!grabbedInteractableObject.RequestInteraction(this))
                    grabbedInteractableObject = null;
                else
                    onInteractionUpdated?.Invoke(true);
            }
        }

        private void UpdateInteractableRotation()
        {
            if (!IsGrabbingObject) return;

            if (rotateInteractableRightInput)
            {
                grabbedInteractableObject.OnSpecialRightInteraction();
            }
            else if (rotateInteractableLeftInput)
            {
                grabbedInteractableObject.OnSpecialLeftInteraction();
            }
        }

        private bool HasCurrentCell => currentGridCell != null;

        private void UpdateCurrentGridCell()
        {
            if (Physics.Raycast(rotatingTransform.position, rotatingTransform.forward, out var hit, 1, gridLayer))
            {
                GridCell cellInFront = hit.transform.GetComponentInParent<GridCell>();
                if (!HasCurrentCell)
                {
                    cellInFront.SelectCell();
                    currentGridCell = cellInFront;
                }
                else
                {
                    if (currentGridCell != cellInFront)
                    {
                        currentGridCell.UnselectCell();
                        cellInFront.SelectCell();
                        currentGridCell = cellInFront;
                    }
                }
            }
            else
            {
                if (HasCurrentCell)
                    currentGridCell.UnselectCell();
                currentGridCell = null;
            }
        }

        public void FixedUpdate()
        {
            Vector3 rayOrigin = transform.position;
            rayOrigin.y += _verticalOffSet;            
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, _characterManager.CharacterAnimatorTransform.forward, out hit, _interactionDist, 1 << 6))
            {
                InteractableObject interactableObject = hit.collider.GetComponent<InteractableObject>();
                if (!interactableObjectsAtRange.Contains(interactableObject))
                {
                    interactableObject.gameObject.GetComponent<MeshRenderer>().material.color = Color.green;
                    interactableObjectsAtRange.Add(interactableObject);
                }
                    
                Debug.Log("Did Hit");
                Debug.DrawRay(rayOrigin, _characterManager.CharacterAnimatorTransform.forward.normalized * _interactionDist, Color.red);
            }
            else
            {
                Debug.DrawRay(rayOrigin, _characterManager.CharacterAnimatorTransform.forward.normalized * _interactionDist, Color.yellow);
                foreach(InteractableObject tool in interactableObjectsAtRange)
                {
                    tool.gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
                }
                interactableObjectsAtRange.Clear();
            }
        }
    }
}
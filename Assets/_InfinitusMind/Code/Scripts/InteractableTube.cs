using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace Infinitus
{
    public class InteractableTube : InteractableObject
    {
        [BoxGroup("Settings")] [SerializeField]
        private float rotationAmount;

        [BoxGroup("Runtime")] [ShowInInspector] [ReadOnly]
        private GridCell currentGridCell;

        [BoxGroup("Settings")] [SerializeField]
        private float tweenDuration;

        private Tween xTween;
        private Tween yTween;
        private Tween zTween;

        private bool CharacterHasGridCell => holderCharacterController.currentGridCell != null;

        public override bool RequestInteraction(CharacterController requestingCharacterController)
        {
            if (picked)
            {
                if (CharacterHasGridCell)
                {
                    if (holderCharacterController.currentGridCell.InUse)
                    {
                        return false;
                    }
                    else
                    {
                        picked = false;
                        DropInCell(requestingCharacterController.currentGridCell);
                        return true;
                    }
                }
                else
                {
                    picked = false;
                    Drop(requestingCharacterController);
                    return true;
                }
            }
            else
            {
                picked = true;
                holderCharacterController = requestingCharacterController;
                PickUp(requestingCharacterController);
                return true;
            }
        }


        protected override void PickUp(CharacterController requestingCharacterController)
        {
            targetTransform.parent = requestingCharacterController.itemHolderTransform;
            TweenToPosition(new Vector3(0, 0, 0));

            if (currentGridCell == null) return;
            currentGridCell.RemoveTool();
            currentGridCell = null;
        }

        protected override void Drop(CharacterController requestingCharacterController)
        {
            targetTransform.parent = null;
            var characterPosition = requestingCharacterController.transform.position;
            var targetPosition = characterPosition +
                                 new Vector3((requestingCharacterController.lastMovementInput.normalized * 1).x,
                                     characterPosition.y,
                                     (requestingCharacterController.lastMovementInput.normalized * 1).y);
            TweenToPosition(targetPosition, yEase: Ease.InQuart);

            holderCharacterController = null;
        }

        private void DropInCell(GridCell gridCell)
        {
            currentGridCell = gridCell;
            currentGridCell.AddTool(gameObject, false);
            targetTransform.parent = currentGridCell.transform;
            TweenToPosition(new Vector3(0, 0, 0), yEase: Ease.InOutQuad);

            holderCharacterController = null;
        }

        private void TweenToPosition(Vector3 targetPosition,
            Ease xEase = Ease.InOutQuad,
            Ease yEase = Ease.OutExpo,
            Ease zEase = Ease.InOutQuad)
        {
            xTween = targetTransform.DOLocalMoveX(targetPosition.x, tweenDuration).SetEase(xEase);
            yTween = targetTransform.DOLocalMoveY(targetPosition.y, tweenDuration).SetEase(yEase);
            zTween = targetTransform.DOLocalMoveZ(targetPosition.z, tweenDuration).SetEase(zEase);
        }

        public override void OnSpecialRightInteraction()
        {
            Rotate(true);
        }

        public override void OnSpecialLeftInteraction()
        {
            Rotate(false);
        }

        private void Rotate(bool right)
        {
            Vector3 eulerAngles = objectTransform.eulerAngles;
            eulerAngles = new Vector3(eulerAngles.x, eulerAngles.y +
                                                     (right ? rotationAmount : -rotationAmount), eulerAngles.z);
            objectTransform.eulerAngles = eulerAngles;
        }
    }
}
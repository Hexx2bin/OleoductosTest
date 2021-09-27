using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Infinitus
{
    public class GridCell : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _selectedGO;

        private GameObject _currentTool;

        public bool InUse
        {
            get { return _currentTool != null ? true : false; }
        }

        private void Start()
        {
            Assert.IsNotNull(_selectedGO, "no se ensamblo el objeto para resaltar la seleccion");
            _selectedGO.enabled = false;
        }

        public void SelectCell()
        {
            Color selectColor = InUse ? Color.red : Color.green;
            selectColor.a = 0.3f;
            _selectedGO.material.color = selectColor;
            _selectedGO.enabled = true;
        }

        public void UnselectCell()
        {
            _selectedGO.enabled = false;
        }

        public void AddTool(GameObject tool, bool snapTool = true)
        {
            if (InUse) return;
            if (_currentTool != null)
            {
                Destroy(_currentTool);
            }

            if (snapTool)
                tool.transform.localPosition = Vector3.zero;
            _currentTool = tool;
        }

        public void RemoveTool()
        {
            if (_currentTool == null) return;
            _currentTool = null;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Transform childCubeTransform = transform.GetChild(0);
            Gizmos.DrawWireCube(childCubeTransform.position, childCubeTransform.localScale);
        }
    }
}
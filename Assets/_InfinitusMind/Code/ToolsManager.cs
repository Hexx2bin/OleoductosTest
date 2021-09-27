using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infinitus
{
    public class ToolsManager : MonoBehaviour
    {
        [SerializeField] private LayerMask _mouseDragLayer;

        private GameObject _dummyTool;
        private GameObject _toolClone;
        private Vector3 _distance;
        private Vector3 _lastMousePos;
        private GridCell currentCell;

        void Start()
        {
        }

        void Update()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.yellow);
            RaycastHit rayCastHit;
            if (Physics.Raycast(ray, out rayCastHit))
            {
                if (rayCastHit.collider.transform.parent != null)
                {
                    _dummyTool = rayCastHit.collider.transform.parent.gameObject;
                    Debug.Log("objeto seleccionado: " + rayCastHit.collider.transform.parent.gameObject.name);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                Debug.Log("seleccionar objeto");
                _toolClone = Instantiate(_dummyTool);
                _lastMousePos = Input.mousePosition;
            }

            if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                if (_toolClone != null)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Grid")))
                    {
                        GridCell selectedCell = hit.collider.GetComponentInParent<GridCell>();
                        if (selectedCell.InUse)
                        {
                            Destroy(_toolClone);
                        }
                        else
                        {
                            selectedCell.AddTool(_toolClone);
                            GridManager.Instance.UnselectAllCells();
                        }
                    }
                    else
                    {
                        Destroy(_toolClone);
                    }

                    _toolClone = null;
                }
            }

            if (Input.GetKey(KeyCode.Mouse1) && _toolClone != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mouseDragLayer))
                {
                    _toolClone.transform.position = hit.point;
                }

                RaycastHit hit2;
                if (Physics.Raycast(ray, out hit2, Mathf.Infinity, LayerMask.GetMask("Grid")))
                {
                    if (currentCell != hit2.collider.GetComponentInParent<GridCell>())
                    {
                        currentCell = hit2.collider.GetComponentInParent<GridCell>();
                        GridManager.Instance.UnselectAllCells();
                    }
                    else
                    {
                        return;
                    }

                    if (currentCell != null)
                    {
                        currentCell.SelectCell();
                    }
                    else
                    {
                    }
                }
            }
        }
    }
}
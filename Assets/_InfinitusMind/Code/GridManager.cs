using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Infinitus
{
    public class GridManager : MonoBehaviour
    {
        [SerializeField] private GameObject _gridCell;

        [SerializeField] private int _rows = 4;

        [SerializeField] private int _columns = 4;

        [SerializeField] private int _cellSize = 3;

        private static GridManager _instance;

        public static GridManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameObject.FindObjectOfType<GridManager>();
                }

                return _instance;
            }
        }

        void Start()
        {
            Assert.IsNotNull(_gridCell, "no se ensamblo la celda individual de la cuadricula");
            CreateGrid(_rows, _columns);
        }

        public void UnselectAllCells()
        {
            foreach (GridCell cell in GetComponentsInChildren<GridCell>())
            {
                cell.UnselectCell();
            }
        }

        private void CreateGrid(int rows, int columns)
        {
            for (int i = 0; i < rows; ++i)
            {
                for (int j = 0; j < rows; ++j)
                {
                    GameObject dummyCell = Instantiate(_gridCell, transform);
                    Vector3 dummyPos = dummyCell.transform.position;
                    dummyPos.x = j * _cellSize;
                    dummyPos.z = i * -_cellSize;
                    dummyCell.transform.position = dummyPos + transform.position;
                }
            }
        }
    }
}
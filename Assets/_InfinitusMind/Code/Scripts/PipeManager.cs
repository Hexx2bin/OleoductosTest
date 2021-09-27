using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Infinitus
{
    public class PipeManager : MonoBehaviour
    {

        private List<InteractableObject> _tubeList = new List<InteractableObject>();

        void Start()
        {
            foreach(InteractableObject tube in GameObject.FindObjectsOfType<InteractableObject>())
            {
                _tubeList.Add(tube);
            }
        }

        public void JoinTube()
        {

        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            for(int i = 0; i < _tubeList.Count; ++i)
            {
                for(int j = 1; j < _tubeList[i].Anchors.Length - 1; ++j)
                {
                    for(int k = 0; k < _tubeList.Count; ++k)
                    {
                        if(k != i)
                        {
                            Gizmos.DrawLine(_tubeList[i].Anchors[j].position, _tubeList[k].Anchors[j].position);
                            Gizmos.DrawLine(_tubeList[i].Anchors[j].position, _tubeList[k].Anchors[j+1].position);
                            Gizmos.DrawLine(_tubeList[i].Anchors[j+1].position, _tubeList[k].Anchors[j].position);
                            Gizmos.DrawLine(_tubeList[i].Anchors[j+1].position, _tubeList[k].Anchors[j + 1].position);
                        }                        
                    }                    
                }
            }
        }
    }
}
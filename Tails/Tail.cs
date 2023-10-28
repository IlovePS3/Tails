using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Tails
{
    class Tail : MonoBehaviour
    {
        public VRRig rig;
        public Player player;
        DynamicBone phys;
        Renderer renderer;
        int cosmetic;
        void Start () 
        {
            renderer = transform.GetChild(1).GetComponent<Renderer>();
            phys = gameObject.AddComponent<DynamicBone>();
            phys.m_Root = transform.GetChild(0);
        }

        void Update()
        {
            if(rig != null) 
            {
                if(renderer.material != rig.materialsToChangeTo[rig.setMatIndex])
                {
                    renderer.material = rig.materialsToChangeTo[rig.setMatIndex];
                }
            }
            if(player != null) 
            {
                int.TryParse(player.CustomProperties["Tail"].ToString(), out int c);
                cosmetic = c;
            }
        }
    }
}

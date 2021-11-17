using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MaterialUI
{
    public class MaterialUIManager : MonoBehaviour
    {
        public static Transform UIRoot;

        void Awake()
        {
            UIRoot = this.gameObject.transform;
        }
    }
}

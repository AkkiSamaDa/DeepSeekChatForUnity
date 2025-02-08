using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat
{
    public class DeepSeekUtil : MonoBehaviour
    {
        #region 单例
        private static DeepSeekUtil _instance;

        public static DeepSeekUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DeepSeekUtil");
                    _instance = go.AddComponent<DeepSeekUtil>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        #endregion
        
        
    }
}
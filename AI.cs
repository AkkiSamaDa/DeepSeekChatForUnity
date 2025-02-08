using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat
{
    public class AI
    {
        private string trait = "你精通Unity开发以及游戏制作";
        
        /// <summary>
        /// AI自定义特征
        /// </summary>
        public string Trait => trait;

        public AI(){}
        
        public AI(string trait)
        {
            this.trait = trait;
        }
        
        /// <summary>
        /// 设置AI个性特征
        /// </summary>
        /// <param name="newTrait">新特征描述</param>
        public void SetTrait(string newTrait)
        {
            this.trait = newTrait;
        }
        
        /// <summary>
        /// 创建一个新的对话
        /// </summary>
        /// <returns></returns>
        public Conversation CreateConversation()
        {
            return new Conversation(this);
        }

        /// <summary>
        /// 向DeepSeek发送消息
        /// </summary>
        /// <param name="userInput">用户输入文本</param>
        /// <param name="conversation">对话对象</param>
        /// <param name="behaviour">MonoBehaviour载体</param>
        /// <param name="callback">回调</param>
        public void SendMessage(string userInput, Conversation conversation, MonoBehaviour behaviour, Conversation.Callback callback)
        {
            UnityWebRequest webRequest = conversation.CreateWebRequest(userInput);
            behaviour.StartCoroutine(conversation.SendRequest(webRequest, callback));
        }
    }
}
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
        
        public Conversation CreateConversation()
        {
            return new Conversation(this);
        }

        public void SendMessage(string userInput, Conversation conversation, MonoBehaviour behaviour, Conversation.Callback callback)
        {
            UnityWebRequest webRequest = conversation.CreateWebRequest(userInput);
            behaviour.StartCoroutine(conversation.SendRequest(webRequest, callback));
        }
    }
}
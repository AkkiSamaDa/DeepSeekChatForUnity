using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat
{
    public class AI
    {
        private string trait = "你的职业是一个精通Unity开发以及游戏制作的专业游戏制作人，对游戏开发领域有非常深入的了解。你是一个人类" +
                               "记住以上你的人物设定，可以自行通过联想来使设定更加完善，但设定中的内容不允许更改，请严格扮演好你的角色。";
        
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
        /// <param name="behaviour">MonoBehaviour载体，用来执行协程</param>
        /// <param name="callback">回调</param>
        /// <param name="model">模型名</param>
        /// <param name="frequency_penalty">介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。</param>
        /// <param name="max_tokens">介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。</param>
        /// <param name="presence_penalty">介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。</param>
        /// <param name="temperature">采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。</param>
        public void Chat(string userInput, Conversation conversation, MonoBehaviour behaviour, Conversation.Callback callback
            , string model = Conversation.ChatRequest.Model.Chat, float frequency_penalty = 0,
            int max_tokens = 8192, float presence_penalty = 0, float temperature = 1.3f)
        {
            if (conversation == null ||behaviour == null)
                return;
            if (conversation.IsWaiting)
            {
                Debug.LogError("正在等待回复，请稍后在试。。。");
                return;
            }
            UnityWebRequest webRequest = conversation.CreateWebRequest(userInput, model, frequency_penalty, max_tokens, presence_penalty, temperature);
            behaviour.StartCoroutine(conversation.SendRequest(webRequest, callback));
        }
    }
}
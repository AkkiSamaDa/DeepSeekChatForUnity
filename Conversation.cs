using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace AIChat
{
    public class Conversation
    {
        [System.Serializable]
        public struct Message
        {
            public string content;
            public string role;

            public Message(string content, string role)
            {
                this.content = content;
                this.role = role;
            }
        }
        
        [System.Serializable]
        public class ChatRequest
        {
            public static class Role
            {
                public const string System = "system";
                public const string User = "user";
                public const string Assistant = "assistant";
                public const string Tool = "tool";
            }
            public static class Model
            {
                /// <summary>
                /// DeepSeek-V3
                /// </summary>
                public const string Chat = "deepseek-chat";
                
                /// <summary>
                /// DeepSeek-R1
                /// </summary>
                public const string Reasoner = "deepseek-reasoner";
            }
            
            public List<Message> messages;
            public string model;
            
            /// <summary>
            /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
            /// </summary>
            public float frequency_penalty;
            
            /// <summary>
            /// 介于 1 到 8192 间的整数，限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。
            /// </summary>
            public int max_tokens;
            
            /// <summary>
            /// 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
            /// </summary>
            public float presence_penalty;
            
            /// <summary>
            /// 采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。
            /// </summary>
            public float temperature;

            public ChatRequest(List<Message> messages, string model = Model.Chat, float frequency_penalty = 0,
                int max_tokens = 8192, float presence_penalty = 0, float temperature = 1)
            {
                this.messages = messages;
                this.model = model;
                this.frequency_penalty = frequency_penalty;
                this.max_tokens = max_tokens;
                this.presence_penalty = presence_penalty;
                this.temperature = temperature;
            }
        }
        
        [System.Serializable]
        public class ChatResponse
        {
            [System.Serializable]
            public class Choice
            {
                public string finish_reason;
                public int index;
                public ResponseMessage message;
            }
            
            [System.Serializable]
            public class ResponseMessage
            {
                public string content;
                public string reasoning_content;
                public string role;
            }
            
            /// <summary>
            /// 该对话的唯一标识符。
            /// </summary>
            public string id;
            
            /// <summary>
            /// 生成该 completion 的模型名。
            /// </summary>
            public string model;
            
            /// <summary>
            /// 创建聊天完成时的 Unix 时间戳（以秒为单位）。
            /// </summary>
            public int created;

            public Choice[] choices;

            /// <summary>
            /// 获取指定段的AI回复内容
            /// </summary>
            /// <param name="index">段落索引值，从0开始</param>
            /// <returns></returns>
            public string GetContent(int index)
            {
                if (this.choices.Length > 0 && index >= 0 && index < this.choices.Length)
                {
                    return choices[index].message.content;
                }
                return string.Empty;
            }
            
            
            /// <summary>
            /// 获取所有AI回复内容，通常AI回复只有一段内容，建议使用GetContent(0)获取，多段时会作拼接
            /// </summary>
            /// <returns></returns>
            public string GetAllContent()
            {
                string content = string.Empty;
                if (this.choices.Length <= 0) return content;
                for (int i = 0; i < this.choices.Length; i++)
                {
                    content += this.choices[i].message.content;
                    if (i < this.choices.Length - 1)
                        content += "\n";
                }
                return content;
            }
        }
        
        private AI ai;

        private List<Message> messageHistory;

        private bool isWaiting;
        
        private Message currentMessage;
        
        public bool IsWaiting => isWaiting;
        public Conversation(AI ai)
        {
            this.ai = ai;
        }
        
        //参数
        private const string URL = "https://api.deepseek.com/chat/completions";
        private static string TOKEN;
        
        public delegate void Callback(ChatResponse response, bool isSuccess);

        public UnityWebRequest CreateWebRequest(string userInput, string model, float frequency_penalty,
            int max_tokens, float presence_penalty, float temperature)
        {
            messageHistory ??= new List<Message>()
            {
                new (ai.Trait, ChatRequest.Role.System),
            };

            currentMessage = new(userInput, ChatRequest.Role.User);
            //追加历史对话记录
            var messages = new List<Message>(messageHistory)
            {
                currentMessage
            };

            ChatRequest chatReq = new(messages, model, frequency_penalty, max_tokens, presence_penalty, temperature);

            string jsonData = JsonUtility.ToJson(chatReq);
            //Debug.Log("Sending JSON: " + JsonUtility.ToJson(chatReq));
            //Debug.Log("Sending Message: " + userInput);
            
            UnityWebRequest webReq = new (URL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            webReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webReq.downloadHandler = new DownloadHandlerBuffer();
            
            webReq.SetRequestHeader("Content-Type", "application/json");
            webReq.SetRequestHeader("Accept", "application/json");
            webReq.SetRequestHeader("Authorization", $"Bearer {TOKEN}");
            return webReq;
        }

        public IEnumerator SendRequest(UnityWebRequest unityWebRequest, Callback callback)
        {
            UnityWebRequestAsyncOperation reqOp = unityWebRequest.SendWebRequest();
            
            isWaiting = true;
            while (!reqOp.isDone)
            {
                Debug.Log("等待回复中...");
                yield return new WaitForSeconds(3);
            }
            isWaiting = false;
            
            if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError ||
                unityWebRequest.result == UnityWebRequest.Result.ProtocolError ||
                unityWebRequest.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.LogError($"Error: {unityWebRequest.responseCode}, {unityWebRequest.downloadHandler.text}" + "\n" 
                    + unityWebRequest.error);
                callback(null, false);
                yield break;
            }

            if (string.IsNullOrEmpty(unityWebRequest.downloadHandler.text))
            {
                callback(null, false);
                Debug.LogError("content is empty " + unityWebRequest.responseCode);
                yield break;
            }
            
            ChatResponse chatResponse = JsonUtility.FromJson<ChatResponse>(unityWebRequest.downloadHandler.text);

            if (chatResponse?.choices?.Length > 0)
            {
                for (int i = 0; i < chatResponse.choices.Length; i++)
                {
                    string responseMessage = chatResponse.GetContent(i);
                    //维护历史消息
                    messageHistory.Add(currentMessage);
                    messageHistory.Add(new Message(responseMessage, ChatRequest.Role.Assistant));
                }
            }
            
            callback(chatResponse, true);
        }

        public static void SetToken(string token)
        {
            TOKEN = token;
        }
    }
}
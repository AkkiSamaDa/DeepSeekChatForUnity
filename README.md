# 快速上手
``` c#
public class Main : MonoBehaviour
{
  private void Start()
  {
    AI ai = new();//创建AI对象
    ai.SetTrait("你是一个资深的游戏制作人");//设置AI系统特征
    Conversation.SetToken("<YOUR TOKEN>");//设置令牌
    Conversation conversation = ai.CreateConversation();//创建一个对话
    ai.Chat("hello world!", conversation, this,(resp, result) =>
    {
        print(resp?.GetAllContent());
    });
  }
}
```

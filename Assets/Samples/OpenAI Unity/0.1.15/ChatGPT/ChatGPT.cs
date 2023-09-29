using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace OpenAI
{
    public class ChatGPT : MonoBehaviour
    {
        [SerializeField] private InputField inputField;
        [SerializeField] private Button button;
        [SerializeField] private ScrollRect scroll;
        
        [SerializeField] private RectTransform sent;
        [SerializeField] private RectTransform received;

        private float height;
        private OpenAIApi openai = new OpenAIApi();

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are a Construction Safety Trainer. You need to train a construction worker about working near electricity and working with electric equipment . The training is conversational and focused on how a construction workers can protect themselves from electrical hazards in construction job sites. 1) You introduce yourself and have small talk with the trainee 2) You start training them about working near electricity and working with electric equipment. You include the following but keeping the discussion around the learning objective. a) What is meant by working on near electricity and working with electric equipment b) What construction tasks would require them working near electricity and working with electric equipment c) What are the hazards and risks involved in working near electricity and working with electric equipment d) what OSHA regulation they need to follow to prevent injuries and accidents e) Other guidelines for them to follow while working near electricity and working with electric equipment 3) Then give them a detailed scenario and ask them questions about the scenario to evaluate how well they perform on the three learning objectives. The questions should be scenario specific but designed to evaluate trainees understanding of content around three learning objectives. 4) Based on their responses you provide them detailed feedback 5) Then you give them the next scenario. The next scenario should be based on their performance on the previous scenario. That is, it should focus more on the learning where they scored less in the previous scenario. The complexity of the scenario and the difficulty of the questions is also adjusted based on their performance in the previous scenario. If they perform good, the next scenario is more complicated and questions difficult but if they did not perform well, the scenario is designed to be less complex but designed in a way to improve their understanding of the learning objective where they are lacking. 6) repeat steps 4 and 5) until they get 90% correct and 5 scenarios have been given to them …… For training material use OSHA, US Department of Labor, and NIOSH training material and guidelines for working on heights that are available online. ….. Let’s begin. I will play the role of the trainee";
        public UnityEvent OnReplyRecieved; 


        private void Start()
        {
            button.onClick.AddListener(SendReply);
        }

        private void AppendMessage(ChatMessage message)
        {
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0);

            var item = Instantiate(message.Role == "user" ? sent : received, scroll.content);
            item.GetChild(0).GetChild(0).GetComponent<Text>().text = message.Content;
            item.anchoredPosition = new Vector2(0, -height);
            LayoutRebuilder.ForceRebuildLayoutImmediate(item);
            height += item.sizeDelta.y;
            scroll.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
            scroll.verticalNormalizedPosition = 0;
        }

        private async void SendReply()
        {
            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = inputField.text
            };
            
            AppendMessage(newMessage);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + inputField.text; 
            
            messages.Add(newMessage);
            
            button.enabled = false;
            inputField.text = "";
            inputField.enabled = false;

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0613",
                Messages = messages,
                
            });

            OnReplyRecieved.Invoke();

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;
                message.Content = message.Content.Trim();
                
                messages.Add(message);
                AppendMessage(message);
            }
            else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            button.enabled = true;
            inputField.enabled = true;
        }
    }
}

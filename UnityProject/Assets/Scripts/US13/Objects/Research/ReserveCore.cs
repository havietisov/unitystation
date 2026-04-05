using UnityEngine;
using US13.Core.Chat;
using Util;

namespace US13.Objects.Research
{
    public class ReserveCore : MonoBehaviour
    {
        public static ReserveCore Instance { get; private set; }


        //temporary solution
        public static void AddAsComponentToObject(GameObject obj)
        {
            if (Instance == null)
            {
                var core = obj.AddComponent<ReserveCore>();
                ReserveCore.Instance = core;
            }

            Chat.AddLocalMsgToChat(
                message: "RESERVE CORE ONLINE.",
                originator: obj,
                language: null,
                speakerName: "Reserve Core",
                doSpeechBubble: false
            );
        }
            
        private void OnEnable()
        {
            ChatRelay.OnLocalChatProcessed += OnLocalChat;
        }
        private void OnDisable()
        {
            ChatRelay.OnLocalChatProcessed -= OnLocalChat;
        }
        private void OnLocalChat(ChatEvent chatEvent)
        {
            if (!Mirror.NetworkServer.active) return;
            if (chatEvent.originator == gameObject) return; // don't hear yourself
            
            // Distance check (same 14f as NPC hearing)
            var myPos = transform.gameObject.AssumedWorldPosServer();
            //if (Vector2.Distance(chatEvent.position, myPos) > 14f) return;
            
            // You have: chatEvent.speaker, chatEvent.message, chatEvent.channels
            // Send to your API, queue response, etc.
            Debug.Log($"ReserveCore received chat: {chatEvent.message} from {chatEvent.speaker} on {chatEvent.channels}");

            Chat.AddLocalMsgToChat(
                message: "RESERVE CORE RESPONDS TO: " + chatEvent.message,
                originator: gameObject,
                language: null,
                speakerName: "Reserve Core",
                doSpeechBubble: true,
                worldPos: chatEvent.originator.AssumedWorldPosServer()
            );
        }
    }
}
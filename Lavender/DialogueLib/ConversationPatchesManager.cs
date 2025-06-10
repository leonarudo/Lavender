using System;
using System.Collections.Generic;

namespace Lavender.DialogueLib
{
    public class ConversationPatchesManager
    {
        private const int NewDialogueEntryStartingID = 10000;
        private static int NextDialogueEntryID = NewDialogueEntryStartingID;

        public static List<ConversationPatcher> Conversations = new List<ConversationPatcher>();
        public static Dictionary<string, List<ConversationPatcher>> ConversationToPatcher = new Dictionary<string, List<ConversationPatcher>>();

        private static ConversationPatchesManager _instance = null!;

        public static ConversationPatchesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ConversationPatchesManager();
                }

                return _instance;
            }
        }

        private ConversationPatchesManager() 
        {
            SaveController.LoadingStarted += OnLoadingStarted;
            SaveController.LoadingDone += OnLoadingDone;
        }

        ~ConversationPatchesManager()
        {
            SaveController.LoadingStarted -= OnLoadingStarted;
            SaveController.LoadingDone -= OnLoadingDone;
        }

        /// <summary>
        /// You probably want Lavender.AddConversationPatcher().
        /// </summary>
        /// <param name="patcher"></param>
        public void AddConversationPatcher(ConversationPatcher patcher)
        {
            if (!Conversations.Contains(patcher))
            {
                Conversations.Add(patcher);

                LavenderLog.DialogueVerbose(patcher.ConversationName, $"Registered conversation patcher {patcher.GetType().Name} for conversation {patcher.ConversationName}");

                // Track which patchers are modifying which conversations
                if (ConversationToPatcher.ContainsKey(patcher.ConversationName))
                {
                    ConversationToPatcher[patcher.ConversationName].Add(patcher);
                }
                else
                {
                    ConversationToPatcher.Add(patcher.ConversationName, new List<ConversationPatcher>([patcher]));
                }

                // If we're currently in a gameplay state, then immediately run the patch.
                // Otherwise, it will be patched once the game loads a gameplay scene.
                if (Lavender.instance.LoadingDone && Lavender.instance.lastLoadedScene != 0)
                {
                    patcher.TryPatchDialogue();
                }
            }
        }

        /// <summary>
        /// You probably want Lavender.GetPatchersForConversation().
        /// </summary>
        /// <param name="conversationName"></param>
        /// <returns></returns>
        public IEnumerable<ConversationPatcher> GetPatchersForConversation(string conversationName)
        {
            if (ConversationToPatcher.TryGetValue(conversationName, out var patchers))
            {
                return patchers;
            }
            return [];
        }

        // Single access point to generate DialogueEntry IDs
        // Allows us to dynamically avoid duplicate IDs at runtime
        // Assumes that NewDialogueEntryStartingID is a high enough number that the vanilla dialogues don't reach it
        //  If we start getting collisions with vanilla dialogues, we need to increase NewDialogueEntryStartingID
        public int GetNextDialogueEntryID()
        {
            // Intentional that this is a post-op increment
            // First ID used is equal to NewDialogueEntryStartingID
            return NextDialogueEntryID++;
        }

        private void OnLoadingStarted()
        {
            foreach (var patcher in Conversations)
            {
                patcher.ConversationDBReloading();
            }
        }

        private void OnLoadingDone()
        {
            LavenderLog.DialogueVerboseNoConversation($"Scene loaded, reset dialogue patcher DialogueEntry IDs back to {NewDialogueEntryStartingID} and running all registered conversation patchers.");
            NextDialogueEntryID = NewDialogueEntryStartingID;
            foreach (var patcher in Conversations)
            {
                try
                {
                    patcher.TryPatchDialogue();
                }
                catch (Exception e)
                {
                    LavenderLog.Error($"Exception while patching conversation \"{patcher.ConversationName}\": " + e.ToString());
                }
            }
        }
    }
}

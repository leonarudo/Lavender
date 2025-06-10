using Mono.Cecil.Cil;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Lavender.DialogueLib
{
    public static class ConversationUtils
    {
        /// <summary>
        /// Find DialogueEntry(s) that are direct and immediate responses to the provided entry
        /// </summary>
        /// <param name="conversation">Conversation containing the interaction</param>
        /// <param name="dialogueEntry">A DialogueEntry belonging to the provided Conversation</param>
        /// <returns>0 or more DialogueEntry(s) responding.  Note that it is possible for a player to respond to themself, or an NPC to respond to themself</returns>
        public static IEnumerable<DialogueEntry> GetResponsesTo(Conversation conversation, DialogueEntry dialogueEntry)
        {
            return dialogueEntry.outgoingLinks.Where((Link l) =>
            {
                return l.originConversationID == conversation.id && l.destinationConversationID == conversation.id && l.originDialogueID == dialogueEntry.id;
            }).Select((Link l) =>
            {
                return conversation.GetDialogueEntry(l.destinationDialogueID);
            });
        }

        /// <summary>
        /// Find DialogueEntry(s) that are direct and immediate responses to the provided entry.  Excludes any that have a userScript, condition, or dynamic text
        /// </summary>
        /// <param name="conversation">Conversation containing the interaction</param>
        /// <param name="dialogueEntry">A DialogueEntry belonging to the provided Conversation</param>
        /// <returns>0 or more DialogueEntry(s) responding.  Note that it is possible for a player to respond to themself, or an NPC to respond to themself</returns>
        public static IEnumerable<DialogueEntry> GetStaticResponsesTo(Conversation conversation, DialogueEntry dialogueEntry)
        {
            return dialogueEntry.outgoingLinks.Where((Link l) =>
            {
                if (l.originConversationID == conversation.id && l.destinationConversationID == conversation.id && l.originDialogueID == dialogueEntry.id)
                {
                    DialogueEntry target = conversation.GetDialogueEntry(l.destinationDialogueID);
                    if (target.userScript != "" || target.conditionsString != "" || target.currentDialogueText.Contains("[lua("))
                    {
                        return false;
                    }

                    return true;
                }

                return false;

            }).Select((Link l) =>
            {
                return conversation.GetDialogueEntry(l.destinationDialogueID);
            });
        }

        /// <summary>
        /// Find the soonest DialogueEntry(s) accessed from the provided entry that we can add a player response to (and the player response will be selectable ingame)
        /// May return entries that are not linked to directly by this DialogueEntry - this happens when in a portion of the conversation that will auto-advance through entries (ie a monologue).
        /// If there are somehow multiple valid results (f.ex branching dialogue) then we will only return the ones with a matching node distance, and only the closest distance.
        /// </summary>
        /// <param name="conversation">Conversation containing the interaction</param>
        /// <param name="entry">The DialogueEntry where we would prefer to add player response.  Note that it may not be possible to respond directly to this entry.</param>
        /// <returns>1 or more DialogueEntry where the responses can be linked *from*/as a source, and be visible ingame.</returns>
        public static IEnumerable<DialogueEntry> AdvanceToRespondable(Conversation conversation, DialogueEntry entry)
        {
            List<DialogueEntry> visited = new List<DialogueEntry>();
            List<DialogueEntry> results = new List<DialogueEntry>();

            Stack<DialogueEntry> pendingCurrentDepth = new Stack<DialogueEntry>();
            Stack<DialogueEntry> pendingNextDepth = new Stack<DialogueEntry>();

            pendingCurrentDepth.Push(entry);

            while (pendingCurrentDepth.Any())
            {
                entry = pendingCurrentDepth.Pop();
                visited.Add(entry);

                bool advance = false;
                // If there is an NPC response to our current entry, then the dialogue system will automatically take it and there will be no chance for the player to speak
                if (DoResponsesIncludeNPC(conversation, entry))
                {
                    advance = true;
                }
                // If any response is a sequenced node, then we cannot respond in this node because execution will fast-track to the sequence
                if (GetResponsesTo(conversation, entry).Except(visited).Where((DialogueEntry de) => de.Sequence != null && de.Sequence.Length != 0).Any())
                {
                    advance = true;
                }

                if (advance && results.Count == 0)
                {
                    foreach (DialogueEntry de in GetResponsesTo(conversation, entry).Except(visited))
                    {
                        pendingNextDepth.Push(de);
                    }

                    // If we have exhausted all options at our current depth, then time to start looking at the next depth
                    if (!pendingCurrentDepth.Any())
                    {
                        pendingCurrentDepth = pendingNextDepth;
                        pendingNextDepth = new Stack<DialogueEntry>();
                    }
                }
                else
                {
                    results.Add(entry);
                }
            }
            
            return results;
        }

        /// <summary>
        /// Check if any of the responses to the supplied DialogueEntry are spoken by an NPC
        /// </summary>
        /// <param name="conversation">Conversation containing the interaction</param>
        /// <param name="entry">DialogueEntry we are querying.  Note that the speaker of this entry does not matter, we are looking at who is speaking in response to it.</param>
        /// <returns>true/false</returns>
        public static bool DoResponsesIncludeNPC(Conversation conversation, DialogueEntry entry)
        {
            foreach (var l in entry.outgoingLinks)
            {
                DialogueEntry de = conversation.GetDialogueEntry(l.destinationDialogueID);
                if (de != null && de.ActorID != 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

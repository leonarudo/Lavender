using PixelCrushers.DialogueSystem;

namespace Lavender.DialogueLib
{
    /// <summary>
    /// Helper class for more accurate control over insertion of links between dialogue entries.
    /// The static initializer functions should provide configurations that handle most common usecases.
    /// </summary>
    public class LinkOrdering
    {
        public DialogueEntry? AfterEntry = null;
        public DialogueEntry? BeforeEntry = null;

        public Link? AfterLink = null;
        public Link? BeforeLink = null;

        public bool PreferLater = true;
        public bool PreferLaterIfMatched = true;
        public bool PreferBeforeGoodbye = false;

        /// <summary>
        /// Create a LinkOrdering instance that prefers to place the link last in the available options, but before any "Goodbye"/"(Leave)" option
        /// May miss custom goodbye options, in which case you may need to use BeforeDialogue
        /// </summary>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering DefaultOrdering()
        {
            return new LinkOrdering() { PreferBeforeGoodbye = true, PreferLaterIfMatched = true, PreferLater = true };
        }

        /// <summary>
        /// Create a LinkOrdering instance that prefers to place the link first in the list.
        /// Note: While it will place it first, it places it first at the moment that the link is created.  Overuse of this preset will cause links to get shuffled around.  
        /// There can be only 1 first.
        /// </summary>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering First()
        {
            return new LinkOrdering() { PreferLater = false };
        }

        /// <summary>
        /// Create a LinkOrdering instance that guarantees the link will be placed before the specified response
        /// Will attempt to place the link directly before the response, if possible
        /// </summary>
        /// <param name="entry">The entry to be in front of in the response list</param>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering BeforeDialogue(DialogueEntry entry)
        {
            return new LinkOrdering() { BeforeEntry = entry, PreferLater = false, PreferLaterIfMatched = true };
        }

        /// <summary>
        /// Create a LinkOrdering instance that guarantees the link will be placed after the specified response
        ///  Will attempt to place the link directly after the response, if possible
        /// </summary>
        /// <param name="entry">The entry to put the response after</param>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering AfterDialogue(DialogueEntry entry)
        {
            return new LinkOrdering() { AfterEntry = entry, PreferLater = true, PreferLaterIfMatched = false };
        }

        /// <summary>
        /// Create a LinkOrdering instance that guarantees the link will be placed before the provided link
        /// Will attempt to place the new link directly before the provided link, if possible
        /// Effectively an alias for BeforeDialogue().
        /// </summary>
        /// <param name="link">The link to be in front of in the response list</param>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering BeforeResponse(Link link)
        {
            return new LinkOrdering() { BeforeLink = link, PreferLater = false, PreferLaterIfMatched = true };
        }

        /// <summary>
        /// Create a LinkOrdering instance that guarantees the link will be placed after the specified link
        /// Will attempt to place the new link directly after the provided link, if possible
        /// Effectively an alias for AfterDialogue().
        /// </summary>
        /// <param name="link">The link to put the response after</param>
        /// <returns>A LinkOrdering instance ready for use with ConversationPatcher.Link()</returns>
        public static LinkOrdering AfterResponse(Link link)
        {
            return new LinkOrdering() { AfterLink = link, PreferLater = true, PreferLaterIfMatched = false };
        }

        /// <summary>
        /// Handles the linking logic.  Not for external use - called automatically by the library.
        /// Function is exposed should you want to implement your own custom linking logic:
        /// 1) Subclass LinkOrdering
        /// 2) Override AttachToDialogueEntry to implement your logic
        /// 3) Create and pass an instance of your class to ConversationPatcher.Link()
        /// </summary>
        /// <param name="conversation">The interaction the linking happens within</param>
        /// <param name="source">The DialogueEntry that is visible on the screen</param>
        /// <param name="dest">The DialogueEntry that you are taken to when the response is selected (or automatically if NPC -> NPC or using a Sequence)</param>
        /// <param name="newLink">The Link object that needs to be inserted into the source.outgoingLinks List.</param>
        public virtual void AttachToDialogueEntry(Conversation conversation, DialogueEntry source, DialogueEntry dest, Link newLink)
        {
            int minIndex = 0;
            int maxIndex = source.outgoingLinks.Count;
            bool matched = false;

            if (BeforeLink != null || AfterLink != null || BeforeEntry != null || AfterEntry != null || PreferBeforeGoodbye)
            {
                for (int i = 0; i < source.outgoingLinks.Count; i++)
                {
                    Link link = source.outgoingLinks[i];
                    if (link == BeforeLink)
                    {
                        maxIndex = i;
                        matched = true;
                    }
                    if (link == AfterLink)
                    {
                        minIndex = i + 1;
                        matched = true;
                    }

                    if (BeforeEntry != null && BeforeEntry.id == link.destinationDialogueID)
                    {
                        maxIndex = i;
                        matched = true;
                    }
                    if (AfterEntry != null && AfterEntry.id == link.destinationDialogueID)
                    {
                        minIndex = i + 1;
                        matched = true;
                    }

                    if (PreferBeforeGoodbye)
                    {
                        DialogueEntry target = conversation.GetDialogueEntry(link.destinationDialogueID);
                        if (target != null &&
                            (target.DialogueText.Contains("(Leave)") || target.DialogueText == "Goodbye" || target.DialogueText == "Goodbye."))
                        {
                            LavenderLog.DialogueVerbose(conversation.Title, $"  Found goodbye dialog at index {i}: {target.DialogueText} (id: {target.id})");
                            maxIndex = i;
                            matched = true;
                        }
                    }
                }
            }

            if (minIndex > maxIndex)
            {
                LavenderLog.Error($"Impossible link ordering criteria - it is trying to be in 2 places at once!  Look into this please, conversation: {conversation.Name}, link source: \"{source.DialogueText}\", link target: \"{dest.DialogueText}\", minIndex={minIndex}, maxIndex={maxIndex}");
                minIndex = maxIndex;
            }

            int idx = 0;
            if (matched)
            {
                idx = PreferLaterIfMatched ? maxIndex : minIndex;
            }
            else
            {
                idx = PreferLater ? maxIndex : minIndex;
            }

            source.outgoingLinks.Insert(idx, newLink);

            LavenderLog.DialogueVerbose(conversation.Title, $"Inserted link [{newLink.originConversationID}:{newLink.originDialogueID} => {newLink.destinationConversationID}:{newLink.destinationDialogueID}] " + 
                $"to {source.DialogueText} ({source.id}) at index {idx} / {source.outgoingLinks.Count}");
        }
    }
}

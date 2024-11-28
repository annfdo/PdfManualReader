namespace PdfManualReader
{
    public class StaffBackendClient
    {
        public record AssistantChatReplyItem(AssistantChatReplyItemType Type, string Text, int? SearchResultId = null, int? SearchResultProductId = null, int? SearchResultPageNumber = null);

        public enum AssistantChatReplyItemType { AnswerChunk, Search, SearchResult, IsAddressedToCustomer };



    }

    public record AssistantChatRequest(
        int? ProductId,
        string? CustomerName,
        string? TicketSummary,
        string? TicketLastCustomerMessage,
        IReadOnlyList<AssistantChatRequestMessage> Messages);


    public class AssistantChatRequestMessage
    {
        public bool IsAssistant { get; set; }
        public required string Text { get; set; }
    }

}

using System.Collections.Generic;

namespace IR_SearchEngine.Core.DTOs
{
    public class SearchResponseDto
    {
        public List<DocumentResultDto> Documents { get; set; } = new List<DocumentResultDto>();
        public List<string> ProcessingSteps { get; set; } = new List<string>();
        public List<string> SuggestedTerms { get; set; } = new();
        public int TotalResults { get; set; }
    }

    public class DocumentResultDto
    {
        public int DocId { get; set; }
        public string Content { get; set; } = null!;
    }
}
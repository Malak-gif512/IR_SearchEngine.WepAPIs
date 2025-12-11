using IR_SearchEngine.Core.Enums;
namespace IR_SearchEngine.Core.DTOs
{
    public class SearchRequestDto
    {
        public string Query { get; set; } = null!;
        public SearchType SearchType { get; set; }
    }
}
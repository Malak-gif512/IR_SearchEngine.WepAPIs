using System.Collections.Generic;

namespace IR_SearchEngine.Core.Interfaces
{
    public interface ITextProcessor
    {
        List<(string term, int position)> AnalyzeWithPositions(string text);
        List<string> Analyze(string text, out List<string> logs);
        string ApplyStemming(string word);
    }
}
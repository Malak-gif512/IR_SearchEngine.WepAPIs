using System.Collections.Generic;

namespace IR_SearchEngine.Core.Interfaces
{
    public interface IDataRepository
    {
        Dictionary<int, string> GetAllDocuments();
        void AddDocument(int id, string content);

        // Inverted Index Operations
        void AddToInvertedIndex(string term, int docId);
        HashSet<int> GetInvertedIndex(string term);
        IEnumerable<string> GetInvertedIndexKeys(); // Required for Soundex scan

        // Positional Index Operations
        void AddToPositionalIndex(string term, int docId, int position);
        Dictionary<int, List<int>> GetPositionalIndex(string term);
    }
}
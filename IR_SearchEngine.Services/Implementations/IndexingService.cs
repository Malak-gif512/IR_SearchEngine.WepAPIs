using IR_SearchEngine.Core.Interfaces;
using System.Linq;

namespace IR_SearchEngine.Services.Implementations
{
    public class IndexingService : IIndexingService
    {
        private readonly IDataRepository _repo;
        private readonly ITextProcessor _processor;

        public IndexingService(IDataRepository repo, ITextProcessor processor)
        {
            _repo = repo;
            _processor = processor;
        }

        public void IndexAllDocuments()
        {
            foreach (var doc in _repo.GetAllDocuments()) IndexDocument(doc.Key, doc.Value);
        }

        public void IndexDocument(int id, string content)
        {
            // 1. تخزين النص الأصلي
            _repo.AddDocument(id, content);

            // 2. استخدام الدالة الجديدة اللي بترجع الكلمات وأماكنها
            var tokensWithPos = _processor.AnalyzeWithPositions(content);

            // 3. التخزين في الفهارس
            foreach (var item in tokensWithPos)
            {
                string term = item.term;     // الكلمة المعالجة (Stemmed)
                int position = item.position; // مكانها الأصلي

                // Inverted Index
                _repo.AddToInvertedIndex(term, id);

                // Positional Index
                _repo.AddToPositionalIndex(term, id, position);
            }
        }


        public Dictionary<int, string> GetAllDocuments()
        {
            // بنجيب الداتا من الريبو مباشرة
            return _repo.GetAllDocuments();
        }
    }
}
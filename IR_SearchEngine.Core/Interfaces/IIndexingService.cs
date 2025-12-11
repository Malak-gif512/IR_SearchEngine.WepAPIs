using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_SearchEngine.Core.Interfaces
{
   public interface IIndexingService
    {
        Dictionary<int, string> GetAllDocuments();
        void IndexAllDocuments();
        void IndexDocument(int id, string content);
    }
}

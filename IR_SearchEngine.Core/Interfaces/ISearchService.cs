using IR_SearchEngine.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IR_SearchEngine.Core.Interfaces
{
   public interface ISearchService
    
    {
        SearchResponseDto Search(SearchRequestDto request);
      
    }
}

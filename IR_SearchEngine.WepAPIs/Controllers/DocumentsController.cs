using IR_SearchEngine.Core.DTOs;
using IR_SearchEngine.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IR_SearchEngine.WepAPIs.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly IIndexingService _indexingService;

        // بنطلب خدمة الفهرسة، مش محتاجين السيرش هنا
        public DocumentsController(IIndexingService indexingService)
        {
            _indexingService = indexingService;
        }

        // Endpoint: POST api/Documents
        [HttpPost]
        public IActionResult UploadDocument([FromBody] DocumentUploadDto dto)
        {
            // 1. التحقق إن المحتوى مش فاضي
            if (string.IsNullOrWhiteSpace(dto.Content))
            {
                return BadRequest("Content cannot be empty.");
            }

            // 2. المنطق الذكي لتوليد الـ ID
            if (dto.Id == 0)
            {
                // هات كل الأرقام الموجودة
                var allDocs = _indexingService.GetAllDocuments();

                // لو مفيش ولا ملف، ابدأ بـ 1
                // لو فيه، هات أكبر رقم وزود عليه 1
                int newId = allDocs.Count > 0 ? allDocs.Keys.Max() + 1 : 1;

                // اعتمد الرقم الجديد
                dto.Id = newId;
            }
            else
            {
                // لو هو باعت رقم معين، اتأكد إنه مش محجوز عشان ميمسحش القديم
                if (_indexingService.GetAllDocuments().ContainsKey(dto.Id))
                {
                    return BadRequest($"Error: Document ID {dto.Id} is already taken. Please send '0' to auto-generate.");
                }
            }

            // 3. التنفيذ: فهرسة المستند بالرقم النهائي
            _indexingService.IndexDocument(dto.Id, dto.Content);

            // 4. الرد: لازم نرجع الـ generatedId عشان الموبايل يعرف الملف راح فين
            return Ok(new
            {
                message = "Document indexed successfully!",
                generatedId = dto.Id
            });
        }

    }
}

using IR_SearchEngine.Core.Interfaces;
using IR_SearchEngine.Infrastructure.Repositories;
using IR_SearchEngine.Services.Implementations;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// ---------------------------------------------------------
// 1.  ”ÃÌ· Œœ„«  Swagger («·Ã“¡ «·‰«ﬁ’)
// ---------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// ---------------------------------------------------------

// 2.  ”ÃÌ· «·‹ Repositories & Services (“Ì „« ﬂ‰« ⁄«„·Ì‰)
builder.Services.AddSingleton<IDataRepository, DataRepository>();
builder.Services.AddScoped<ITextProcessor, TextProcessor>();
builder.Services.AddScoped<IIndexingService, IndexingService>();
builder.Services.AddScoped<ISearchService, SearchService>();

var app = builder.Build();

// ---------------------------------------------------------
// 3.  ›⁄Ì· Swagger Middleware (⁄‘«‰ Ì› Õ ›Ì «·„ ’›Õ)
// ---------------------------------------------------------
if (app.Environment.IsDevelopment())
{
   
}
app.UseSwagger();
app.UseSwaggerUI();
// ---------------------------------------------------------

// 4.  ‘€Ì· «·‹ Indexing ⁄‰œ «·»œ«Ì…
using (var scope = app.Services.CreateScope())
{
    var indexer = scope.ServiceProvider.GetRequiredService<IIndexingService>();
    indexer.IndexAllDocuments();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
using ExampleWebApi.Models;
using Lucene.Net.Documents;
using WebTruss.Search;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var bookIndexConfig = new LuceneIndexConfig
{
    KeyPropertyName = nameof(Book.Id),
    Path = "C:\\Users\\salma\\Desktop\\Lucene",
    PropertyConfigs = new List<LuceneIndexConfig.PropertyConfig>
    {
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Title), Field.Store.YES, Field.Index.ANALYZED),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Description), Field.Store.YES, Field.Index.NO),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Author), Field.Store.YES, Field.Index.ANALYZED),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.ISBN), Field.Store.YES, Field.Index.NOT_ANALYZED),
    }
};
var bookIndex = new LuceneIndex<Book>(bookIndexConfig);

builder.Services.AddSingleton(bookIndex);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

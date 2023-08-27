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
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Title), true),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Description), false),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.Author), true),
        new LuceneIndexConfig.PropertyConfig(nameof(Book.ISBN), false),
    }
};;
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

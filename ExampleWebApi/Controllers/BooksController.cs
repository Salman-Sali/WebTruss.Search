using ExampleWebApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebTruss.Search;

namespace ExampleWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly LuceneIndex<Book> bookSearch;

        public BooksController(LuceneIndex<Book> bookSearch)
        {
            this.bookSearch = bookSearch;
        }

        [HttpPost]
        public IActionResult Add(Book book)
        {
            bookSearch.Put(book);
            return this.Ok();
        }

        [HttpGet]
        public IActionResult Get(string query)
        {
            var config = new SearchConfig
            {
                Query = query,
                Count = 10,
                TargetProperty = nameof(Book.Title),
                Fuzzy = true,
            };
            return this.Ok(bookSearch.Search(config));
        }
    }
}

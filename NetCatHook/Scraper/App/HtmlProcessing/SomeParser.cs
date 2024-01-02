using AngleSharp;
using AngleSharp.Dom;

namespace NetCatHook.Scraper.App.Parsing;

class SomeParser
{
    public string DoSome(int val)
    {
        return $"str-{val}";
    }

    public async Task<string?> ParseHtml(string html)
    {
        //Create a new context for evaluating webpages with the default config
        IBrowsingContext context = BrowsingContext.New(Configuration.Default);

        // TODO: async
        //Create a document from a virtual request / response pattern
        IDocument document = await context.OpenAsync(req => req.Content(html));

        //Do something with LINQ
        //IEnumerable<IElement> blueListItemsLinq = document.All.Where(m => m.LocalName == "li" && m.ClassList.Contains("blue"));

        //Or directly with CSS selectors
        IHtmlCollection<IElement> elements = document.QuerySelectorAll("a");

        return elements.FirstOrDefault()?.GetAttribute("href");
    }

}

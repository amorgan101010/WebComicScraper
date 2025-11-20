// See https://aka.ms/new-console-template for more information
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

var previousUrl = "";
var currentUrl = "https://www.nuklearpower.com/2001/03/02/episode-001-were-going-where/";
var finalUrl = "https://www.nuklearpower.com/2010/06/01/the-epilogue/";
//const string finalUrl = "https://www.nuklearpower.com/2001/08/19/episode-069-thief-is-one-slick-mo-fo/";

var webdriverOptions = new ChromeOptions();
webdriverOptions.AddArgument("--headless=new");
IWebDriver driver = new ChromeDriver(webdriverOptions);
using HttpClient httpClient = new();
var episodes = new List<Episode>();
var index = 0;
do
{
    driver.Navigate().GoToUrl(currentUrl); // TODO: avoid possible nullref
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

    var comicDiv = driver.FindElement(By.Id("comic"));
    var comic = comicDiv.FindElement(By.TagName("img"));
    var comicUrl = comic.GetAttribute("src");
    var comicName = comic.GetAttribute("title");

    var episode = await DownloadComicAsync(httpClient, comicUrl, comicName); // TODO: avoid possible nullref
    episodes.Add(episode);

    var nextButton = driver.FindElement(By.ClassName("navbar-next"));
    var nextLink = nextButton.FindElement(By.TagName("a"));
    previousUrl = currentUrl;
    currentUrl = nextLink.GetAttribute("href");

    index++;
}
while (previousUrl != finalUrl);

// Based on https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/images/MultipleImages.cs
Image image = new Image(ImageDataFactory.Create(episodes.First().Path));
PdfDocument pdfDoc = new PdfDocument(new PdfWriter(Directory.GetCurrentDirectory() + "/compiled.pdf"));
Document doc = new Document(pdfDoc, new PageSize(image.GetImageWidth(), image.GetImageHeight()));

foreach (var episode in episodes)
{
    image = new Image(ImageDataFactory.Create(episode.Path));
    var pageSize = new PageSize(image.GetImageWidth(), image.GetImageHeight());
    pdfDoc.AddNewPage(pageSize);
    doc.Add(image);

    // Add header
    // Adapted from https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/events/VariableHeader.cs
    var page = pdfDoc.GetLastPage();
    var header = episode.Name;
    var debugPageSize = page.GetPageSize();
    var yOffset = 20;
    new Canvas(page, pageSize)
                    .ShowTextAligned(
                        header,
                        30,
                        pageSize.GetHeight() - yOffset,
                        null)
                    .Close();
}

doc.Close();

async Task<Episode> DownloadComicAsync(HttpClient httpClient, string comicUrl, string comicName)
{
    var episode = new Episode(comicName, index);
    // adapted from https://www.reddit.com/r/csharp/comments/11r7o1d/comment/jc6zy2g/
    byte[] imageBytes = await httpClient.GetByteArrayAsync(comicUrl);

    File.WriteAllBytes(episode.Path, imageBytes);

    return episode;
}

public class Episode
{
    public string Name {get;set;}
    public int Index {get;set;}
    public string Path {get;set;}
    public Episode(string name, int index)
    {
        string projectDir = Directory.GetCurrentDirectory();
        Name = name;
        Index = index;
        Path = System.IO.Path.Combine(projectDir, IndexAsString() + ".png");
    }
    public string IndexAsString()
    {
        return Index.ToString("D4");
    }
}
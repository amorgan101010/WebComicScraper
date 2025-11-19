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

var index = 0;
do
{
    driver.Navigate().GoToUrl(currentUrl); // TODO: avoid possible nullref
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

    var comicDiv = driver.FindElement(By.Id("comic"));
    var comic = comicDiv.FindElement(By.TagName("img"));
    var comicUrl = comic.GetAttribute("src");
    var comicName = comic.GetAttribute("title");

    await DownloadComicAsync(httpClient, comicUrl, comicName); // TODO: avoid possible nullref

    var nextButton = driver.FindElement(By.ClassName("navbar-next"));
    var nextLink = nextButton.FindElement(By.TagName("a"));
    previousUrl = currentUrl;
    currentUrl = nextLink.GetAttribute("href");

    index++;
}
while (previousUrl != finalUrl);

var imagePaths = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
    .Where(path => path.Contains(".png")).ToList();

imagePaths.Sort();

// Based on https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/images/MultipleImages.cs
Image image = new Image(ImageDataFactory.Create(imagePaths.First()));
PdfDocument pdfDoc = new PdfDocument(new PdfWriter(Directory.GetCurrentDirectory() + "/compiled.pdf"));
Document doc = new Document(pdfDoc, new PageSize(image.GetImageWidth(), image.GetImageHeight()));

foreach (var imagePath in imagePaths)
{
    image = new Image(ImageDataFactory.Create(imagePath));
    var pageSize = new PageSize(image.GetImageWidth(), image.GetImageHeight());
    pdfDoc.AddNewPage(pageSize);
    doc.Add(image);

    // Add header
    // Adapted from https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/events/VariableHeader.cs
    var page = pdfDoc.GetLastPage();
    var header = imagePath // I need to learn regex...
        .Split("/")
        .Last()
        .Split(".png")
        .First()
        .Substring(4);
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

async Task DownloadComicAsync(HttpClient httpClient, string comicUrl, string comicName)
{
    // adapted from https://www.reddit.com/r/csharp/comments/11r7o1d/comment/jc6zy2g/
    byte[] imageBytes = await httpClient.GetByteArrayAsync(comicUrl);

    string projectDir = Directory.GetCurrentDirectory();
    var indexStr = index.ToString("D4");
    string localPath = System.IO.Path.Combine(projectDir, indexStr + comicName + ".png");

    File.WriteAllBytes(localPath, imageBytes);
}

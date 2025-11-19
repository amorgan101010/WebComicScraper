// See https://aka.ms/new-console-template for more information
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Event;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

var currentUrl = "https://www.nuklearpower.com/2001/03/02/episode-001-were-going-where/";
//var finalUrl = "https://www.nuklearpower.com/2010/06/01/the-epilogue/";
const string finalUrl = "https://www.nuklearpower.com/2001/03/14/episode-005-run-heroes-run/";

var webdriverOptions = new ChromeOptions();
webdriverOptions.AddArgument("--headless=new");
IWebDriver driver = new ChromeDriver(webdriverOptions);

do
{
    driver.Navigate().GoToUrl(currentUrl); // TODO: avoid possible nullref
    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

    var comicDiv = driver.FindElement(By.Id("comic"));
    var comic = comicDiv.FindElement(By.TagName("img"));
    var comicUrl = comic.GetAttribute("src");
    var comicName = comic.GetAttribute("title");
    await DownloadComicAsync(comicUrl, comicName); // TODO: avoid possible nullref

    var nextButton = driver.FindElement(By.ClassName("navbar-next"));
    var nextLink = nextButton.FindElement(By.TagName("a"));
    currentUrl = nextLink.GetAttribute("href");

}
while (currentUrl != finalUrl);

var imagePaths = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
    .Where(path => path.Contains(".png"));

// Based on https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/images/MultipleImages.cs
Image image = new Image(ImageDataFactory.Create(imagePaths.First()));
PdfDocument pdfDoc = new PdfDocument(new PdfWriter(Directory.GetCurrentDirectory() + "/compiled.pdf"));
Document doc = new Document(pdfDoc, new PageSize(image.GetImageWidth(), image.GetImageHeight()));

// TODO: As it is currently this isn't going to preserve the order of non-numbered strips...
// I need to tack on an absolute number to each image name and remove it when labeling the PDF.
foreach (var imagePath in imagePaths)
{
    image = new Image(ImageDataFactory.Create(imagePath));
    var pageSize = new PageSize(image.GetImageWidth(), image.GetImageHeight());
    pdfDoc.AddNewPage(pageSize);
    doc.Add(image);

    // Add header
    // Adapted from https://github.com/itext/itext-publications-samples-dotnet/blob/master/itext/itext.samples/itext/samples/sandbox/events/VariableHeader.cs
    var page = pdfDoc.GetLastPage();
    var header = imagePath.Split("/").Last().Split(".png").First();
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

static async Task DownloadComicAsync(string comicUrl, string comicName)
{
    // adapted from https://www.reddit.com/r/csharp/comments/11r7o1d/comment/jc6zy2g/
    using HttpClient httpClient = new();
    byte[] imageBytes = await httpClient.GetByteArrayAsync(comicUrl);

    string projectDir = Directory.GetCurrentDirectory();
    string localPath = System.IO.Path.Combine(projectDir, comicName + ".png");

    File.WriteAllBytes(localPath, imageBytes);
}

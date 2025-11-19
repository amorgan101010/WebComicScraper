// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

IWebDriver driver = new ChromeDriver();

var currentUrl = "https://www.nuklearpower.com/2001/03/02/episode-001-were-going-where/";
//var finalUrl = "https://www.nuklearpower.com/2010/06/01/the-epilogue/";
var finalUrl = "https://www.nuklearpower.com/2001/03/14/episode-005-run-heroes-run/";

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

// TODO: Stitch them into a PDF with the name on each page

static async Task DownloadComicAsync(string comicUrl, string comicName)
{
    // adapted from https://www.reddit.com/r/csharp/comments/11r7o1d/comment/jc6zy2g/
    using HttpClient httpClient = new();
    byte[] imageBytes = await httpClient.GetByteArrayAsync(comicUrl);

    string projectDir = Directory.GetCurrentDirectory();
    string localPath = Path.Combine(projectDir, comicName + ".png");

    File.WriteAllBytes(localPath, imageBytes);
}


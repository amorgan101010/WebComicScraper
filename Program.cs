// See https://aka.ms/new-console-template for more information
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Net.Http.Headers;

IWebDriver driver = new ChromeDriver();

var currentUrl = "https://www.nuklearpower.com/2001/03/02/episode-001-were-going-where/";
var finalUrl = "https://www.nuklearpower.com/2010/06/01/the-epilogue/";

do
{
    driver.Navigate().GoToUrl(currentUrl);

    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);

    var comicDiv = driver.FindElement(By.Id("comic"));
    var comic = comicDiv.FindElement(By.TagName("img"));
    var comicUrl = comic.GetAttribute("src");
    var comicName = comic.GetAttribute("title");
    await DownloadComicAsync(comicUrl, comicName); // TODO: avoid possible nullref

    //TODO: move on to the next URL
    currentUrl = finalUrl;
}
while (currentUrl != finalUrl);

static async Task DownloadComicAsync(string comicUrl, string comicName)
{
    // adapted from https://www.reddit.com/r/csharp/comments/11r7o1d/comment/jc6zy2g/
    using HttpClient httpClient = new();
    byte[] imageBytes = await httpClient.GetByteArrayAsync(comicUrl);

    string projectDir = Directory.GetCurrentDirectory();
    string localPath = Path.Combine(projectDir, comicName + ".png");

    File.WriteAllBytes(localPath, imageBytes);
}


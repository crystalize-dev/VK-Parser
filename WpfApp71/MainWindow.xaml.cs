﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Threading;

namespace WpfApp71
{
  /// <summary>
  /// Логика взаимодействия для MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private string[] getHrefs(IWebElement item)
    {
        List<IWebElement> webElementsHref = item.FindElements(By.TagName("a")).ToList(); // Ищем все теги ссылок
        List<string> Hrefs = new List<string>();

        foreach(var hrefs in webElementsHref) // Оставляем только нужные
        {
          if (hrefs == null)
            continue;
          if (hrefs.GetAttribute("target") != "_blank")
            continue;
          if (hrefs.GetAttribute("rel") != "nofollow noopener")
            continue;
        Hrefs.Add(hrefs.GetAttribute("href"));
        }

      return Hrefs.ToArray();
    }
        private string[] getImages(IWebElement item)
    {

      List<IWebElement> webElementsImages = item.FindElements(By.TagName("img")).ToList();
      List<string> images = new List<string>();
      List<IWebElement> webElementsImagesPack = item.FindElements(By.TagName("a")).ToList();
      List<string> imagesStyle = new List<string>();

      foreach (var temp in webElementsImagesPack)
      {
        if (temp.GetAttribute("aria-label") != "фотография")
          continue;
        imagesStyle.Add(temp.GetAttribute("style"));
      }

      if (imagesStyle.Count() == 1)
        imagesStyle.Clear();

      for(int i=0;i<imagesStyle.Count();i++)
      {
        int start = imagesStyle[i].IndexOf('(');
        int end = imagesStyle[i].IndexOf(')');

        imagesStyle[i] = imagesStyle[i].Substring(start + 1, end - start);
      }

      foreach(var tempimage in webElementsImages)
      {
        if (tempimage.GetAttribute("src") == null)
          continue;
       if (tempimage.GetAttribute("class").Trim() != "PagePostLimitedThumb")
          continue;
        images.Add(tempimage.GetAttribute("src"));
      }
      
      foreach(var tempimage in imagesStyle)
      {
        images.Add(tempimage);
      }

      return images.ToArray();

    }

    private void setDataInLabel(string option)
    {
      if (option == "text")
        label.Dispatcher.Invoke(new Action(() => label.Text = JSONWorker.readText()));
      if (option == "hrefs")
        label.Dispatcher.Invoke(new Action(() => label.Text = JSONWorker.readHrefs()));
      if (option == "images")
        label.Dispatcher.Invoke(new Action(() => label.Text = JSONWorker.readImages()));
    }

    private void Button_Click(object sender, RoutedEventArgs e)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument(@"user-data-dir=C:\Users\man21\AppData\Local\Google\Chrome\User Data");
            ChromeDriver chromeDriver = new ChromeDriver(chromeOptions);
            chromeDriver.Navigate().GoToUrl("https://vk.com/feed");
            List<IWebElement> webElements = chromeDriver.FindElements(By.TagName("div")).ToList();

            List<VkText> texts = new List<VkText>();
            List<VkImages> images = new List<VkImages>();
            List<VkHrefs> hrefs = new List<VkHrefs>();

            foreach (var item in webElements)
            {
                try
                {
                    if (!item.Displayed)
                        continue;
                }
                catch (Exception)
                {
                    continue;
                }
                if (item.GetAttribute("class")==null)
                    continue;
                if (!item.GetAttribute("class").Trim().Equals("feed_row"))
                    continue;
                IWebElement webElementId = item.FindElement(By.TagName("div"));
                if (webElementId == null)
                    continue;
                if (webElementId.GetAttribute("id") == null)
                    continue;

                VkText vkText = new VkText()
                {
                    Text = item.Text,
                    Id = webElementId.GetAttribute("id")
               };
                VkImages vkImages = new VkImages()
                {
                  Id = webElementId.GetAttribute("id"),
                  Images = getImages(item)
                };
                VkHrefs vkHrefs = new VkHrefs()
                {
                  Id = webElementId.GetAttribute("id"),
                  Href = getHrefs(item)
                };

                texts.Add(vkText);
                images.Add(vkImages);
                hrefs.Add(vkHrefs);
            }
            {
              Thread thread = new Thread(() => JSONWorker.setTextInJson(texts));
              thread.Start();
            }
            {
              Thread thread1 = new Thread(() => JSONWorker.setImagesInJson(images));
              thread1.Start();
            }
            {
              Thread thread2 = new Thread(() => JSONWorker.setHrefsInJson(hrefs));
              thread2.Start();
            }

            label.Dispatcher.Invoke(new Action(() => label.Text = "Парс завершён!"));
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
      setDataInLabel("text");
    }

    private void Button_Click_2(object sender, RoutedEventArgs e)
    {
      setDataInLabel("images");
    }

    private void Button_Click_3(object sender, RoutedEventArgs e)
    {
      setDataInLabel("hrefs");
    }
  }
}

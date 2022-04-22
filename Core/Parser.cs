using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using System.Text;

namespace testApp.Core
{
	class Parser
	{
		private Settings sett;
		private StringBuilder text;

		private object locker = new();

		private int count = 0;

		public Parser()
		{
			sett = new Settings();

			text = new StringBuilder();
			text.AppendLine("Регион\tПуть\tНазвание товар\tЦена\tЦена старая\tНаличие\tСсылки на картинки\tСсылка на товар");
			File.WriteAllText(sett.pathCsv, text.ToString() , Encoding.UTF32);

			var site = GetHTMLByURL(sett.url + sett.prefFirstPage);
			sett.endP = getPages(site.Result);

			Thread th = new Thread(() =>{ getItems(site.Result); });
			th.Start();
			Thread[] threads = new Thread[sett.endP - sett.stP + 1];
			for (int i = sett.stP; i <= sett.endP; i++)
			{
				site = GetHTMLByURL(sett.url + sett.pref + i);
				threads[i - sett.stP] = new Thread(() => { getItems(site.Result); });
				threads[i - sett.stP].Start();
			}
		}

		private async static Task<IDocument> GetHTMLByURL(string url)
		{
			try
			{
				Thread.Sleep(new Random().Next(1000, 2000));
				var config = Configuration.Default.WithDefaultLoader();
				var context = BrowsingContext.New(config);
				var doc = await context.OpenAsync(url);

				return doc;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
				return null;
			}
		}

		private int getPages(IDocument document)
		{
			var items = document.QuerySelectorAll("a.page-link");
			return items != null ? Int32.Parse(items.ElementAt(items.Count() - 2).TextContent) : -1;
		}

		private void getItems(IDocument document)
		{
			var items = document.QuerySelectorAll("a").Where(item => item.ClassName != null && item.ClassName == "d-block p-1 product-name gtm-click");

			foreach (var item in items)
			{
				var url = sett.url + item.GetAttribute("href");
				document = GetHTMLByURL(url).Result;
				getItem(document, url);
			}
		}

		private void getItem(IDocument document, string url)
		{
			var region = document.QuerySelectorAll("a").Where(item => item.GetAttribute("data-src") != null && item.GetAttribute("data-src") == "#region").ElementAt(0).TextContent.Trim();
			var name = document.QuerySelector("h1").TextContent;
			var menu_items = document.QuerySelectorAll("a.breadcrumb-item");
			var price = document.QuerySelector("span.price").TextContent;
			var oldPrice = document.QuerySelector("span.old-price");
			var ok = document.QuerySelector("span.ok") != null ? document.QuerySelector("span.ok").TextContent.Trim() : "Нет в наличии";
			var images_url = document.QuerySelector("div.card-slider-nav").QuerySelectorAll("img");



			List<string> menu_items_lists = new List<string>();
			List<string> images_url_list = new List<string>();

			foreach(var menu_item in menu_items)
			{
				menu_items_lists.Add(menu_item.TextContent);
			}

			foreach (var image in images_url)
			{
				images_url_list.Add(image.GetAttribute("src"));
			}

			writeLine(new string[] { region, name, price, oldPrice == null ? "Нет" : oldPrice.TextContent, ok, url}, menu_items_lists, images_url_list);
		}

		//mass.length = 6
		private void writeLine(string[] mass, List<string> menu, List<string> images)
		{
			lock (locker)
			{
				string write = $"{mass[0]}\t";
				foreach (var i in menu)
				{
					write += $"{i}>>";
				}
				write += $"{mass[1]}\t{mass[1]}\t{mass[2]}\t{mass[3]}\t{mass[4]}\t";
				foreach (var image in images)
				{
					write += $"{image}, ";
				}
				write += $"\t{mass[5]}";
				text.Clear();
				text.AppendLine(write);
				File.AppendAllText(sett.pathCsv, text.ToString(), Encoding.UTF32);

				Console.WriteLine(++count);

			}
		}

		static void Main(string[] args)
		{
			new Parser();
		}
	}
}

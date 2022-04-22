namespace testApp.Core
{
	class Settings
	{
		public string url { get; set; } = "https://www.toy.ru";
		public string pref { get; set; } = "/catalog/boy_transport/?count=30&filterseccode%5B0%5D=transport&PAGEN_8=";
		public string prefFirstPage { get; set; } = "/catalog/boy_transport/";
		public int stP { get; set; } = 2;
		public int endP { get; set; } = -1;
		public string pathCsv { get; set; } = "table.csv";
	}
}

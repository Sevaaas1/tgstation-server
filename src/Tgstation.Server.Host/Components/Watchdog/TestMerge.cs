﻿using Tgstation.Server.Api.Models.Internal;

namespace Tgstation.Server.Host.Components.Watchdog
{
	/// <summary>
	/// This model mirrors /datum/tgs_revision_information/test_merge
	/// </summary>
	sealed class TestMerge : RevisionInformation
	{
		public int Number { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public string Author { get; set; }
		public string Url { get; set; }
		public string PullRequestCommit { get; set; }
		public long TimeMerged { get; set; }
		public string Comment { get; set; }
	}
}

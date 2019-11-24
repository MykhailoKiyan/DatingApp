using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers {
	public class MessageParams {
		public int PageNumber { get; set; } = 1;

		private const int MaxPageSize = 10;

		private const int MinPageSize = 2;

		private const int DefaultPageSize = 5;

		private int pageSize = DefaultPageSize;

		public int PageSize {
			get { return this.pageSize; }
			set {
				if (value > MaxPageSize)
					this.pageSize = MaxPageSize;
				else if (value < MinPageSize)
					this.pageSize = DefaultPageSize;
				else
					this.pageSize = value;
			}
		}

		public int UserId { get; set; }

		public string MessageContainer { get; set; } = "Unread";
	}
}

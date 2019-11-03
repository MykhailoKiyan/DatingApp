﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Helpers {
	public class PagedList<T> : List<T> {
		public int CurrentPage { get; set; }

		public int TotalPages { get; set; }

		public int PageSize { get; set; }

		public int TotalCount { get; set; }

		public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize) {
			this.CurrentPage = pageNumber;
			this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);
			this.PageSize = pageSize;
			this.TotalCount = count;
			this.AddRange(items);
		}

		public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize) {
			var count = source.CountAsync();
			var numberToSkip = (pageNumber - 1) * pageSize;
			var items = source.Skip(numberToSkip).Take(pageSize).ToListAsync();
			Task.WaitAll(count, items);
			return new PagedList<T>(items.Result, count.Result, pageNumber, pageSize);
		}
	}
}

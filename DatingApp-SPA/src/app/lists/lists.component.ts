import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { User } from '../_models/user';
import { Pagination, PaginatedResult } from './../_models/pagination';
import { AlertifyService } from './../_services/alertify.service';
import { UserService } from './../_services/user.service';
import { AuthService } from './../_services/auth.service';

@Component({
	selector: 'app-lists',
	templateUrl: './lists.component.html',
	styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {
	users: User[];
	pagination: Pagination;
	likesParam: string;

	constructor(
			private authServics: AuthService,
			private userService: UserService,
			private alertify: AlertifyService,
			private route: ActivatedRoute) { }

	ngOnInit() {
		this.route.data.subscribe(data => {
			this.users = data['users'].result;
			this.pagination = data['users'].pagination;
		});
		this.likesParam = 'Likers';
	}

	pageChanged(event: any): void {
		this.pagination.currentPage = event.page;
		this.loadUsers();
	}

	loadUsers(): void {
		this.userService.getUsers(
				this.pagination.currentPage,
				this.pagination.itemsPerPage,
				null,
				this.likesParam)
			.subscribe(
				(response: PaginatedResult<User[]>) => {
					this.pagination = response.pagination;
					this.users = response.result;
				},
				(error: any) => {
					this.alertify.error(error);
				}
			);
	}

	getMasterTitle(): string {
		return `${this.likesParam === 'Likers' ? 'Member who like me' : 'Members who I\'ve liked'} :
			${this.pagination.totalItems}`;
	}
}

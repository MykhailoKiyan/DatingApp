import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { User } from '../../_models/user';
import { Pagination, PaginatedResult } from '../../_models/pagination';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';

@Component({
	selector: 'app-member-list',
	templateUrl: './member-list.component.html',
	styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
	users: User[];
	user: User;
	pagination: Pagination;
	genderList = [{	value: 'male', display: "Males" },
	 { value: 'female', display: "Females" }];
	userParams: any = {};

	constructor(
			private userService: UserService,
			private alertify: AlertifyService,
			private route: ActivatedRoute) {
				this.user = JSON.parse(localStorage.getItem('user'));
			}

	ngOnInit() {
		this.route.data.subscribe(data => {
			this.users = data['users'].result;
			this.pagination = data['users'].pagination;
		});

		this.resetFilter(false);
	}

	resetFilter(doLoadUsers: boolean = true): void {
		this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
		this.userParams.minAge = null;
		this.userParams.maxAge = null;
		this.userParams.orderBy = 'lastActive';
		if (doLoadUsers) {
			this.loadUsers();
		}
	}

	pageChanged(event: any): void {
		this.pagination.currentPage = event.page;
		this.loadUsers();
	}

	loadUsers(): void {
		this.userService.getUsers(
				this.pagination.currentPage,
				this.pagination.itemsPerPage,
				this.userParams)
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
}

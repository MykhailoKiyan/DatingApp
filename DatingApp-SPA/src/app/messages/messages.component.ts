import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { Message } from '../_models/message';
import { Pagination, PaginatedResult } from '../_models/pagination';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';

@Component({
	selector: 'app-messages',
	templateUrl: './messages.component.html',
	styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit {
	messages: Message[];
	pagination: Pagination;
	messageContainer = 'Unread';

	constructor(
			private userService: UserService,
			private authService: AuthService,
			private route: ActivatedRoute,
			private alertify: AlertifyService) { }

	ngOnInit() {
		this.route.data.subscribe(d => {
			const data = d['messages'];
			this.messages = data.result;
			this.pagination = data.pagination;
		});
	}

	loadMessages() {
		const userId = this.authService.decodedToken.nameid;
		this.userService.getMessages(userId, this.pagination.currentPage, this.pagination.itemsPerPage,
				this.messageContainer)
			.subscribe(
				(res: PaginatedResult<Message[]>) => {
					this.messages = res.result;
					this.pagination = res.pagination;
				},
				error => {
					this.alertify.error(error);
				});
	}

	deleteMesage(id: number) {
		this.alertify.confirm('Are you sure want to delete this message?', () => {
				const userId = this.authService.decodedToken.nameid;
				this.userService.deleteMessage(id, userId)
					.subscribe(() => {
							const index: number = this.messages.findIndex(m => m.id === id);
							this.messages.splice(index, 1);
							this.alertify.success('Message has been deleted!');
						}, error => {
							this.alertify.error(error);
						});
			});
	}

	pageChanged(event: any): void {
		this.pagination.currentPage = event.page;
		this.loadMessages();
	}
}

import { Component, OnInit, Input } from '@angular/core';

import { AlertifyService } from 'src/app/_services/alertify.service';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { Message } from '../../_models/message';
import { tap } from 'rxjs/operators';
import { currentId } from 'async_hooks';

@Component({
	selector: 'app-member-messages',
	templateUrl: './member-messages.component.html',
	styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
	@Input() recipientId: number;
	messages: Message[];
	newMessage: any = {};

	constructor(
			private userService: UserService,
			private authService: AuthService,
			private alertify: AlertifyService) { }

	ngOnInit() {
		this.loadMessages();
	}

	loadMessages() {
		const userId = +this.authService.decodedToken.nameid;
		return this.userService.getMessageThread(userId, this.recipientId)
			.pipe(tap(messages => {
				for (let i = 0; i < messages.length; i++) {
					if (!messages[i].isRead && messages[i].recipientId === userId) {
						this.userService.markAsRead(userId, messages[i].id)
							.subscribe(() => {}, error => {
								this.alertify.error(error);
							});
					}
				}
			}))
			.subscribe(messages => {
					this.messages = messages;
				}, error => {
					this.alertify.error(error);
				});
	}

	sendMessage() {
		this.newMessage.recipientId = this.recipientId;
		const userId = this.authService.decodedToken.nameid;
		this.userService.sendMessage(userId, this.newMessage)
			.subscribe(
				(response: Message) => {
					this.messages.unshift(response);
					this.newMessage.content = '';
				}, error => {
					this.alertify.error(error);
				}
			);
	}
}

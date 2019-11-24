import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { Message } from '../_models/message';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';


@Injectable()
export class MessagesResolver implements Resolve<Message[]> {
	pageNumber = 1;
	pageSize = 3;
	messageContainer = 'Unread';

	constructor(
		private userService: UserService,
		private authService: AuthService,
		private router: Router,
		private alertify: AlertifyService) { }

	resolve(route: ActivatedRouteSnapshot): Observable<Message[]> {
		const name = this.authService.decodedToken.nameid;
		const users: Observable<Message[]> = this.userService.getMessages(name, this.pageNumber, this.pageSize,
				this.messageContainer)
			.pipe(catchError(error => {
				this.alertify.error('Problem retrieving data');
				this.router.navigate(['/home']);
				return of(null);
			}));

		return users;
	}
}

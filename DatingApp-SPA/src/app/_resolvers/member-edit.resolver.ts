import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

import { User } from '../_models/user';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MemberEditResolver implements Resolve<User> {
	constructor(
		private userService: UserService,
		private authService: AuthService,
		private router: Router,
		private alertify: AlertifyService) { }

	resolve(route: ActivatedRouteSnapshot): Observable<User> {
		const id: number = +this.authService.decodedToken.nameid;
		const user: Observable<User> = this.userService.getUser(id)
			.pipe(catchError(error => {
				this.alertify.error('Problem retrieving data - Member Edit resolver');
				this.router.navigate(['/members']);
				return of(null);
			}));
		return user;
	}
}

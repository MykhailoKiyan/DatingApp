import { Injectable } from '@angular/core';
import { CanActivate, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';

@Injectable({
	providedIn: 'root'
})
export class AuthGuard implements CanActivate {
	constructor(private authService: AuthService, private router: Router, private alerty: AlertifyService) { }

	canActivate(next: ActivatedRouteSnapshot): boolean {
		const roles = next.firstChild.data['roles'] as string[];
		if (this.authService.roleMatch(roles)) {
			return true;
		}
		if (roles) {
			const match = this.authService.roleMatch(roles);
			if (match) {
				return true;
			} else {
				this.router.navigate(['members']);
				this.alerty.error('You are not authorized to access this area.');
			}

		}

		this.alerty.error('You shall not pass!!!');
		this.router.navigate(['/home']);
		return false;
	}
}

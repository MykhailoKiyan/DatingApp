import { Component, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';
import { AlertifyService } from '../_services/alertify.service';
import { Router } from '@angular/router';

@Component({
	selector: 'app-nav',
	templateUrl: './nav.component.html',
	styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
	model: any = {};
	photoUrl: string;

	constructor(public authService: AuthService, private alertify: AlertifyService, private router: Router) { }

	ngOnInit() {
		this.authService.currentPhotoUrl.subscribe(url => this.photoUrl = url);
	}

	login() {
		this.authService.login(this.model).subscribe(
			// next
			() => {
				this.alertify.success('Logged is successfully');
			},
			// error
			err => {
				this.alertify.error(err);
			},
			// complete
			() => {
				this.router.navigate(['/members']);
			}
		);
	}

	loggedIn() {
		return this.authService.loggedIn();
	}

	logout() {
		localStorage.removeItem('token');
		this.authService.decodedToken = null;
		localStorage.removeItem('user');
		this.authService.currentUser = null;
		this.alertify.message('Logged out');
		this.router.navigate(['/home']);
	}
}

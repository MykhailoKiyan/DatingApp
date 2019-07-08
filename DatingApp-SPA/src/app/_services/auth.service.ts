import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../environments/environment';
import { User } from '../_moduls/user';


@Injectable({
	providedIn: 'root'
})
export class AuthService {
	baseUrl = environment.baseUrl + 'auth/';
	decodedToken: any;
	jwtHelper = new JwtHelperService();
	currentUser: User;
	photoUrl: BehaviorSubject<string> = new BehaviorSubject<string>('../../assets/user.png');
	currentPhotoUrl: Observable<string> = this.photoUrl.asObservable();

	constructor(private http: HttpClient) { }

	changeMemberPhoto(url: string) {
		this.photoUrl.next(url);
	}

	login(model: any) {
		return this.http.post(this.baseUrl + 'login', model).pipe(map(
			(response: any) => {
				const res = response;
				if (res) {
					localStorage.setItem('token', res.token);
					localStorage.setItem('user', JSON.stringify(res.user));
					this.decodedToken = this.jwtHelper.decodeToken(res.token);
					this.currentUser = res.user;
					this.changeMemberPhoto(this.currentUser.photoUrl);
				}
			})
		);
	}

	register(model: any) {
		return this.http.post(this.baseUrl + 'register', model);
	}

	loggedIn() {
		const token = localStorage.getItem('token');
		return !this.jwtHelper.isTokenExpired(token);
	}
}

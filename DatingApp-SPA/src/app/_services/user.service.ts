import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_moduls/user';

@Injectable({
	providedIn: 'root'
})
export class UserService {
	baseUrl: string = environment.baseUrl;

	constructor(private http: HttpClient) { }

	/**
	 * Getting an array of all Users from the Server
	 * @returns Observable {Observable<User[]>} Observable of Users
	 */
	getUsers(): Observable<User[]> {
		return this.http.get<User[]>(this.baseUrl + 'users');
	}

	getUser(id: number): Observable<User> {
		return this.http.get<User>(this.baseUrl + 'users/' + id);
	}

	updateUser(id: number, user: User) {
		return this.http.put<User>(this.baseUrl + 'users/' + id, user);
	}

	setMainPhoto(userId: number, photoId: number) {
		return this.http.post(this.baseUrl + 'users/' + userId + '/photos/' + photoId + '/setMain', {});
	}

	deletePhoto(userId: number, photoId: number) {
		return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + photoId);
	}
}

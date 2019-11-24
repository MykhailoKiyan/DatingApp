import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { User } from '../_models/user';
import { Message } from '../_models/message';
import { PaginatedResult } from '../_models/pagination';

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
	getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
		const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();
		let params = new HttpParams();
		if (page != null && itemsPerPage != null) {
			params = params.append('pageNumber', page);
			params = params.append('pageSize', itemsPerPage);
		}

		if (userParams != null) {
			if (userParams.minAge != null) {
				params = params.append('minAge', userParams.minAge);
			}

			if (userParams.maxAge != null) {
				params = params.append('maxAge', userParams.maxAge);
			}

			if (userParams.gender != null) {
				params = params.append('gender', userParams.gender);
			}

			if (userParams.orderBy != null) {
				params = params.append('orderBy', userParams.orderBy);
			}
		}

		switch (likesParam) {
			case "Likers":
				params = params.append('likers', 'true');
				break;

				case "Likees":
				params = params.append('likees', 'true');
				break;
		}

		return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params })
				.pipe(
					map(response => {
						paginatedResult.result = response.body;
						const paginationResponseHeaders = response.headers.get('Pagination');
						if (paginationResponseHeaders) {
							paginatedResult.pagination = JSON.parse(paginationResponseHeaders);
						}

						return paginatedResult;
				})
			);
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

	sendLike(id: number, recipient: number) {
		return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipient, {});
	}

	getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
		const paginatiedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();
		let params = new HttpParams();
		params = params.append('MessageContainer', messageContainer);
		if (page != null && itemsPerPage != null) {
			params = params.append('pageNumber', page);
			params = params.append('pageSize', itemsPerPage);
		}
		return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', { observe: 'response', params })
			.pipe(map(response => {
				paginatiedResult.result = response.body;
				const paginationResponseHeaders = response.headers.get('Pagination');
				if (paginationResponseHeaders) {
					paginatiedResult.pagination = JSON.parse(paginationResponseHeaders);
				}
				return paginatiedResult;
			}));
	}

	getMessageThread(id: number, recipientId: number) {
		return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages/thread/' + recipientId);
	}

	sendMessage(id: number, message: Message){
		return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
	}

	deleteMessage(id: number, userId: number) {
		return this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + id, {});
	}

	markAsRead(userId: number, messageId: number) {
		return this.http.post(this.baseUrl + 'users/' + userId + '/messages/' + messageId + '/read', {});
	}
}

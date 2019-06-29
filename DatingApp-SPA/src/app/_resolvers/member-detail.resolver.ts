import { Injectable } from "@angular/core";
import { Resolve, Router, ActivatedRouteSnapshot } from "@angular/router";
import { Observable, of } from "rxjs";
import { catchError } from "rxjs/operators";

import { User } from "../_moduls/user";
import { UserService } from "../_services/user.service";
import { AlertifyService } from "../_services/alertify.service";

@Injectable()
export class MemberDetailResolver implements Resolve<User> {
  constructor(
    private userService: UserService,
    private router: Router,
    private alertify: AlertifyService) { }

  resolve(route: ActivatedRouteSnapshot): Observable<User> {
    let id: number = +route.params['id'];
    let user: Observable<User> = this.userService.getUser(id)
      .pipe(catchError(error => {
        this.alertify.error('Problem retrieving data - Member Detail resolver');
        this.router.navigate(['/members']);
        return of(null);
      }));
    return user;
  }
}

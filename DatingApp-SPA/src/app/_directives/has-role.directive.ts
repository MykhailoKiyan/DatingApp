import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
	selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
	@Input() appHasRole: string[];
	isVivible = false;

	constructor(
			private container: ViewContainerRef,
			private template: TemplateRef<any>,
			private auth: AuthService
	) { }

	ngOnInit(): void {
		const roles = this.auth.decodedToken.roles as string[];
		// if no roles then clear the View Container
		if (!roles) {
			this.container.clear();
		}

		// if the current user has role then need render the element
		if (this.auth.roleMatch(this.appHasRole)) {
			if(!this.isVivible) {
				this.isVivible = true;
				this.container.createEmbeddedView(this.template);
			} else {
				this.isVivible = false;
				this.container.clear();
			}
		}
	}

}

import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { Auth } from "./auth";
export const authGuard: CanActivateFn = (route) => {
  const auth = inject(Auth),
    router = inject(Router);
  if (!auth.estaAutenticado()) return router.createUrlTree(["/login"]);
  const roles = route.data["roles"] as string[];
  return !roles || roles.some((r) => auth.tieneRol(r))
    ? true
    : router.createUrlTree(["/"]);
};

import { inject } from "@angular/core";
import { HttpInterceptorFn } from "@angular/common/http";
import { catchError, throwError, timeout } from "rxjs";
import { Auth } from "./auth";
import { config } from "./config";
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const url = new URL(req.url, location.origin),
    base = new URL(config.apiUrl, location.origin);
  if (
    url.origin !== base.origin ||
    !url.pathname.startsWith(base.pathname + "/")
  )
    return next(req);
  const auth = inject(Auth),
    token = auth.getToken();
  const request = token
    ? req.clone({ setHeaders: { Authorization: "Bearer " + token } })
    : req;
  return next(request).pipe(
    timeout(20000),
    catchError((error) => {
      if (error.status === 401 && token && auth.getToken() === token)
        auth.logout();
      return throwError(() => error);
    }),
  );
};

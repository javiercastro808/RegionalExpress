import { API_URL } from "./api-url";
import {
  HttpInterceptorFn
} from '@angular/common/http';

export const authInterceptor:
  HttpInterceptorFn =
  (req, next) => {

    const token =
      localStorage.getItem(
        'regionalExpressToken'
      );

    if (!token || !(req.url === API_URL || req.url.startsWith(API_URL + "/"))) {
      return next(req);
    }


    const request =
      req.clone({
        setHeaders: {
          Authorization:
            `Bearer ${token}`
        }
      });


    return next(request);
  };
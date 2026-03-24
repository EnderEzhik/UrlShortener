import { hasAuthToken } from "./common.js";

if (!hasAuthToken()) {
    window.location.pathname = "/login";
}
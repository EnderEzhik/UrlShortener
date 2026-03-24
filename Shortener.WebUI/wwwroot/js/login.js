import { apiServerAddress } from "./config.js";
import { hasAuthToken } from "./common.js";

const form = document.getElementById("login-form");
const loginInput = document.getElementById("login-identifier");
const passwordInput = document.getElementById("login-password");
const rememberCheckbox = document.getElementById("remember-me");

form.addEventListener("submit", async function (event) {
    event.preventDefault();
    
    form.classList.add("was-validated");
    if (!form.checkValidity()) return;
    
    const login = loginInput.value;
    const password = passwordInput.value;
    
    try {
        const response = await fetch(apiServerAddress + "/login", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                login: login,
                password: password
            })
        });
        
        if (!response.ok) {
            console.error(response);
            return;
        }

        const data = await response.json();
        
        if (!data.token) {
            console.error(data);
            return;
        }
        
        if (rememberCheckbox.checked) {
            localStorage.setItem("token", JSON.stringify(data));
            sessionStorage.removeItem("token");
        }
        else {
            sessionStorage.setItem("token", JSON.stringify(data));
            localStorage.removeItem("token");
        }
        window.location.pathname = "";
    }
    catch (error) {
        console.error(error);
    }
});

document.addEventListener("DOMContentLoaded", () => {
    if (hasAuthToken()) {
        window.location.pathname = "";
    }
});
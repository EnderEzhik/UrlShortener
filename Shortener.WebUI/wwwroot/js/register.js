import { hasAuthToken } from "./common.js";
import {apiServerAddress} from "./config.js";

const form = document.getElementById("registration-form");
const loginInput = document.getElementById("register-login");
const passwordInput = document.getElementById("register-password");
const passwordConfirmInput = document.getElementById("register-password-confirm");

form.addEventListener("submit", async (event) => {
    event.preventDefault();

    form.classList.add("was-validated");
    if (!form.checkValidity()) return;
    
    const login = loginInput.value;
    const password = passwordInput.value;
    const passwordConfirm = passwordConfirmInput.value;
    
    if (password !== passwordConfirm) {
        alert("Пароли должны совпадать");
        return;
    }

    try {
        const response = await fetch(apiServerAddress + "/register", {
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
        
        localStorage.setItem("token", JSON.stringify(data));
        window.location.pathname = "";
    }
    catch (error) {
        console.error(error);
    }
})

document.addEventListener("DOMContentLoaded", () => {
    if (hasAuthToken()) {
        window.location.pathname = "";
    }
});
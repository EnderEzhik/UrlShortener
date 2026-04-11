import { hasAuthToken } from "./common.js";
if (hasAuthToken()) {
    window.location.pathname = "";
}

import { apiServerAddress } from "./config.js";

const form = document.getElementById("login-form");
const loginInput = document.getElementById("login-identifier");
const passwordInput = document.getElementById("login-password");
const rememberCheckbox = document.getElementById("remember-me");
const errorBox = document.getElementById("form-error");

function clearError() {
    if (!errorBox) return;
    errorBox.textContent = "";
    errorBox.classList.add("d-none");
}

function showError(message) {
    if (!errorBox) return;
    errorBox.textContent = message || "Произошла ошибка. Попробуйте еще раз.";
    errorBox.classList.remove("d-none");
}

async function getResponseErrorMessage(response) {
    let message = `Ошибка сервера (${response.status}).`;

    const data = await response.json();
    const extracted = data.error?.message;
    if (extracted) message = extracted;

    return message;
}

form.addEventListener("submit", async function (event) {
    event.preventDefault();
    clearError();

    form.classList.add("was-validated");
    if (!form.checkValidity()) return;

    const login = loginInput.value;
    const password = passwordInput.value;

    try {
        const response = await fetch(apiServerAddress + "/auth/login", {
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
            if (response.status === 401) {
                showError("Неверный логин или пароль");
            }
            else {
                const serverMessage = await getResponseErrorMessage(response);
                showError(serverMessage);
            }
            return;
        }

        const data = await response.json();

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
        showError("Не удалось отправить запрос. Проверьте соединение и попробуйте снова.");
    }
});

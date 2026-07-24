import { hasAuthToken } from "./common.js";
if (hasAuthToken()) {
    window.location.pathname = "";
}

const form = document.getElementById("registration-form");
const loginInput = document.getElementById("register-login");
const passwordInput = document.getElementById("register-password");
const passwordConfirmInput = document.getElementById("register-password-confirm");
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

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    clearError();

    form.classList.add("was-validated");
    if (!form.checkValidity()) return;

    const login = loginInput.value;
    const password = passwordInput.value;
    const passwordConfirm = passwordConfirmInput.value;

    if (password !== passwordConfirm) {
        showError("Пароли должны совпадать.");
        passwordConfirmInput?.focus?.();
        return;
    }

    try {
        const response = await fetch("/api/auth/register", {
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
            if (response.status === 409) {
                showError("Этот логин уже занят");
            }
            else {
                const serverMessage = await getResponseErrorMessage(response);
                showError(serverMessage);
            }
            return;
        }

        const data = await response.json();

        localStorage.setItem("token", JSON.stringify(data));

        window.location.pathname = "";
    }
    catch (error) {
        console.error(error);
        showError("Не удалось отправить запрос. Проверьте соединение и попробуйте снова.");
    }
});

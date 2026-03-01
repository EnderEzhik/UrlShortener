import { hasAuthToken } from "./common.js";

function clearJwt() {
    localStorage.removeItem("token");
    sessionStorage.removeItem("token");
}

function applyAuthUiState() {
    const hasToken = hasAuthToken();
    const authButtons = document.querySelector("[data-auth-buttons]");
    const profileMenu = document.querySelector("[data-profile-menu]");

    if (!authButtons || !profileMenu) {
        return;
    }

    if (hasToken) {
        authButtons.classList.add("d-none");
        profileMenu.classList.remove("d-none");
    } else {
        authButtons.classList.remove("d-none");
        profileMenu.classList.add("d-none");
    }
}

document.addEventListener("DOMContentLoaded", () => {
    applyAuthUiState();

    const logoutBtn = document.querySelector("[data-logout-btn]");
    if (logoutBtn) {
        logoutBtn.addEventListener("click", () => {
            clearJwt();
            applyAuthUiState();
            window.location.href = "/login";
        });
    }
});


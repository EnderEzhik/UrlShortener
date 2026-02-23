function isJwtToken(value) {
    const parts = value.split(".");
    return parts.length === 3 && parts.every(p => p.length > 0);
}

function hasAuthTokenCookie() {
    if (!document.cookie) {
        return false;
    }

    return document.cookie.split(";").some(cookieStr => {
        const parts = cookieStr.split("=");
        if (parts.length < 2) {
            return false;
        }

        const rawValue = parts.slice(1).join("=").trim();
        if (!rawValue) {
            return false;
        }

        try {
            const decoded = decodeURIComponent(rawValue);
            return isJwtToken(decoded);
        } catch {
            return isJwtToken(rawValue);
        }
    });
}

function clearJwtCookies() {
    if (!document.cookie) {
        return;
    }

    const cookies = document.cookie.split(";");

    cookies.forEach(cookieStr => {
        const parts = cookieStr.split("=");
        if (parts.length < 2) {
            return;
        }

        const name = parts[0].trim();
        const rawValue = parts.slice(1).join("=").trim();

        if (!name || !rawValue) {
            return;
        }

        let value = rawValue;
        try {
            value = decodeURIComponent(rawValue);
        } catch {
            // ignore
        }

        if (isJwtToken(value)) {
            document.cookie = `${name}=; Max-Age=0; path=/;`;
        }
    });
}

function applyAuthUiState() {
    const hasToken = hasAuthTokenCookie();
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
            clearJwtCookies();
            applyAuthUiState();
            window.location.href = "/login";
        });
    }
});


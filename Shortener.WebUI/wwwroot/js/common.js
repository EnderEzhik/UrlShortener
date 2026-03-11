function formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function buildShortUrl(shortCode) {
    return window.location.origin + "/" + shortCode;
}

function checkTokenExpiration(jwtToken) {
    const tokenParts = jwtToken.split(".");

    if (tokenParts.length !== 3) {
        console.warn("Неверный формат JWT токена");
        return false;
    }

    const payload = JSON.parse(atob(tokenParts[1]));

    const currentTime = Math.floor(Date.now() / 1000);

    if (payload.exp < currentTime) {
        console.log("Срок действия токена истек");
        return false;
    }

    console.log("Токен действителен");
    return true;
}

function hasAuthToken() {
    let tokenData = localStorage.getItem("token");
    if (tokenData) {
        const token = JSON.parse(tokenData).token;
        if (!checkTokenExpiration(token)) {
            localStorage.removeItem("token");
            return false;
        }
        return true;
    }
    tokenData = sessionStorage.getItem("token");
    if (tokenData) {
        const token = JSON.parse(tokenData).token;
        if (!checkTokenExpiration(token)) {
            sessionStorage.removeItem("token");
            return false;
        }
        return true;
    }
    return false;
}

function getAuthToken() {
    let token = localStorage.getItem("token");
    if (token) {
        return JSON.parse(token).token;
    }
    token = sessionStorage.getItem("token");
    if (token) {
        return JSON.parse(token).token;
    }
    return null;
}

export { formatDate, buildShortUrl, hasAuthToken, getAuthToken };

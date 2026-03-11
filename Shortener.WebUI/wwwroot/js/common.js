function formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function buildShortUrl(shortCode) {
    return window.location.origin + "/" + shortCode;
}

function hasAuthToken() {
    if (localStorage.getItem("token")) {
        return true;
    }
    if (sessionStorage.getItem("token")) {
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

export {formatDate, buildShortUrl, hasAuthToken, getAuthToken };

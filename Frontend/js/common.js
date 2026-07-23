function formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function checkTokenExpires(tokenExpires) {
    if (tokenExpires < new Date()) {
        console.log("Срок действия токена истек");
        return false;
    }
    console.log("Токен действителен");
    return true;
}

function hasAuthToken() {
    let tokenData = localStorage.getItem("token");
    if (tokenData) {
        const decodedTokenData = JSON.parse(tokenData);
        const tokenExpires = new Date(decodedTokenData.expires);
        if (!checkTokenExpires(tokenExpires)) {
            localStorage.removeItem("token");
            return false;
        }
        return true;
    }
    tokenData = sessionStorage.getItem("token");
    if (tokenData) {
        const decodedTokenData = JSON.parse(tokenData);
        const tokenExpires = new Date(decodedTokenData.expires);
        if (!checkTokenExpires(tokenExpires)) {
            sessionStorage.removeItem("token");
            return false;
        }
        return true;
    }
    return false;
}

function getAuthToken() {
    let tokenData = localStorage.getItem("token");
    if (tokenData) {
        return JSON.parse(tokenData).token;
    }
    tokenData = sessionStorage.getItem("token");
    if (tokenData) {
        return JSON.parse(tokenData).token;
    }
    return null;
}

export { formatDate, hasAuthToken, getAuthToken };

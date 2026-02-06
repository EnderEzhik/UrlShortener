function formatDate(value) {
    if (!value) return "—";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "—";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function buildShortUrl(shortCode) {
    return window.location.origin + "/" + shortCode;
}

export {formatDate, buildShortUrl};

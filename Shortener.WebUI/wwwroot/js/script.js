const form = document.getElementById("shortener-form");
const originalUrlInput = document.getElementById("original-url");
const expiryInput = document.getElementById("expiry-date");
const shortUrlInput = document.getElementById("short-url");
const copyBtn = document.getElementById("copy-btn");
const resetBtn = document.getElementById("reset-btn");
const meta = document.getElementById("meta");

const toastEl = document.getElementById("app-toast");
const toastBody = document.getElementById("toast-body");
const toast = new bootstrap.Toast(toastEl, { delay: 2200 });

const apiServerAddress = "https://localhost:7000/api";

function showToast(message) {
    toastBody.textContent = message;
    toast.show();
}

async function createShortUrl(originalUrl, expiresDatetime) {
    const response = await fetch(`${apiServerAddress}/links`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({
            url: originalUrl,
            expiresAt: expiresDatetime
        })
    });
    const data = await response.json();
    console.log(data);
    return data;
}

function buildShortUrl(shortCode) {
    return `${apiServerAddress}/` + shortCode;
}

function formatExpiry(value) {
    if (!value) return "";
    const d = new Date(value);
    if (Number.isNaN(d.getTime())) return "";
    return d.toLocaleString("ru-RU", { dateStyle: "medium", timeStyle: "short" });
}

function removeMilliseconds(dateTime) {
    const dateTimeString = dateTime.toISOString();
    const dateTimeSplited = dateTimeString.split(".");
    const dateTimeStringWithoutMilliseconds = dateTimeSplited[0] + "Z";
    return dateTimeStringWithoutMilliseconds;
}

async function copyToClipboard(text) {
    if (navigator.clipboard && window.isSecureContext) {
        await navigator.clipboard.writeText(text);
        return;
    }

    const tmp = document.createElement("textarea");
    tmp.value = text;
    tmp.setAttribute("readonly", "");
    tmp.style.position = "absolute";
    tmp.style.left = "-9999px";
    document.body.appendChild(tmp);
    tmp.select();
    document.execCommand("copy");
    document.body.removeChild(tmp);
}

function setResult(shortUrl, expiryValue) {
    shortUrlInput.value = shortUrl;
    copyBtn.disabled = !shortUrl;
    meta.textContent = expiryValue ? "Истекает: " + formatExpiry(expiryValue) : "Без даты истечения";
}

function reset() {
    form.classList.remove("was-validated");
    originalUrlInput.value = "";
    expiryInput.value = "";
    shortUrlInput.value = "";
    copyBtn.disabled = true;
    meta.textContent = "";
}

resetBtn.addEventListener("click", () => {
    reset();
    showToast("Сброшено");
    originalUrlInput.focus();
});

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    e.stopPropagation();

    form.classList.add("was-validated");
    if (!form.checkValidity()) return;
    
    const originalUrl = originalUrlInput.value;
    
    let expiresDatetime = null;
    if (expiryInput.value) {
        const expiryDate = new Date(expiryInput.value);
        // expiryDate.setMilliseconds(0);
        // expiresDatetime = expiryDate.toISOString();
        expiresDatetime = removeMilliseconds(expiryDate);
    }

    try {
        const data = await createShortUrl(originalUrl, expiresDatetime);
        
        const shortUrl = buildShortUrl(data["shortCode"]);

        setResult(shortUrl, data["expiresAt"]);
        showToast("Короткая ссылка создана");
    }
    catch (error) {
        showToast("Не удалось создать короткую ссылку");
    }
});

copyBtn.addEventListener("click", async () => {
    const value = shortUrlInput.value.trim();
    if (!value) return;
    try {
        await copyToClipboard(value);
        showToast("Скопировано в буфер обмена");
    } catch {
        showToast("Не удалось скопировать");
    }
});
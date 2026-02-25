import {apiServerAddress} from "./config.js";
import {formatDate, buildShortUrl} from "./common.js";

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

function showToast(message) {
    toastBody.textContent = message;
    toast.show();
}

function setResult(shortUrl, expiryValue) {
    shortUrlInput.value = shortUrl;
    copyBtn.disabled = !shortUrl;
    meta.textContent = expiryValue ? "Истекает: " + formatDate(expiryValue) : "Без даты истечения";
}

function reset() {
    form.classList.remove("was-validated");
    originalUrlInput.value = "";
    expiryInput.value = "";
    shortUrlInput.value = "";
    copyBtn.disabled = true;
    meta.textContent = "";
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
    return await response.json();
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
        expiresDatetime = removeMilliseconds(expiryDate);
    }

    try {
        const data = await createShortUrl(originalUrl, expiresDatetime);
        
        const shortUrl = buildShortUrl(data["shortCode"]);

        setResult(shortUrl, data["expiresAt"]);
        showToast("Короткая ссылка создана");
    }
    catch (err) {
        console.log(err);
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